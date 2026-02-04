using FluentValidation;
using OneClickEcho.Domain.CampaignAggregate.Enums;

namespace OneClickEcho.Application.Campaign.Commands.CreateCampaign;

public sealed class CreateCampaignValidator : AbstractValidator<CreateCampaignCommand>
{
    public CreateCampaignValidator()
    {
        RuleFor(x => x.CampaignName)
            .NotEmpty()
            .WithMessage("CampaignName is required.")
            .MinimumLength(3)
            .WithMessage("CampaignName must be at least 3 characters.")
            .MaximumLength(30)
            .WithMessage("CampaignName must be less than 30 characters.");

        RuleFor(x => x.CompanyId)
            .NotEmpty()
            .WithMessage("CompanyId is required.");

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

        RuleFor(x => x.IsSms)
            .NotNull()
            .WithMessage("IsSms cannot be null.");

        RuleFor(x => x.SmsSender)
            .MaximumLength(255)
            .WithMessage("SmsSender must be less than 255 characters.")
            .When(x => x.IsSms);

        RuleFor(x => x.SmsMessage)
            .MaximumLength(160)
            .WithMessage("SmsMessage must be less than 160 characters.")
            .When(x => x.IsSms && !string.IsNullOrEmpty(x.SmsMessage));

        RuleFor(x => x.SendingType)
            .IsInEnum()
            .WithMessage("SendingType is invalid.");

        RuleFor(x => x.SendingDatetime)
            .NotEmpty()
            .WithMessage("SendingDatetime is required.")
            .When(x => x.SendingType == CampaignSendingType.ScheduledDateTime);
    }
}