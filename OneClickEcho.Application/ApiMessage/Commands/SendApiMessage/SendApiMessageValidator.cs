using FluentValidation;

namespace OneClickEcho.Application.ApiMessage.Commands.SendApiMessage;

public sealed class SendApiMessageValidator : AbstractValidator<SendApiMessageCommand>
{
    public SendApiMessageValidator()
    {
        RuleFor(x => x.CompanyId)
            .NotEmpty()
            .WithMessage("CompanyId is required.");
            
        RuleFor(x => x.ApiPassword)
            .NotEmpty()
            .WithMessage("ApiPassword is required.");
            
        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithMessage("PhoneNumber is required.")
            .Matches(@"^\+?[1-9]\d{1,14}$")
            .WithMessage("Invalid phone number format. Must follow E.164 format.");
            
        RuleFor(x => x.Message)
            .NotEmpty()
            .WithMessage("MessageContent is required.")
            .MaximumLength(1600)
            .WithMessage("MessageContent must not exceed 1600 characters.");
        
        RuleFor(x => x.Sender)
            .NotEmpty()
            .MaximumLength(30)
            .WithMessage("ViberSender must not exceed 30 characters");
        
        RuleFor(x => x.ApiMessageType)
            .NotNull()
            .WithMessage("MessageType is required.");
        
        RuleFor(x => x.SmsMessage)
            .MaximumLength(160)
            .WithMessage("SmsMessage must not exceed 160 characters");

        When(x => !string.IsNullOrWhiteSpace(x.ViberButtonUrl), () => {
            RuleFor(x => x.ViberButtonUrl)
                .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
                .WithMessage("ViberButtonUrl must be a valid URL");

            RuleFor(x => x.ViberButtonUrlTitle)
                .NotEmpty()
                .WithMessage("ViberButtonUrlTitle is required when ViberButtonUrl is provided")
                .MaximumLength(30)
                .WithMessage("ViberButtonUrlTitle must not exceed 30 characters");
        });
    }
}