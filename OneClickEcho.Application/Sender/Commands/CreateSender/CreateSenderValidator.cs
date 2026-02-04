using FluentValidation;

namespace OneClickEcho.Application.Sender.Commands.CreateSender
{
    public class CreateSenderValidator : AbstractValidator<CreateSenderCommand>
    {
        public CreateSenderValidator()
        {
            RuleFor(x => x.CompanyId)
                .NotEmpty()
                .WithMessage("CompanyId is required.");

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Name is required.")
                .MinimumLength(3)
                .WithMessage("Name must be at least 3 characters.")
                .MaximumLength(30)
                .WithMessage("Name must be less than 30 characters.");

            RuleFor(x => x.Type)
                .NotNull()
                .WithMessage("Type is required.");
        }
    }
}
