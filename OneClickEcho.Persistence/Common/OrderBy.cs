using System.Linq.Expressions;

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

            MemberExpression property;
            try
            {
                property = Expression.Property(parameter, propertyName);
            }
            catch
            {
                // AggregateRootId doesn't handle shadowed Id well, so I have to manually tell him which one to take
                // Get the first one with the name Id and of the correct type
                property = Expression.Property(parameter,
                    parameter.Type
                        .GetProperties()
                        .First(p =>
                            p.Name == propertyName &&
                            p.PropertyType == parameter.Type.BaseType?.GetGenericArguments().FirstOrDefault()));
            }

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