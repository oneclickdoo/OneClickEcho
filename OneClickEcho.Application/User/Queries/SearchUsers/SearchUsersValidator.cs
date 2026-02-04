using FluentValidation;

namespace OneClickEcho.Application.User.Queries.SearchUsers
{
    public class SearchUsersValidator : AbstractValidator<SearchUsersQuery>
    {
        public SearchUsersValidator()
        {
            RuleFor(x => x.CompanyId)
                .NotEmpty()
                .WithMessage("CompanyId is required.")
                .NotEqual(Guid.Empty)
                .WithMessage("CompanyId must be a valid GUID.");

            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email is required.");
        }
    }
}
