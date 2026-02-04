using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.Common.Identity;
using OneClickEcho.Domain.Common.Shared;

namespace OneClickEcho.Application.User.Commands.ForceResetPassword;

public class ForceResetPasswordHandler(IUserManager userManager)
    : ICommandHandler<ForceResetPasswordCommand, ForceResetPasswordResponse>
{
    private readonly IUserManager _userManager = userManager;

    public async Task<Result<ForceResetPasswordResponse>> Handle(ForceResetPasswordCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            await _userManager.ForceUpdatePassword(command.Id, command.NewPassword, cancellationToken);
        }
        catch
        {
            return Result.Failure<ForceResetPasswordResponse>(new Error(
                "Request.Failed",
                "Password failed to reset."
            ));
        }

        return new ForceResetPasswordResponse();
    }
}

