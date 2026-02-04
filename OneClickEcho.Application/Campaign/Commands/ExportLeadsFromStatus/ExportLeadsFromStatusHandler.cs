using System.Text;
using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.CampaignAggregate.Repositories;
using OneClickEcho.Domain.CampaignAggregate.ValueObjects;
using OneClickEcho.Domain.CampaignLeadAggregate.Repositories;
using OneClickEcho.Domain.Common.Shared;

namespace OneClickEcho.Application.Campaign.Commands.ExportLeadsFromStatus;

public class ExportLeadsFromStatusHandler : ICommandHandler<ExportLeadsFromStatusCommand, ExportLeadsFromStatusResponse>
{
    private readonly ICampaignRepository _campaignRepository;
    private readonly ICampaignLeadRepository _campaignLeadRepository;

    public ExportLeadsFromStatusHandler(ICampaignRepository campaignRepository, ICampaignLeadRepository campaignLeadRepository)
    {
        _campaignRepository = campaignRepository;
        _campaignLeadRepository = campaignLeadRepository;
    }

    public async Task<Result<ExportLeadsFromStatusResponse>> Handle(ExportLeadsFromStatusCommand request, CancellationToken cancellationToken)
    {
        var campaign = await _campaignRepository.GetByIdAsync(CampaignId.Create(request.CampaignId), cancellationToken);
        
        if (campaign is null)
        {
            return Result.Failure<ExportLeadsFromStatusResponse>(new Error(
                "Campaign.NotFound",
                "Campaign not found."
            ));
        }

        var leads = await _campaignLeadRepository.GetLeadsByCampaignIdAndStatusAsync(
            campaign, request.ViberStatus, request.SmsStatus, cancellationToken);
        
        StringBuilder csvContent = new();

        // append header row
        csvContent.AppendLine("PhoneNumber,FirstName,LastName,Gender,Email,DateOfBirth,City,State,Country");
        
        // append data rows
        foreach (Domain.LeadAggregate.Lead lead in leads)
        {
            csvContent.AppendLine($"{lead.PhoneNumber},{lead.FirstName},{lead.LastName},{lead.Gender}," +
                                  $"{lead.Email},{lead.DateOfBirth},{lead.City},{lead.State},{lead.Country}");
        } 

        // encode CSV content
        byte[] fileBytes = Encoding.UTF8.GetBytes(csvContent.ToString());

        return new ExportLeadsFromStatusResponse(fileBytes);
    }
}