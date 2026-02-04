using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.Common.Identity;
using OneClickEcho.Domain.Common.Shared;

namespace OneClickEcho.Application.User.Commands.DeleteUser;

public class DeleteUserHandler(IUserManager userManager) : ICommandHandler<DeleteUserCommand, DeleteUserResponse>
{
    private readonly IUserManager _userManager = userManager;

    public async Task<Result<DeleteUserResponse>> Handle(DeleteUserCommand command, CancellationToken cancellationToken)
    {
        await _userManager.DeleteUser(command.Id, cancellationToken);

        return new DeleteUserResponse();
    }
}

