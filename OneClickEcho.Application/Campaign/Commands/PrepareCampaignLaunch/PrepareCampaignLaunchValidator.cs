using FluentValidation;

namespace OneClickEcho.Application.Campaign.Commands.PrepareCampaignLaunch;

public class PrepareCampaignLaunchValidator : AbstractValidator<PrepareCampaignLaunchCommand>
{
    public PrepareCampaignLaunchValidator()
    {
        RuleFor(x => x.CampaignId).NotEmpty().WithMessage("CampaignId is required.");
    }
}
