using OneClickEcho.Application.Campaign.Queries.GetCampaignById;
using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.CampaignAggregate.Repositories;
using OneClickEcho.Domain.CampaignAggregate.ValueObjects;
using OneClickEcho.Domain.CampaignLeadAggregate.Repositories;
using OneClickEcho.Domain.Common.Models;
using OneClickEcho.Domain.Common.Shared;
using System.Text;

namespace OneClickEcho.Application.Campaign.Queries.ExportCampaignLeads
{
    public class ExportCampaignLeadsHandler(ICampaignLeadRepository campaignLeadRepository, ICampaignRepository campaignRepository)
        : IQueryHandler<ExportCampaignLeadsQuery, ExportCampaignLeadsResponse>
    {
        private readonly ICampaignLeadRepository _campaignLeadRepository = campaignLeadRepository;
        private readonly ICampaignRepository _campaignRepository = campaignRepository;

        public async Task<Result<ExportCampaignLeadsResponse>> Handle(ExportCampaignLeadsQuery query, CancellationToken cancellationToken)
        {
            // get campaign
            Domain.CampaignAggregate.Campaign? campaign = await _campaignRepository
                .GetByIdAsync(CampaignId.Create(query.CampaignId), cancellationToken);

            if (campaign is null)
            {
                Result.Failure<GetCampaignByIdResponse>(new Error(
                    "Campaign.NotFound",
                    $"The Campaign with Id:\"{query.CampaignId}\" does not exist."
                    ));
            }

            // get campaign leads
            List<LeadWithCampaignLeadDto> campaignLeadDtos = await _campaignLeadRepository
                .ExportCampaignLeads(
                    CampaignId.Create(query.CampaignId),
                    smsStatus: query.SmsStatus,
                    viberStatus: query.ViberStatus,
                    cancellationToken: cancellationToken);

            // create CSV content
            StringBuilder csvContent = new();

            // append header row
            csvContent.AppendLine("Phone number,First name,Last name,Gender,Email,Date of birth,City,State,Country," +
                "Is blacklisted,Viber status,Viber status description,SMS status,SMS status description");

            // append data rows
            foreach (LeadWithCampaignLeadDto dto in campaignLeadDtos)
            {
                csvContent.AppendLine($"{dto.Lead.PhoneNumber},{dto.Lead.FirstName},{dto.Lead.LastName},{dto.Lead.Gender}," +
                    $"{dto.Lead.Email},{dto.Lead.DateOfBirth},{dto.Lead.City},{dto.Lead.State},{dto.Lead.Country}," +
                    $"{dto.CampaignLead.IsBlacklisted},{dto.CampaignLead.ViberStatus},{dto.CampaignLead.ViberStatusDescription}," +
                    $"{dto.CampaignLead.SMSStatus},{dto.CampaignLead.SMSStatusDescription}");
            }

            // encode CSV content
            byte[] fileBytes = Encoding.UTF8.GetBytes(csvContent.ToString());

            return new ExportCampaignLeadsResponse(fileBytes);
        }
    }
}
