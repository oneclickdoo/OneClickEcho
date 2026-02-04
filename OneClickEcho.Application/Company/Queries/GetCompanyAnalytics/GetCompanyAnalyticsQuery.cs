using OneClickEcho.Application.Common.Messaging;

namespace OneClickEcho.Application.Company.Queries.GetCompanyAnalytics
{
    public record GetCompanyAnalyticsQuery(
        Guid CompanyId,
        string? StartDate,
        string? EndDate
        ) : IQuery<GetCompanyAnalyticsResponse>;
}
