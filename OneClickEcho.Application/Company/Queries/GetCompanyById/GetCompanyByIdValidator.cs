using FluentValidation;

namespace OneClickEcho.Application.Company.Queries.GetCompanyById
{
    public class GetCompanyByIdValidator : AbstractValidator<GetCompanyByIdQuery>
    {
        public GetCompanyByIdValidator()
        {
            RuleFor(query => query.CompanyId)
                .NotEmpty()
                .WithMessage("CompanyId must not be empty.");
        }
    }
}
