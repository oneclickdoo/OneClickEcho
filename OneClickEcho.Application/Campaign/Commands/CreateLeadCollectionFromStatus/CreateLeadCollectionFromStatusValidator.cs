using FluentValidation;

namespace OneClickEcho.Application.Campaign.Commands.CreateLeadCollectionFromStatus;

public class CreateLeadCollectionFromStatusValidator : AbstractValidator<CreateLeadCollectionFromStatusCommand>
{
    public CreateLeadCollectionFromStatusValidator()
    {
        RuleFor(x => x.CompanyId)
            .NotEmpty()
            .WithMessage("CampaignId is required.");
        
        RuleFor(x => x.CampaignId)
            .NotEmpty()
            .WithMessage("CampaignId is required.");
        
        RuleFor(x => x.CollectionName)
            .NotEmpty()
            .WithMessage("Collection name is required.")
            .MaximumLength(100)
            .WithMessage("Collection name cannot exceed 100 characters.");
    }
}