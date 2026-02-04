using FluentValidation;

namespace OneClickEcho.Application.User.Queries.GetCurrentUser;

public sealed class GetCurrentUserValidator : AbstractValidator<GetCurrentUserQuery>
{
    public GetCurrentUserValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Invalid email format.");

        RuleForEach(x => x.CompanyIds)
            .NotEmpty()
            .WithMessage("CompanyIds is required.")
            .NotEqual(Guid.Empty)
            .WithMessage("CompanyId must be a valid GUID.");
    }
}