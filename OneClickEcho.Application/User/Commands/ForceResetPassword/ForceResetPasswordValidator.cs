using FluentValidation;

namespace OneClickEcho.Application.User.Commands.ForceResetPassword;

public sealed class ForceResetPasswordValidator : AbstractValidator<ForceResetPasswordCommand>
{
    public ForceResetPasswordValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("User ID is required.")
            .Must(userId => userId != Guid.Empty)
            .WithMessage("User ID must be a valid GUID.");

        RuleFor(user => user.NewPassword)
            .NotEmpty()
            .WithMessage("Password is required.")
            .MinimumLength(6)
            .WithMessage("Password must be at least 6 characters long.")
            .Matches("[A-Z]")
            .WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]")
            .WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]")
            .WithMessage("Password must contain at least one digit.")
            .Matches("[^a-zA-Z0-9]")
            .WithMessage("Password must contain at least one non-alphanumeric character.");
    }
}

