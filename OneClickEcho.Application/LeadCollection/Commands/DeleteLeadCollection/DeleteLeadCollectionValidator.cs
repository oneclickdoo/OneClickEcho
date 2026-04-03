using FluentValidation;

namespace OneClickEcho.Application.LeadCollection.Commands.DeleteLeadCollection;

public class DeleteLeadCollectionValidator : AbstractValidator<DeleteLeadCollectionCommand>
{
    public DeleteLeadCollectionValidator()
    {
        RuleFor(x => x.LeadCollectionId)
            .NotEmpty()
            .WithMessage("LeadCollectionId is required.");
    }
}