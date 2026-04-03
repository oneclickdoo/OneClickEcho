namespace OneClickEcho.Application.Admin.Queries.GetAdminAnalytics
{
    public class GetAdminAnalyticsResponse
    {
        public int TotalViberMessagesSent { get; set; }

        public int TotalViberMessagesDelivered { get; set; }

        public int TotalSmsMessagesSent { get; set; }

        public int TotalSmsMessagesDelivered { get; set; }

        public decimal TotalRevenue { get; set; }

        public decimal TotalProfit { get; set; }

        public List<AdminCompanyAnalyticsDto> Companies { get; set; } = default!;
    }

    public class AdminCompanyAnalyticsDto
    {
        public Guid CompanyId { get; set; }

        public string Name { get; set; } = string.Empty;

        public int ViberMessagesSent { get; set; }

        public int ViberMessagesDelivered { get; set; }

        public int SmsMessagesSent { get; set; }

        public int SmsMessagesDelivered { get; set; }

        public decimal Revenue { get; set; }

        public decimal Profit { get; set; }
    }
}
