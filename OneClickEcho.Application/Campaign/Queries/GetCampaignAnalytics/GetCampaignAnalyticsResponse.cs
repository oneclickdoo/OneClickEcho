namespace OneClickEcho.Application.Campaign.Queries.GetCampaignAnalytics
{
    public class GetCampaignAnalyticsResponse
    {
        public CampaignViberAnalytics? Viber { get; set; }

        public CampaignSmsAnalytics? Sms { get; set; }
    }

    public class CampaignViberAnalytics
    {
        public int Pending { get; set; }

        public int Delivered { get; set; }

        public int Undelivered { get; set; }

        public int Seen { get; set; }

        public int Received { get; set; }

        public int Clicked { get; set; }

        public int Expired { get; set; }
        
        public int Unsubscribed { get; set; }

        public int Total { get; set; }
    }

    public class CampaignSmsAnalytics
    {
        public int Pending { get; set; }

        public int Delivered { get; set; }

        public int Undelivered { get; set; }

        public int Blacklisted { get; set; }

        public int Error { get; set; }

        public int Total { get; set; }
    }
}
