using FluentValidation;

namespace OneClickEcho.Application.Campaign.Commands.PauseCampaign
{
    public class PauseCampaignValidator : AbstractValidator<PauseCampaignCommand>
    {
        public PauseCampaignValidator()
        {
            RuleFor(x => x.CampaignId)
                .NotEmpty()
                .WithMessage("CampaignId is required.");
        }
    }
}
