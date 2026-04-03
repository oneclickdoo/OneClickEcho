using FluentValidation;

namespace OneClickEcho.Application.Sender.Commands.DeleteSender
{
    public class DeleteSenderValidator : AbstractValidator<DeleteSenderCommand>
    {
        public DeleteSenderValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("SenderId must not be empty.");
        }
    }
}
