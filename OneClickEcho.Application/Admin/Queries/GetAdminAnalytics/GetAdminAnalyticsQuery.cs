using OneClickEcho.Application.Common.Messaging;

namespace OneClickEcho.Application.Admin.Queries.GetAdminAnalytics
{
    public record GetAdminAnalyticsQuery(string? StartDate, string? EndDate) : IQuery<GetAdminAnalyticsResponse>;
}
