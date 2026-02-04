using FluentValidation;

namespace OneClickEcho.Application.User.Commands.CreateUser;

public class CreateUserValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserValidator()
    {
        RuleFor(command => command.CompanyId)
            .NotEmpty()
            .WithMessage("CompanyId is required.");

        RuleFor(command => command.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Invalid email format.");

        RuleFor(user => user.Password)
            .Cascade(CascadeMode.Stop)
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
            .WithMessage("Password must contain at least one non-alphanumeric character.")
            .When(user => !string.IsNullOrEmpty(user.Password));

        RuleFor(command => command.Role)
            .NotEmpty()
            .WithMessage("Role is required.")
            .IsInEnum()
            .WithMessage("Invalid role.");
    }
}