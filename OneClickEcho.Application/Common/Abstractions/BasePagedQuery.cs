using OneClickEcho.Domain.Common.Queries;

namespace OneClickEcho.Application.Common.Abstractions;

public abstract class BasePagedQuery : IPagedQuery
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public string? OrderBy { get; set; }

    public string? Filter { get; set; }
}