using FluentValidation;

namespace OneClickEcho.Application.NounCase.Queries.GetNounCaseByNominative;

public sealed class GetNounCaseByNominativeValidator : AbstractValidator<GetNounCaseByNominativeQuery>
{
    public GetNounCaseByNominativeValidator()
    {
        RuleFor(p => p.Nominative).NotEmpty().MaximumLength(100);
    }
}