using FluentValidation;

namespace OneClickEcho.Application.LeadCollection.Commands.EditLeadCollection;

public class EditLeadCollectionValidator : AbstractValidator<EditLeadCollectionCommand>
{
    public EditLeadCollectionValidator()
    {
        RuleFor(x => x.LeadCollectionId)
            .NotEmpty()
            .WithMessage("LeadCollectionId is required.");

        RuleFor(x => x.CollectionName)
            .NotEmpty()
            .WithMessage("Collection name is required.")
            .MaximumLength(100)
            .WithMessage("Collection name cannot exceed 100 characters.");
    }
}