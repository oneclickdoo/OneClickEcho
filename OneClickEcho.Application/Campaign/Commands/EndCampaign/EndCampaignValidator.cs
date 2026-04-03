using FluentValidation;

namespace OneClickEcho.Application.Campaign.Commands.EndCampaign
{
    public class EndCampaignValidator : AbstractValidator<EndCampaignCommand>
    {
        public EndCampaignValidator()
        {
            RuleFor(x => x.CampaignId)
                .NotEmpty()
                .WithMessage("CampaignId is required.");
        }
    }
}
