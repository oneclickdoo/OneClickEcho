using OneClickEcho.Application.Common.Messaging;

namespace OneClickEcho.Application.User.Commands.DeleteUser;

public record DeleteUserCommand(Guid Id) : ICommand<DeleteUserResponse>;

