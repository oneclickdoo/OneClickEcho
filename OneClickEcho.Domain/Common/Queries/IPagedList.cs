namespace OneClickEcho.Domain.Common.Queries;

public interface IPagedList<T>
{
    List<T> Items { get; }
    int PageNumber { get; }
    int PageSize { get; }
    int TotalCount { get; }
    int TotalPages { get; }

    bool HasNextPage => PageNumber < TotalPages;
    bool HasPreviousPage => PageNumber > 1;
}