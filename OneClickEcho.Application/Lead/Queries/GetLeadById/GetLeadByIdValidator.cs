using FluentValidation;

namespace OneClickEcho.Application.Lead.Queries.GetLeadById;

public class GetLeadByIdValidator : AbstractValidator<GetLeadByIdQuery>
{
    public GetLeadByIdValidator()
    {
        RuleFor(x => x.LeadId)
            .NotEmpty().WithMessage("Id is required.")
            .Must(id => id != Guid.Empty).WithMessage("Id must be a valid GUID.");
    }
}