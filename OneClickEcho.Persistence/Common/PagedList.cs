using Microsoft.EntityFrameworkCore;
using OneClickEcho.Domain.ApplicationUserAggregate;
using OneClickEcho.Domain.Common.Queries;
using OneClickEcho.Persistence.Common.Filtering;

namespace OneClickEcho.Persistence.Common;

public class PagedList<T> : IPagedList<T>
{
    private PagedList(List<T> items, int pageNumber, int pageSize, int totalCount)
    {
        Items = items;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalCount = totalCount;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
    }

    public List<T> Items { get; }

    public int PageNumber { get; }

    public int PageSize { get; }

    public int TotalCount { get; }

    public int TotalPages { get; }

    public bool HasNextPage => PageNumber < TotalPages;

    public bool HasPreviousPage => PageNumber > 1;

    public static async Task<PagedList<T>> CreateAsync(IQueryable<T> source, IPagedQuery query, CancellationToken cancellationToken = default)
    {
        IQueryable<T> itemsQuery = source;

        if (typeof(T) == typeof(ApplicationUser))
        {
            itemsQuery = itemsQuery.Cast<ApplicationUser>().Include("CompanyIds").Cast<T>();
        }

        if (!string.IsNullOrEmpty(query.Filter))
        {
            itemsQuery = itemsQuery.Where(Filtering<T>.ApplyFilter(query.Filter));
        }

        if (!string.IsNullOrEmpty(query.OrderBy))
        {
            itemsQuery = itemsQuery.ApplyOrderBy(query.OrderBy);
        }

        int totalCount = await itemsQuery.CountAsync(cancellationToken);

        itemsQuery = itemsQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize);

        List<T> items = await itemsQuery.ToListAsync(cancellationToken);

        return new PagedList<T>(items, query.Page, query.PageSize, totalCount);
    }

    /// <summary>Builds a page when the query was executed manually (e.g. join + filters not expressible as <see cref="Filtering{TEntity}"/>).</summary>
    public static PagedList<T> CreateFromParts(List<T> items, int pageNumber, int pageSize, int totalCount) =>
        new(items, pageNumber, pageSize, totalCount);
}