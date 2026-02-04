using FluentValidation;

namespace OneClickEcho.Application.LeadCollection.Queries.GetLeadCollectionById;

public class GetLeadCollectionByIdValidator : AbstractValidator<GetLeadCollectionByIdQuery>
{
    public GetLeadCollectionByIdValidator()
    {
        RuleFor(x => x.LeadCollectionId)
            .NotEmpty()
            .WithMessage("LeadCollectionId is required.");
    }
}