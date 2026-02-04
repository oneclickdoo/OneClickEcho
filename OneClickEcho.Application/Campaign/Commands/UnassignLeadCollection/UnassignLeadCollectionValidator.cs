using FluentValidation;

namespace OneClickEcho.Application.Campaign.Commands.UnassignLeadCollection;

public class UnassignLeadCollectionValidator : AbstractValidator<UnassignLeadCollectionCommand>
{
    public UnassignLeadCollectionValidator()
    {
        RuleFor(x => x.CampaignId)
            .NotEmpty()
            .WithMessage("CampaignId is required.");

        RuleFor(x => x.LeadCollectionId)
            .NotEmpty()
            .WithMessage("LeadCollectionId is required.");
    }
}