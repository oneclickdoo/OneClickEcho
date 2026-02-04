using OneClickEcho.Application.Common.Messaging;

namespace OneClickEcho.Application.User.Commands.ForceResetPassword;

public record ForceResetPasswordCommand(Guid Id, string NewPassword) : ICommand<ForceResetPasswordResponse>;

