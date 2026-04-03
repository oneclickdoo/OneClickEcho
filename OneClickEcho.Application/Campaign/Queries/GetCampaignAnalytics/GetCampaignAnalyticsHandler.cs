using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.CampaignAggregate.Repositories;
using OneClickEcho.Domain.CampaignAggregate.ValueObjects;
using OneClickEcho.Domain.CampaignLeadAggregate;
using OneClickEcho.Domain.CampaignLeadAggregate.Enums;
using OneClickEcho.Domain.CampaignLeadAggregate.Repositories;
using OneClickEcho.Domain.Common.Shared;

namespace OneClickEcho.Application.Campaign.Queries.GetCampaignAnalytics
{
    public class GetCampaignAnalyticsHandler(ICampaignRepository campaignRepository,
        ICampaignLeadRepository campaignLeadRepository/*, IRedisCacheService cacheService*/)
        : IQueryHandler<GetCampaignAnalyticsQuery, GetCampaignAnalyticsResponse>
    {
        private readonly ICampaignRepository _campaignRepository = campaignRepository;
        private readonly ICampaignLeadRepository _campaignLeadRepository = campaignLeadRepository;
        //private readonly IRedisCacheService _cacheService = cacheService;

        public async Task<Result<GetCampaignAnalyticsResponse>> Handle(GetCampaignAnalyticsQuery query, CancellationToken cancellationToken)
        {
            // get campaign
            Domain.CampaignAggregate.Campaign? campaign = await _campaignRepository
                .GetByIdAsync(CampaignId.Create(query.CampaignId), cancellationToken);

            if (campaign is null)
            {
                Result.Failure<GetCampaignAnalyticsResponse>
                    (new Error("Campaign.NotFound", $"The Campaign with Id:\"{query.CampaignId}\" does not exist."));
            }

            // check cache
            //string cacheKey = $"Campaign:{query.CampaignId}";

            //GetCampaignAnalyticsResponse cacheCampaignAnalytics = await _cacheService
            //    .GetCacheValueAsync<GetCampaignAnalyticsResponse>(cacheKey);

            //if (cacheCampaignAnalytics != null)
            //{
            //    return cacheCampaignAnalytics;
            //}

            GetCampaignAnalyticsResponse response = new();

            // get campaign's leads
            List<CampaignLead> campaignLeads = _campaignLeadRepository
                .GetAllCampaignLeadsAsync(CampaignId.Create(query.CampaignId), cancellationToken).Result;

            // calculate statuses based on campaign's channel
            if (campaign!.IsViber)
            {
                CampaignViberAnalytics campaignViberAnalytics = GetViberAnalytics(campaignLeads);
                
                int unsubscribedCount = await _campaignLeadRepository.CountUnsubscribedLeadsForCampaignId(CampaignId.Create(query.CampaignId), cancellationToken);

                campaignViberAnalytics.Unsubscribed = unsubscribedCount;
                
                response.Viber = campaignViberAnalytics;

                if (campaign.FallbackToSMS)
                {
                    CampaignSmsAnalytics campaignSmsAnalytics = GetSmsAnalytics(campaignLeads);

                    response.Sms = campaignSmsAnalytics;
                }
            }
            else if (campaign.IsSms)
            {
                CampaignSmsAnalytics campaignSmsAnalytics = GetSmsAnalytics(campaignLeads);

                response.Sms = campaignSmsAnalytics;
            }
            else
            {
                throw new Exception("No channel is selected for campaign.");
            }

            // set cache
            // await _cacheService.SetCacheValueAsync(cacheKey, response, TimeSpan.FromMinutes(1));

            return response;
        }

        private static CampaignViberAnalytics GetViberAnalytics(List<CampaignLead> campaignLeads)
        {
            int notSent = 0;
            int pending = 0;
            int delivered = 0;
            int undelivered = 0;
            int seen = 0;
            int received = 0;
            int clicked = 0;
            int expired = 0;

            foreach (CampaignLead campaignLead in campaignLeads)
            {
                switch (campaignLead.ViberStatus)
                {
                    case CampaignLeadViberStatus.None:
                        notSent++;
                        break;
                    case CampaignLeadViberStatus.Pending:
                        pending++;
                        break;
                    case CampaignLeadViberStatus.Delivered:
                        delivered++;
                        break;
                    case CampaignLeadViberStatus.Undelivered:
                        undelivered++;
                        break;
                    case CampaignLeadViberStatus.Seen:
                        seen++;
                        break;
                    case CampaignLeadViberStatus.Received:
                        received++;
                        break;
                    case CampaignLeadViberStatus.Expired:
                        expired++;
                        break;
                    case CampaignLeadViberStatus.Clicked:
                        clicked++;
                        break;
                }
            }

            int totalLeads = campaignLeads.Count;
            int sent = totalLeads - notSent;
            int funnelDelivered = delivered + seen + clicked;
            int funnelSeen = seen + clicked;

            return new CampaignViberAnalytics
            {
                Total = totalLeads,
                NotSent = notSent,
                Sent = sent,
                Pending = pending,
                Delivered = delivered,
                Undelivered = undelivered,
                Seen = seen,
                Received = received,
                Clicked = clicked,
                Expired = expired,
                FunnelDelivered = funnelDelivered,
                FunnelSeen = funnelSeen
            };
        }

        private static CampaignSmsAnalytics GetSmsAnalytics(List<CampaignLead> campaignLeads)
        {
            int notSent = 0;
            int pending = 0;
            int delivered = 0;
            int undelivered = 0;
            int blacklisted = 0;
            int error = 0;

            foreach (CampaignLead campaignLead in campaignLeads)
            {
                switch (campaignLead.SMSStatus)
                {
                    case CampaignLeadSMSStatus.None:
                        notSent++;
                        break;
                    case CampaignLeadSMSStatus.Pending:
                        pending++;
                        break;
                    case CampaignLeadSMSStatus.Delivered:
                        delivered++;
                        break;
                    case CampaignLeadSMSStatus.Undelivired:
                        undelivered++;
                        break;
                    case CampaignLeadSMSStatus.Blacklisted:
                        blacklisted++;
                        break;
                    case CampaignLeadSMSStatus.InvalidUsernameOrPassword:
                    case CampaignLeadSMSStatus.InvalidReference:
                    case CampaignLeadSMSStatus.ErrorDescription:
                    case CampaignLeadSMSStatus.Unknown:
                    case CampaignLeadSMSStatus.InvalidPhone:
                    case CampaignLeadSMSStatus.SendingError:
                        error++;
                        break;
                }
            }

            int totalLeads = campaignLeads.Count;
            int sent = totalLeads - notSent;

            return new CampaignSmsAnalytics
            {
                Total = totalLeads,
                NotSent = notSent,
                Sent = sent,
                Pending = pending,
                Delivered = delivered,
                Undelivered = undelivered,
                Blacklisted = blacklisted,
                Error = error,
                FunnelDelivered = delivered,
                FunnelSeen = 0
            };
        }
    }
}
