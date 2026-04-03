using FluentValidation;

namespace OneClickEcho.Application.Campaign.Commands.CloneCampaign
{
    public sealed class CloneCampaignValidator : AbstractValidator<CloneCampaignCommand>
    {
        public CloneCampaignValidator()
        {
            RuleFor(x => x.CampaignId)
                .NotEmpty()
                .WithMessage("CampaignId is required.");
        }
    }
}
