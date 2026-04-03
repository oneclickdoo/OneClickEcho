using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.Common.Identity;
using OneClickEcho.Domain.Common.Shared;

namespace OneClickEcho.Application.User.Commands.ResetPassword;

public class ResetPasswordHandler(IUserManager userManager) : ICommandHandler<ResetPasswordCommand, ResetPasswordResponse>
{
    private readonly IUserManager _userManager = userManager;

    public async Task<Result<ResetPasswordResponse>> Handle(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        try
        {
            await _userManager.UpdatePassword(command.Password, command.NewPassword, cancellationToken);
        }
        catch
        {
            return Result.Failure<ResetPasswordResponse>(new Error(
                "Request.Failed",
                "Password failed to reset."
            ));
        }
        return new ResetPasswordResponse();
    }
}

