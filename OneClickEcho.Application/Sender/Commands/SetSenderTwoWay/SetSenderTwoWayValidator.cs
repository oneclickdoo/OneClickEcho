using FluentValidation;

namespace OneClickEcho.Application.Sender.Commands.SetSenderTwoWay
{
    public class SetSenderTwoWayValidator : AbstractValidator<SetSenderTwoWayCommand>
    {
        public SetSenderTwoWayValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("SenderId must not be empty.");
        }
    }
}
