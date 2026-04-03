using FluentValidation;

namespace OneClickEcho.Application.User.Commands.AssignUser
{
    public class AssignUserValidator : AbstractValidator<AssignUserCommand>
    {
        public AssignUserValidator()
        {
            RuleFor(command => command.UserId)
                .NotEmpty()
                .WithMessage("UserId is required.");

            RuleFor(command => command.CompanyId)
                .NotEmpty()
                .WithMessage("CompanyId is required.");
        }
    }
}
