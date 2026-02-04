using FluentValidation;

namespace OneClickEcho.Application.Scheduling.Commands.CompleteCampaign;

public class CompleteCampaignValidator : AbstractValidator<CompleteCampaignCommand>
{
    public CompleteCampaignValidator()
    {
        RuleFor(x => x.CampaignId)
            .NotEmpty()
            .WithMessage("CampaignId is required.");
    }
}