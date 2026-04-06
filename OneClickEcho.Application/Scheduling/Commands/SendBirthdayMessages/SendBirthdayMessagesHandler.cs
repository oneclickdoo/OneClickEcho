using MediatR;
using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Application.Common.Services;
using OneClickEcho.Application.Scheduling.Commands.EnqueueCampaign;
using OneClickEcho.Domain.CampaignLeadAggregate.Repositories;
using OneClickEcho.Domain.Common.Shared;

namespace OneClickEcho.Application.Scheduling.Commands.SendBirthdayMessages;

public class SendBirthdayMessagesHandler(ICampaignLeadRepository campaignLeadRepository,
    IMessageSendingService messageSendingService,
    IMediator mediator) : ICommandHandler<SendBirthdayMessagesCommand, SendBirthdayMessagesResponse>
{
    private readonly ICampaignLeadRepository _campaignLeadRepository = campaignLeadRepository;
    private readonly IMessageSendingService _messageSendingService = messageSendingService;
    private readonly IMediator _mediator = mediator;

    public async Task<Result<SendBirthdayMessagesResponse>> Handle(SendBirthdayMessagesCommand request,
        CancellationToken cancellationToken)
    {
        List<MessageSendingCampaignLeadDto> campaignLeads = await _campaignLeadRepository
            .GetAllLeadsForDateOfBirthAsync(DateOnly.FromDateTime(DateTime.Today), cancellationToken);

        // Console.WriteLine($"{DateTime.Now} - Found {campaignLeads.Count} campaigns with leads with a DoB for today.");

        foreach (MessageSendingCampaignLeadDto campaignLead in campaignLeads)
        {
            // Console.WriteLine($"{DateTime.Now} - Sending birthday messages for campaign: {campaignLead.CampaignId.Value}");
            Result<EnqueueCampaignResponse> enqueue =
                await _mediator.Send(new EnqueueCampaignCommand(campaignLead.CampaignId.Value), cancellationToken);
            if (enqueue.IsFailure)
            {
                Console.WriteLine($"{DateTime.Now} - Birthday send skipped (enqueue failed): {enqueue.Error.Message}");
                continue;
            }

            if (!enqueue.Value.ClaimedFromQueued)
            {
                Console.WriteLine($"{DateTime.Now} - Birthday send skipped: campaign {campaignLead.CampaignId.Value} not Queued or already claimed.");
                continue;
            }

            await _messageSendingService.SendMessagesForCampaignId(campaignLead.CampaignId, campaignLead.Leads);
        }

        return new SendBirthdayMessagesResponse();
    }
}