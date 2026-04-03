using FluentValidation;

namespace OneClickEcho.Application.Company.Queries.ExportCompanyLeads
{
    public class ExportCompanyLeadsValidator : AbstractValidator<ExportCompanyLeadsQuery>
    {
        public ExportCompanyLeadsValidator()
        {
            RuleFor(query => query.CompanyId)
                .NotEmpty()
                .WithMessage("CompanyId must not be empty.");
        }
    }
}
