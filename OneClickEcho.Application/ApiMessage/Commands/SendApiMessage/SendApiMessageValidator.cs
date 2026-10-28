using FluentValidation;
using OneClickEcho.Application.Common.Viber;

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
            
        When(x => x.ViberTemplateId is null, () =>
        {
            RuleFor(x => x.Message)
                .NotEmpty()
                .WithMessage("MessageContent is required.");
        });

        RuleFor(x => x.Message)
            .MaximumLength(1600)
            .WithMessage("MessageContent must not exceed 1600 characters.");

        RuleFor(x => x.ViberImageFormat)
            .Must(ViberImageFields.IsAllowedFormat)
            .WithMessage("ViberImageFormat must be gif or img.");

        RuleFor(x => x.ViberImageRatio)
            .Must(ViberImageFields.IsAllowedRatio)
            .WithMessage("ViberImageRatio must be square, portrait, or landscape.");

        RuleFor(x => x.ViberSurveyUiType)
            .Must(ui => ui is null or 1 or 2)
            .WithMessage("ViberSurveyUiType must be 1 or 2.");

        When(x => !string.IsNullOrWhiteSpace(x.ViberSurveyOptionsJson), () =>
        {
            RuleFor(x => x.ViberSurveyOptionsJson)
                .Must(json =>
                {
                    try
                    {
                        ViberSurveyOptionsHelper.ParseRequired(json);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                })
                .WithMessage("ViberSurveyOptionsJson must contain 2–10 unique options (max 50 characters).");
        });

        When(x => !string.IsNullOrWhiteSpace(x.ViberCarouselJson), () =>
        {
            RuleFor(x => x.ViberCarouselJson)
                .Must(json =>
                {
                    try
                    {
                        ViberCarouselHelper.ParseRequired(json);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                })
                .WithMessage("ViberCarouselJson must contain 2–5 valid cards.");
        });
        
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

        RuleFor(x => x.ViberVideoThumbnail)
            .MaximumLength(2048)
            .When(x => !string.IsNullOrWhiteSpace(x.ViberVideoThumbnail))
            .WithMessage("ViberVideoThumbnail must not exceed 2048 characters.");

        RuleFor(x => x.ViberFileSize)
            .GreaterThan(0)
            .When(x => x.ViberFileSize.HasValue)
            .WithMessage("ViberFileSize must be positive when set.");

        RuleFor(x => x.ViberVideoDuration)
            .InclusiveBetween(1, ViberVideoConstraints.MaxDurationSeconds)
            .When(x => x.ViberVideoDuration.HasValue)
            .WithMessage(
                $"ViberVideoDuration must be between 1 and {ViberVideoConstraints.MaxDurationSeconds} seconds when set (Viber video limit).");
    }
}