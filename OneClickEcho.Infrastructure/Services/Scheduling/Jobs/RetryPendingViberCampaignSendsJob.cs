using MediatR;
using OneClickEcho.Application.Common.Services;
using OneClickEcho.Application.Scheduling.Commands.CompleteCampaign;
using OneClickEcho.Domain.CampaignAggregate;
using OneClickEcho.Domain.CampaignAggregate.Repositories;
using OneClickEcho.Domain.CampaignLeadAggregate.Repositories;
using OneClickEcho.Domain.LeadAggregate;
using Quartz;

namespace OneClickEcho.Infrastructure.Services.Scheduling.Jobs;

/// <summary>
/// Retries outbound sends for leads still at ViberStatus None and/or SMSStatus None (up to 2 hours after launch).
/// Campaign stays InProgress until no such rows remain or the window expires (then Completed anyway).
/// </summary>
[DisallowConcurrentExecution]
public class RetryPendingViberCampaignSendsJob(
    ICampaignRepository campaignRepository,
    ICampaignLeadRepository campaignLeadRepository,
    IMessageSendingService messageSendingService,
    IMediator mediator) : IJob
{
    private const int RetryWindowHours = 2;

    private readonly ICampaignRepository _campaignRepository = campaignRepository;
    private readonly ICampaignLeadRepository _campaignLeadRepository = campaignLeadRepository;
    private readonly IMessageSendingService _messageSendingService = messageSendingService;
    private readonly IMediator _mediator = mediator;

    public async Task Execute(IJobExecutionContext context)
    {
        CancellationToken cancellationToken = context.CancellationToken;

        List<Campaign> campaigns = await _campaignRepository.GetInProgressCampaignsForOutboundRetryAsync(cancellationToken);

        if (campaigns.Count == 0)
        {
            return;
        }

        foreach (Campaign campaign in campaigns)
        {
            await ProcessCampaignAsync(campaign, cancellationToken);
        }
    }

    private async Task ProcessCampaignAsync(Campaign campaign, CancellationToken cancellationToken)
    {
        List<Lead> pendingViber = campaign.IsViber
            ? await _campaignLeadRepository.GetLeadsByCampaignIdWithViberStatusNoneAsync(campaign.Id, cancellationToken)
            : [];

        List<Lead> pendingSms = campaign.IsSms
            ? await _campaignLeadRepository.GetLeadsByCampaignIdWithSmsStatusNoneAsync(campaign.Id, cancellationToken)
            : [];

        if (pendingViber.Count == 0 && pendingSms.Count == 0)
        {
            await _mediator.Send(new CompleteCampaignCommand(campaign.Id.Value), cancellationToken);
            return;
        }

        DateTime launchUtc = campaign.SendingDatetime.ToUniversalTime();
        bool pastRetryWindow = DateTime.UtcNow > launchUtc.AddHours(RetryWindowHours);

        if (pastRetryWindow)
        {
            await _mediator.Send(new CompleteCampaignCommand(campaign.Id.Value), cancellationToken);
            return;
        }

        try
        {
            if (pendingViber.Count > 0)
            {
                await _messageSendingService.SendMessagesForCampaignId(
                    campaign.Id,
                    pendingViber,
                    viberOnlyForProvidedLeads: true,
                    smsOnlyForProvidedLeads: false);
            }

            if (pendingSms.Count > 0)
            {
                await _messageSendingService.SendMessagesForCampaignId(
                    campaign.Id,
                    pendingSms,
                    viberOnlyForProvidedLeads: false,
                    smsOnlyForProvidedLeads: true);
            }

            pendingViber = campaign.IsViber
                ? await _campaignLeadRepository.GetLeadsByCampaignIdWithViberStatusNoneAsync(campaign.Id, cancellationToken)
                : [];
            pendingSms = campaign.IsSms
                ? await _campaignLeadRepository.GetLeadsByCampaignIdWithSmsStatusNoneAsync(campaign.Id, cancellationToken)
                : [];

            if (pendingViber.Count == 0 && pendingSms.Count == 0)
            {
                await _mediator.Send(new CompleteCampaignCommand(campaign.Id.Value), cancellationToken);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{DateTime.UtcNow:O} - RetryPendingOutbound: campaign {campaign.Id.Value} FAILED: {ex}");
        }
    }
}
