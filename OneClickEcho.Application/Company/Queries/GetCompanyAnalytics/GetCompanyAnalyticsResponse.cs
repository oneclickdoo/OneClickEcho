using OneClickEcho.Domain.CompanyAggregate.Repositories;

namespace OneClickEcho.Application.Company.Queries.GetCompanyAnalytics
{
    public class GetCompanyAnalyticsResponse
    {
        public decimal ViberPrice { get; set; }
        
        public decimal SmsPrice { get; set; }
        
        public AnalyticsResults? AnalyticsResults { get; set; }
    }
}
