using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.Common.Identity;

namespace OneClickEcho.Application.User.Commands.CreateUser;

public sealed record CreateUserCommand(
    Guid CompanyId,
    string Email,
    string? Password,
    UserRole Role
) : ICommand<CreateUserResponse>;