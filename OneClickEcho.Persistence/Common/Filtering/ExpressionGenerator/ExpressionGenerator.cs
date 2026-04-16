using System.Linq.Expressions;
using System.Reflection;
using OneClickEcho.Domain.ApplicationUserAggregate.Entities;
using OneClickEcho.Domain.CampaignAggregate.Enums;
using OneClickEcho.Domain.Common.Primitives;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;
using OneClickEcho.Persistence.Common.Filtering.Lexer.Enums;
using OneClickEcho.Persistence.Common.Filtering.Lexer.Tokens;
using OneClickEcho.Persistence.Common.Filtering.Lexer.Tokens.Interfaces;
using OneClickEcho.Persistence.Common.Filtering.Parser.Nodes;

namespace OneClickEcho.Persistence.Common.Filtering.ExpressionGenerator;

public class ExpressionGenerator
{
    public static Expression<Func<TEntity, bool>> Generate<TEntity>(ExpressionNode node)
    {
        ParameterExpression parameter = Expression.Parameter(typeof(TEntity), "x");
        Expression body = BuildExpression(node, parameter);
        Expression<Func<TEntity, bool>> x = Expression.Lambda<Func<TEntity, bool>>(body, parameter);
        return x;
    }

    private static Expression BuildExpression(ExpressionNode node, ParameterExpression parameter)
    {
        if (node is ValueNode valueNode)
        {
            if (valueNode.Variable.Value == "CompanyIds")
            {
                ParameterExpression paramC = Expression.Parameter(typeof(ApplicationUserCompany), "c");

                return Expression.Call(
                    typeof(Enumerable),
                    "Any",
                    [typeof(ApplicationUserCompany)],
                    Expression.Property(parameter, "CompanyIds"),
                    Expression.Lambda<Func<ApplicationUserCompany, bool>>(
                        Expression.Equal(
                            Expression.Property(paramC, "CompanyId"),
                            Expression.Constant(CompanyId.Create(new Guid(valueNode.Constant.Value)))
                        ),
                        paramC
                    )
                );
            }

            PropertyInfo? propInfo = parameter.Type.GetProperty(
                valueNode.Variable.Value,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            if (propInfo is null && valueNode.Variable.Value.Equals("Id", StringComparison.OrdinalIgnoreCase))
            {
                propInfo = parameter.Type
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .FirstOrDefault(p =>
                        p.Name == "Id" &&
                        p.PropertyType == parameter.Type.BaseType?.GetGenericArguments().FirstOrDefault());
            }

            if (propInfo is null)
            {
                throw new ArgumentException(
                    $"Filter property '{valueNode.Variable.Value}' was not found on type {parameter.Type.Name}.");
            }

            MemberExpression member = Expression.Property(parameter, propInfo);

            ConstantExpression constant;

            Type listType = typeof(int);

            if (valueNode.Constant is IFilterTokenConstant filterTokenConstant)
            {
                constant = Expression.Constant(filterTokenConstant.Value);

                if (member.Type.IsEnum)
                {
                    if (valueNode.Constant is not FilterTokenConstant<int> filterTokenConstantInt)
                    {
                        throw new InvalidCastException($"The constant must be int when comparing with enums.");
                    }

                    constant = Expression.Constant(Enum.ToObject(member.Type, filterTokenConstantInt.ConstantValue));
                }
                else if (member.Type == typeof(bool))
                {
                    constant = Expression.Constant(ParseBoolFilterConstant(valueNode.Constant));
                }
                else if (member.Type.IsSubclassOf(typeof(AggregateRootId<Guid>)))
                {
                    constant = Expression.Constant(
                        member.Type
                            .GetMethod(
                            "Create",
                            [typeof(Guid)])!
                            .Invoke(null, [Guid.Parse(valueNode.Constant.Value)]));
                }
            }
            else if (valueNode.Constant is FilterTokenConstantList filterTokenConstantList)
            {
                // TODO: FIX THIS GARBAGE!!!
                listType = filterTokenConstantList.ConstantValueList.GetType().GetGenericArguments()[0];
                
                List<CampaignStatus> campaignStatusList = new List<CampaignStatus>();
                foreach (object? listItem in filterTokenConstantList.ConstantValueList)
                {
                    string? text = listItem?.ToString();
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        throw new InvalidOperationException("Campaign status filter list item is null or empty.");
                    }

                    campaignStatusList.Add((CampaignStatus)short.Parse(text));
                }
                
                constant = Expression.Constant(campaignStatusList);
                return Expression.Call(constant, typeof(List<CampaignStatus>).GetMethod("Contains", new[] { typeof(CampaignStatus) })!,
                    member);
                // constant = listType == typeof(Int32)
                //     ? Expression.Constant((List<Int32>)filterTokenConstantList.ConstantValueList)
                //     : listType == typeof(Guid)
                //         ? Expression.Constant((List<Guid>)filterTokenConstantList.ConstantValueList)
                //         : throw new Exception("Unsupported constant list type");
            }
            else
            {
                throw new Exception("Invalid value node");
            }

            return valueNode.Operator is not FilterTokenOperation operatorToken
                ? throw new Exception("Invalid operator")
                : operatorToken.CurrentFilterTokenOperationType switch
                {
                    FilterTokenOperationType.Le => Expression.LessThanOrEqual(member, constant),
                    FilterTokenOperationType.Lt => Expression.LessThan(member, constant),
                    FilterTokenOperationType.Ge => Expression.GreaterThanOrEqual(member, constant),
                    FilterTokenOperationType.Gt => Expression.GreaterThan(member, constant),
                    FilterTokenOperationType.Eq => Expression.Equal(member, constant),
                    FilterTokenOperationType.Ne => Expression.NotEqual(member, constant),
                    FilterTokenOperationType.Co => Expression.Call(member, "Contains", null, constant),
                    FilterTokenOperationType.In => listType == typeof(int)
                        ? Expression.Call(constant, typeof(List<int>).GetMethod("Contains", new[] { typeof(int) })!,
                            member)
                        : Expression.Call(constant, typeof(List<Guid>).GetMethod("Contains", new[] { typeof(Guid) })!,
                            member),
                    _ => throw new NotSupportedException($"Operator {valueNode.Operator} is not supported.")
                };
        }

        if (node is BinaryExpressionNode binaryNode)
        {
            Expression left = BuildExpression(binaryNode.Left, parameter);
            Expression right = BuildExpression(binaryNode.Right, parameter);

            return binaryNode.Operator is not FilterTokenOperation operatorToken
                ? throw new Exception("Invalid operator")
                : (Expression)(operatorToken.CurrentFilterTokenOperationType switch
                {
                    FilterTokenOperationType.And => Expression.AndAlso(left, right),
                    FilterTokenOperationType.Or => Expression.OrElse(left, right),
                    _ => throw new NotSupportedException($"Operator {binaryNode.Operator} is not supported.")
                });
        }

        throw new NotSupportedException($"Node type {node.GetType().Name} is not supported.");
    }

    /// <summary>
    /// Lexer stores quoted literals as strings (<c>'1'</c> → string) and unquoted <c>true</c>/<c>false</c> as string too;
    /// only bare digits become <see cref="FilterTokenConstant{T}"/> of int. Bool fields must accept all of these.
    /// </summary>
    private static bool ParseBoolFilterConstant(FilterToken token)
    {
        switch (token)
        {
            case FilterTokenConstant<int> i:
                return i.ConstantValue != 0;
            case FilterTokenConstant<string> s:
            {
                string v = s.ConstantValue?.Trim() ?? string.Empty;
                if (bool.TryParse(v, out bool parsed))
                {
                    return parsed;
                }

                if (v.Equals("1", StringComparison.Ordinal) || v.Equals("yes", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                if (v.Equals("0", StringComparison.Ordinal) || v.Equals("no", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                throw new InvalidCastException(
                    $"Bool filter constant must be true/false, 1/0, yes/no, or an integer; got \"{v}\".");
            }
            default:
                throw new InvalidCastException("Bool filter constant must be a string or int literal.");
        }
    }
}