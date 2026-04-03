using FluentValidation;

namespace OneClickEcho.Application.User.Commands.DeleteUser;

public sealed class DeleteUserValidator : AbstractValidator<DeleteUserCommand>
{
    public DeleteUserValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty()
            .WithMessage("UserId must not be empty.");
    }
}

