using FluentValidation;

namespace OneClickEcho.Application.Campaign.Commands.LaunchCampaign
{
    public class LaunchCampaignValidator : AbstractValidator<LaunchCampaignCommand>
    {
        public LaunchCampaignValidator()
        {
            RuleFor(x => x.CampaignId)
                .NotEmpty()
                .WithMessage("CampaignId is required.");
        }
    }
}
