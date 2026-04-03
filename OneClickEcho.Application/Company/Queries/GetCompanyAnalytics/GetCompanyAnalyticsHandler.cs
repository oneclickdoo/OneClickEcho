using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.CampaignAggregate.Repositories;
using OneClickEcho.Domain.CampaignLeadAggregate.Repositories;
using OneClickEcho.Domain.Common.Shared;
using OneClickEcho.Domain.CompanyAggregate.Repositories;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;

namespace OneClickEcho.Application.Company.Queries.GetCompanyAnalytics
{
    public class GetCompanyAnalyticsHandler(ICompanyRepository companyRepository, ICampaignRepository campaignRepository,
        ICampaignLeadRepository campaignLeadRepository) : IQueryHandler<GetCompanyAnalyticsQuery, GetCompanyAnalyticsResponse>
    {
        private readonly ICompanyRepository _companyRepository = companyRepository;
        private readonly ICampaignRepository _campaignRepository = campaignRepository;
        private readonly ICampaignLeadRepository _campaignLeadRepository = campaignLeadRepository;

        public async Task<Result<GetCompanyAnalyticsResponse>> Handle(GetCompanyAnalyticsQuery query, CancellationToken cancellationToken)
        {
            // get company
            CompanyId companyId = CompanyId.Create(query.CompanyId);

            Domain.CompanyAggregate.Company? company = await _companyRepository
                .GetByIdAsync(companyId, cancellationToken);

            if (company is null)
            {
                Result.Failure<GetCompanyAnalyticsResponse>
                    (new Error("Company.NotFound", $"The Company with Id:\"{query.CompanyId}\" does not exist."));
            }

            // parse date
            DateTime? startDate = null;
            DateTime? endDate = null;

            if (query.StartDate != null)
            {
                startDate = DateTime.Parse(query.StartDate);
            }
            if (query.EndDate != null)
            {
                endDate = DateTime.Parse(query.EndDate);
            }
            
            var analyticResults = await _companyRepository.GetAnalyticsResultsAsync(companyId, startDate, endDate, cancellationToken);
            
            // Test
            analyticResults.ViberDelivered += analyticResults.NumberOfTestsViber;
            analyticResults.ViberTotalSent += analyticResults.NumberOfTestsViber;
            analyticResults.ViberTotalLeads += analyticResults.NumberOfTestsViber;
            analyticResults.ViberClicked += analyticResults.NumberOfTestsViberClicked;
            analyticResults.SmsTotalSent += analyticResults.NumberOfTestsSms;
            analyticResults.SmsDelivered += analyticResults.NumberOfTestsSms;
            
            // API
            analyticResults.ViberDelivered += analyticResults.NumberOfApiViber;
            analyticResults.ViberTotalSent += analyticResults.NumberOfApiViber;
            analyticResults.ViberTotalLeads += analyticResults.NumberOfApiViber;
            analyticResults.ViberClicked += analyticResults.NumberOfApiViberClicked;
            analyticResults.SmsTotalSent += analyticResults.NumberOfApiSms;
            analyticResults.SmsDelivered += analyticResults.NumberOfApiSms;

            return new GetCompanyAnalyticsResponse()
            {
                ViberPrice = company!.ViberPricePerMesssage,
                SmsPrice = company!.SmsPricePerMesssage,
                AnalyticsResults = analyticResults
            };
        }
    }
}
