using FluentValidation;

namespace OneClickEcho.Application.LeadCollection.Commands.AssignLeadsToCollection;

public class AssignLeadsToCollectionValidator : AbstractValidator<AssignLeadsToCollectionCommand>
{
    public AssignLeadsToCollectionValidator()
    {
        RuleFor(x => x.LeadCollectionId)
            .NotEmpty()
            .WithMessage("LeadCollectionId is required.");

        RuleFor(x => x.Leads)
            .NotEmpty()
            .WithMessage("Leads list cannot be empty.")
            .Must(leads => leads != null && leads.Count > 0)
            .WithMessage("At least one lead must be provided.")
            .ForEach(lead => lead.SetValidator(new SingleLeadDtoValidator()));
    }
}

public class SingleLeadDtoValidator : AbstractValidator<SingleLeadDto>
{
    public SingleLeadDtoValidator()
    {
        RuleFor(x => x.LeadId)
            .NotEmpty()
            .WithMessage("LeadId is required.");
    }
}