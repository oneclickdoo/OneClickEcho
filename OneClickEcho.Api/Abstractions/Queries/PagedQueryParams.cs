using OneClickEcho.Application.Common.Abstractions;

namespace OneClickEcho.App.Abstractions.Queries;

public sealed class PagedQueryParams
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public string? OrderBy { get; set; }

    public string? Filter { get; set; }

    public TBasePagedQuery ConvertToBasePagedQuery<TBasePagedQuery>()
        where TBasePagedQuery : BasePagedQuery, new()
    {
        return new TBasePagedQuery
        {
            Page = Page,
            PageSize = PageSize,
            OrderBy = OrderBy,
            Filter = Filter
        };
    }
}