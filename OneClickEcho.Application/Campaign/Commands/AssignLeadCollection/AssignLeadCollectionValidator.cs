using FluentValidation;

namespace OneClickEcho.Application.Campaign.Commands.AssignLeadCollection;

public class AssignLeadCollectionValidator : AbstractValidator<AssignLeadCollectionCommand>
{
    public AssignLeadCollectionValidator()
    {
        RuleFor(x => x.CampaignId)
            .NotEmpty()
            .WithMessage("CampaignId is required.");

        RuleFor(x => x.LeadCollectionId)
            .NotEmpty()
            .WithMessage("LeadCollectionId is required.");
    }
}