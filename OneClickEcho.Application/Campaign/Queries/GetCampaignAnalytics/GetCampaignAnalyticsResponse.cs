namespace OneClickEcho.Application.Campaign.Queries.GetCampaignAnalytics
{
    public class GetCampaignAnalyticsResponse
    {
        public CampaignViberAnalytics? Viber { get; set; }

        public CampaignSmsAnalytics? Sms { get; set; }
    }

    public class CampaignViberAnalytics
    {
        /// <summary>All campaign leads (recipients).</summary>
        public int Total { get; set; }

        /// <summary>Viber status None (0) only — not submitted to Comtrade / gateway.</summary>
        public int NotSent { get; set; }

        /// <summary>Handed to gateway: any viber_status other than 0 (includes Received, Pending, …).</summary>
        public int Sent { get; set; }

        public int Pending { get; set; }

        public int Delivered { get; set; }

        public int Undelivered { get; set; }

        public int Seen { get; set; }

        public int Received { get; set; }

        public int Clicked { get; set; }

        public int Expired { get; set; }

        public int Unsubscribed { get; set; }

        /// <summary>For funnel: Delivered + Seen + Clicked.</summary>
        public int FunnelDelivered { get; set; }

        /// <summary>For funnel: Seen + Clicked.</summary>
        public int FunnelSeen { get; set; }
    }

    public class CampaignSmsAnalytics
    {
        /// <summary>All campaign leads when SMS channel is used.</summary>
        public int Total { get; set; }

        /// <summary>SMS status None.</summary>
        public int NotSent { get; set; }

        /// <summary>At least one SMS attempt (not None).</summary>
        public int Sent { get; set; }

        public int Pending { get; set; }

        public int Delivered { get; set; }

        public int Undelivered { get; set; }

        public int Blacklisted { get; set; }

        public int Error { get; set; }

        public int FunnelDelivered { get; set; }

        public int FunnelSeen { get; set; }
    }
}
