using FluentValidation;
using OneClickEcho.Application.Common.Viber;
using OneClickEcho.Domain.CampaignAggregate.Enums;

namespace OneClickEcho.Application.Campaign.Commands.EditCampaign;

public sealed class EditCampaignValidator : AbstractValidator<EditCampaignCommand>
{
    public EditCampaignValidator()
    {
        RuleFor(x => x.CampaignId)
            .NotEmpty()
            .WithMessage("CampaignId is required.");

        RuleFor(x => x.IsViber)
            .NotNull()
            .WithMessage("IsViber cannot be null.");

        RuleFor(x => x.FallbackToSMS)
            .NotNull()
            .WithMessage("FallbackToSMS cannot be null.");

        RuleFor(x => x.IsViberReceivable)
            .NotNull()
            .WithMessage("IsViberReceivable cannot be null.");

        RuleFor(x => x.ViberSender)
            .MaximumLength(255)
            .WithMessage("ViberSender must be less than 255 characters.")
            .When(x => x.IsViber);

        RuleFor(x => x.ViberMessage)
            .MaximumLength(1000)
            .WithMessage("ViberMessage must be less than 1000 characters.")
            .When(x => x.IsViber);

        RuleFor(x => x.ViberButtonUrl)
            .MaximumLength(2048)
            .WithMessage("ViberUrl must be less than 2048 characters.")
            .When(x => x.IsViber && !string.IsNullOrEmpty(x.ViberButtonUrl));

        RuleFor(x => x.ViberButtonUrlTitle)
            .MaximumLength(255)
            .WithMessage("ViberUrlTitle must be less than 255 characters.")
            .When(x => x.IsViber && !string.IsNullOrEmpty(x.ViberButtonUrlTitle));

        RuleFor(x => x.ViberFileSize)
            .GreaterThan(0)
            .When(x => x.IsViber && x.ViberFileSize.HasValue)
            .WithMessage("ViberFileSize must be positive when set.");

        RuleFor(x => x.ViberVideoDuration)
            .InclusiveBetween(1, ViberVideoConstraints.MaxDurationSeconds)
            .When(x => x.IsViber && x.ViberVideoDuration.HasValue)
            .WithMessage(
                $"ViberVideoDuration must be between 1 and {ViberVideoConstraints.MaxDurationSeconds} seconds when set.");

        RuleFor(x => x.IsSms)
            .NotNull()
            .WithMessage("IsSms cannot be null.");

        RuleFor(x => x.SmsSender)
            .MaximumLength(255)
            .WithMessage("SmsSender must be less than 255 characters.")
            .When(x => x.IsViber);

        RuleFor(x => x.SmsMessage)
            .MaximumLength(160)
            .WithMessage("SmsMessage must be less than 160 characters.")
            .When(x => x.IsSms && !string.IsNullOrEmpty(x.SmsMessage));

        RuleFor(x => x.SendingType)
            .IsInEnum()
            .WithMessage("SendingType is invalid.");

        RuleFor(x => x.ViberContentKind)
            .IsInEnum()
            .When(x => x.IsViber)
            .WithMessage("ViberContentKind is invalid.");

        When(x => x.IsViber && x.ViberContentKind == CampaignViberContentKind.Survey, () =>
        {
            RuleFor(x => x.ViberSurveyOptionsJson)
                .NotEmpty()
                .WithMessage("Survey options are required for survey campaigns.")
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
                .WithMessage("Survey options must be valid JSON with 2–5 non-empty options (max 50 characters each).");
        });

        RuleFor(x => x.SendingDatetime)
            .NotEmpty()
            .WithMessage("SendingDatetime is required.")
            .When(x => x.SendingType == CampaignSendingType.ScheduledDateTime);
    }
}