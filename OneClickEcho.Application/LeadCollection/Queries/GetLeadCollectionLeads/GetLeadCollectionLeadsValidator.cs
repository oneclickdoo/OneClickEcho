using FluentValidation;

namespace OneClickEcho.Application.LeadCollection.Queries.GetLeadCollectionLeads
{
    public class GetLeadCollectionLeadsValidator : AbstractValidator<GetLeadCollectionLeadsQuery>
    {
        public GetLeadCollectionLeadsValidator()
        {
            RuleFor(x => x.LeadCollectionId)
                .NotEmpty()
                .WithMessage("LeadCollectionId is required.");
        }
    }
}
