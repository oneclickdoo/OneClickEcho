using FluentValidation;

namespace OneClickEcho.Application.Scheduling.Commands.EnqueueCampaign;

public class EnqueueCampaignValidator : AbstractValidator<EnqueueCampaign.EnqueueCampaignCommand>
{
    public EnqueueCampaignValidator()
    {
        RuleFor(x => x.CampaignId)
            .NotEmpty()
            .WithMessage("CampaignId is required.");
    }
}