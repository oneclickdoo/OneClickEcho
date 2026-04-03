using OneClickEcho.Application.Common.Messaging;

namespace OneClickEcho.Application.User.Commands.ResetPassword;

public record ResetPasswordCommand(string Password, string NewPassword) : ICommand<ResetPasswordResponse>;

