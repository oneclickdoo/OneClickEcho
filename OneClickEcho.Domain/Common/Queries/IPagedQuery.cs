namespace OneClickEcho.Domain.Common.Queries;

public interface IPagedQuery
{
    int Page { get; set; }

    int PageSize { get; set; }

    string? OrderBy { get; set; }

    string? Filter { get; set; }
}