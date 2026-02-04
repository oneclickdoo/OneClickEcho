using OneClickEcho.Application.Common.Messaging;

namespace OneClickEcho.Application.User.Commands.AssignUser
{
    public sealed record AssignUserCommand(
        Guid UserId,
        Guid CompanyId
        ) : ICommand<AssignUserResponse>;
}
