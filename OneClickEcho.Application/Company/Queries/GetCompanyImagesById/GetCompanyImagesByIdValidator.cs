using FluentValidation;

namespace OneClickEcho.Application.Company.Queries.GetCompanyImagesById
{
    public class GetCompanyImagesByIdValidator : AbstractValidator<GetCompanyImagesByIdQuery>
    {
        public GetCompanyImagesByIdValidator()
        {
            RuleFor(query => query.CompanyId)
                .NotEmpty()
                .WithMessage("CompanyId must not be empty.");
        }
    }
}
