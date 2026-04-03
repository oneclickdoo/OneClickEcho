using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Application.Common.Services;
using OneClickEcho.Domain.CampaignLeadAggregate.Repositories;
using OneClickEcho.Domain.Common.Shared;

namespace OneClickEcho.Application.Scheduling.Commands.SendBirthdayMessages;

public class SendBirthdayMessagesHandler(ICampaignLeadRepository campaignLeadRepository,
    IMessageSendingService messageSendingService) : ICommandHandler<SendBirthdayMessagesCommand, SendBirthdayMessagesResponse>
{
    private readonly ICampaignLeadRepository _campaignLeadRepository = campaignLeadRepository;
    private readonly IMessageSendingService _messageSendingService = messageSendingService;

    public async Task<Result<SendBirthdayMessagesResponse>> Handle(SendBirthdayMessagesCommand request,
        CancellationToken cancellationToken)
    {
        List<MessageSendingCampaignLeadDto> campaignLeads = await _campaignLeadRepository
            .GetAllLeadsForDateOfBirthAsync(DateOnly.FromDateTime(DateTime.Today), cancellationToken);

        Console.WriteLine($"{DateTime.Now} - Found {campaignLeads.Count} campaigns with leads with a DoB for today.");

        foreach (MessageSendingCampaignLeadDto campaignLead in campaignLeads)
        {
            Console.WriteLine($"{DateTime.Now} - Sending birthday messages for campaign: {campaignLead.CampaignId.Value}");
            await _messageSendingService.SendMessagesForCampaignId(campaignLead.CampaignId, campaignLead.Leads);
        }

        return new SendBirthdayMessagesResponse();
    }
}