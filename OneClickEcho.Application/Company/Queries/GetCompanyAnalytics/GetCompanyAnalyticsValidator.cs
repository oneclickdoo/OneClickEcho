using FluentValidation;

namespace OneClickEcho.Application.Company.Queries.GetCompanyAnalytics
{
    public class GetCompanyAnalyticsValidator : AbstractValidator<GetCompanyAnalyticsQuery>
    {
        public GetCompanyAnalyticsValidator()
        {
            RuleFor(query => query.CompanyId)
                .NotEmpty()
                .WithMessage("CompanyId must not be empty.");
        }
    }
}
