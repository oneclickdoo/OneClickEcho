using FluentValidation;

namespace OneClickEcho.Application.Campaign.Commands.AddLeadsToCollectionFromStatus;

public class AddLeadsToCollectionFromStatusValidator : AbstractValidator<AddLeadsToCollectionFromStatusCommand>
{
    public AddLeadsToCollectionFromStatusValidator()
    {
        RuleFor(x => x.LeadCollectionId)
            .NotEmpty()
            .WithMessage("CampaignId is required.");
        
        RuleFor(x => x.CampaignId)
            .NotEmpty()
            .WithMessage("CampaignId is required.");
    }
}