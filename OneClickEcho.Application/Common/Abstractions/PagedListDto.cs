using OneClickEcho.Domain.Common.Queries;

namespace OneClickEcho.Application.Common.Abstractions;

public abstract class PagedListDto<T, TDto>
{
    protected PagedListDto(IPagedList<T> items)
    {
        Items = ConvertTToTDto(items.Items);
        PageNumber = items.PageNumber;
        PageSize = items.PageSize;
        TotalCount = items.TotalCount;
        TotalPages = items.TotalPages;
    }

    public List<TDto> Items { get; }

    public int PageNumber { get; }

    public int PageSize { get; }

    public int TotalCount { get; }

    public int TotalPages { get; }

    public bool HasNextPage => PageNumber < TotalPages;

    public bool HasPreviousPage => PageNumber > 1;

    public abstract List<TDto> ConvertTToTDto(List<T> items);
}