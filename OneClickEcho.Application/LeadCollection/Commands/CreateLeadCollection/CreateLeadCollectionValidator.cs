using FluentValidation;

namespace OneClickEcho.Application.LeadCollection.Commands.CreateLeadCollection;

public class CreateLeadCollectionValidator : AbstractValidator<CreateLeadCollectionCommand>
{
    public CreateLeadCollectionValidator()
    {
        RuleFor(x => x.CollectionName)
            .NotEmpty()
            .WithMessage("Collection name is required.")
            .MaximumLength(100)
            .WithMessage("Collection name cannot exceed 100 characters.");

        RuleFor(x => x.CompanyId)
            .NotEmpty()
            .WithMessage("CompanyId is required.")
            .Must(id => id != Guid.Empty)
            .WithMessage("CompanyId must be a valid GUID.");
    }
}