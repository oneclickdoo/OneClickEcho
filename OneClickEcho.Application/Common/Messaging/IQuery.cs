using MediatR;
using OneClickEcho.Domain.Common.Shared;

namespace OneClickEcho.Application.Common.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}