using System.Linq.Expressions;
using System.Reflection;

namespace OneClickEcho.Persistence.Common;

public static class OrderBy
{
    public static IQueryable<T> ApplyOrderBy<T>(this IQueryable<T> source, string orderBys)
    {
        if (string.IsNullOrWhiteSpace(orderBys))
        {
            return source;
        }

        string[] orderByClauses = orderBys.Split(',', StringSplitOptions.RemoveEmptyEntries);
        bool firstOrderBy = true;

        foreach (string orderByClause in orderByClauses)
        {
            if (string.IsNullOrWhiteSpace(orderByClause))
            {
                continue;
            }

            string[] orderByParts = orderByClause.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string propertyName = orderByParts[0];
            string direction = orderByParts.Length > 1 && orderByParts[1].Equals("desc", StringComparison.OrdinalIgnoreCase) ? "OrderByDescending" : "OrderBy";

            if (!firstOrderBy)
            {
                // Change method to ThenBy or ThenByDescending for subsequent ordering
                direction = direction.Replace("OrderBy", "ThenBy");
            }
            ParameterExpression parameter = Expression.Parameter(typeof(T), "x");

            // Dashboard sends camelCase (e.g. createdAt); CLR properties are PascalCase (CreatedAt).
            PropertyInfo? propInfo = typeof(T).GetProperty(
                propertyName,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            if (propInfo is null && propertyName.Equals("Id", StringComparison.OrdinalIgnoreCase))
            {
                // AggregateRootId shadowing: pick the Id whose type matches the aggregate's generic id argument.
                propInfo = parameter.Type
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .FirstOrDefault(p =>
                        p.Name == "Id" &&
                        p.PropertyType == parameter.Type.BaseType?.GetGenericArguments().FirstOrDefault());
            }

            if (propInfo is null)
            {
                throw new ArgumentException($"OrderBy property '{propertyName}' was not found on type {typeof(T).Name}.");
            }

            MemberExpression property = Expression.Property(parameter, propInfo);

            LambdaExpression selector = Expression.Lambda(property, parameter);

            MethodCallExpression resultExpression = Expression.Call(
                typeof(Queryable),
                direction,
                new Type[] { typeof(T), property.Type },
                source.Expression,
                Expression.Quote(selector));

            source = source.Provider.CreateQuery<T>(resultExpression);
            firstOrderBy = false;
        }

        return source;
    }
}