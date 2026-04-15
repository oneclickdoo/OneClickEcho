using System.Globalization;
using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.CampaignAggregate.Repositories;
using OneClickEcho.Domain.CampaignLeadAggregate.Enums;
using OneClickEcho.Domain.CampaignLeadAggregate.Repositories;
using OneClickEcho.Domain.Common.Shared;
using OneClickEcho.Domain.CompanyAggregate.Repositories;

namespace OneClickEcho.Application.Admin.Queries.GetAdminAnalytics
{
    public class GetAdminAnalyticsHandler(ICompanyRepository companyRepository, ICampaignRepository campaignRepository,
        ICampaignLeadRepository campaignLeadRepository) : IQueryHandler<GetAdminAnalyticsQuery, GetAdminAnalyticsResponse>
    {
        private readonly ICompanyRepository _companyRepository = companyRepository;
        private readonly ICampaignRepository _campaignRepository = campaignRepository;
        private readonly ICampaignLeadRepository _campaignLeadRepository = campaignLeadRepository;

        // @TODO: add constants in database
        private readonly decimal VIBER_PRICE_PER_MESSAGE = 0.0081M;
        private readonly decimal SMS_PRICE_PER_MESSAGE = 0.013M;

        public async Task<Result<GetAdminAnalyticsResponse>> Handle(GetAdminAnalyticsQuery request, CancellationToken cancellationToken)
        {
            DateTime? startDate = null;
            DateTime? endDate = null;

            if (!TryParseAdminFilterDate(request.StartDate, out startDate, out string? startErr))
            {
                return Result.Failure<GetAdminAnalyticsResponse>(
                    new Error("AdminAnalytics.InvalidStartDate", startErr ?? "Invalid startDate."));
            }

            if (!TryParseAdminFilterDate(request.EndDate, out endDate, out string? endErr))
            {
                return Result.Failure<GetAdminAnalyticsResponse>(
                    new Error("AdminAnalytics.InvalidEndDate", endErr ?? "Invalid endDate."));
            }

            static bool TryParseAdminFilterDate(string? raw, out DateTime? utc, out string? error)
            {
                utc = null;
                error = null;
                if (string.IsNullOrWhiteSpace(raw))
                {
                    return true;
                }

                if (DateTimeOffset.TryParse(
                        raw,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.RoundtripKind,
                        out DateTimeOffset dto))
                {
                    utc = dto.UtcDateTime;
                    return true;
                }

                error = "Use an ISO 8601 date/time (e.g. 2026-03-31T22:00:00.000Z).";
                return false;
            }

            int totalViberMessagesSent = 0;
            int totalViberMessagesDelivered = 0;
            int totalSmsMessagesSent = 0;
            int totalSmsMessagesDelivered = 0;
            decimal totalRevenue = 0;
            decimal totalProfit = 0;

            // get companies
            List<Domain.CompanyAggregate.Company> companies = await _companyRepository.GetAllAsync(cancellationToken);

            List<AdminCompanyAnalyticsDto> companyAnalyticsDtos = [];

            foreach (Domain.CompanyAggregate.Company company in companies)
            {
                int viberMessagesSent = 0;
                int viberMessagesDelivered = 0;
                int smsMessagesSent = 0;
                int smsMessagesDelivered = 0;
                decimal revenue = 0;
                decimal profit = 0;

                // get all company's campaigns
                List<Domain.CampaignAggregate.Campaign> campaigns = await _campaignRepository
                    .GetAllByCompanyId(company.Id, cancellationToken);

                // check for date
                if (startDate != null && endDate != null)
                {
                    campaigns = campaigns.Where(c => c.CreatedAt >= startDate && c.CreatedAt <= endDate)
                        .ToList();
                }

                foreach (Domain.CampaignAggregate.Campaign campaign in campaigns)
                {
                    // get campaign's leads
                    List<Domain.CampaignLeadAggregate.CampaignLead> campaignLeads = await _campaignLeadRepository
                        .GetAllCampaignLeadsAsync(campaign.Id, cancellationToken);

                    // check for date
                    if (startDate != null && endDate != null)
                    {
                        campaignLeads = campaignLeads.Where(cl => cl.CreatedAt >= startDate && cl.CreatedAt <= endDate)
                            .ToList();
                    }

                    foreach (Domain.CampaignLeadAggregate.CampaignLead campaignLead in campaignLeads)
                    {
                        if (campaignLead.ViberStatus != CampaignLeadViberStatus.None)
                        {

                            viberMessagesSent++;
                        }

                        List<CampaignLeadViberStatus> deliveredStatuses = [
                            CampaignLeadViberStatus.Delivered,
                            CampaignLeadViberStatus.Received,
                            CampaignLeadViberStatus.Seen,
                            CampaignLeadViberStatus.Clicked
                            ];

                        if (deliveredStatuses.Contains(campaignLead.ViberStatus))
                        {
                            viberMessagesDelivered++;
                        }

                        if (campaignLead.SMSStatus != CampaignLeadSMSStatus.None)
                        {

                            smsMessagesSent++;
                        }

                        if (campaignLead.SMSStatus == CampaignLeadSMSStatus.Delivered)
                        {
                            smsMessagesDelivered++;
                        }
                    }
                }

                // calculate revenue and profit for company
                revenue = (viberMessagesDelivered * company.ViberPricePerMesssage) + (smsMessagesSent * company.SmsPricePerMesssage);

                profit = (viberMessagesDelivered * (company.ViberPricePerMesssage - VIBER_PRICE_PER_MESSAGE))
                    + (smsMessagesSent * (company.SmsPricePerMesssage - SMS_PRICE_PER_MESSAGE));

                // accumulate
                totalViberMessagesSent += viberMessagesSent;
                totalViberMessagesDelivered += viberMessagesDelivered;
                totalSmsMessagesSent += smsMessagesSent;
                totalSmsMessagesDelivered += smsMessagesDelivered;
                totalRevenue += revenue;
                totalProfit += profit;

                // add company to list
                AdminCompanyAnalyticsDto companyAnalyticsDto = new()
                {
                    CompanyId = company.Id.Value,
                    Name = company.Name,
                    ViberMessagesSent = viberMessagesSent,
                    ViberMessagesDelivered = viberMessagesDelivered,
                    SmsMessagesSent = smsMessagesSent,
                    SmsMessagesDelivered = smsMessagesDelivered,
                    Revenue = revenue,
                    Profit = profit
                };

                companyAnalyticsDtos.Add(companyAnalyticsDto);
            }

            return new GetAdminAnalyticsResponse()
            {
                TotalViberMessagesSent = totalViberMessagesSent,
                TotalViberMessagesDelivered = totalViberMessagesDelivered,
                TotalSmsMessagesSent = totalSmsMessagesSent,
                TotalSmsMessagesDelivered = totalSmsMessagesDelivered,
                TotalRevenue = totalRevenue,
                TotalProfit = totalProfit,
                Companies = companyAnalyticsDtos
            };
        }
    }
}
