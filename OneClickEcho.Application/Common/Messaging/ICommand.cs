using MediatR;
using OneClickEcho.Domain.Common.Shared;

namespace OneClickEcho.Application.Common.Messaging;

public interface ICommand : IRequest<Result>
{
}

public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}
