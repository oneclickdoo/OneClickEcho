using MediatR;
using OneClickEcho.Domain.Common.Shared;

namespace OneClickEcho.Application.Common.Messaging;

public interface IQueryHandler<TQuery, TResponse>
    : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}