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
/// Retries Viber bulk send for leads still at viber_status None, for up to 2 hours after launch.
/// Campaign stays InProgress until all Viber rows leave None or the window expires (then Completed anyway).
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

        List<Campaign> campaigns = await _campaignRepository.GetInProgressViberCampaignsAsync(cancellationToken);

        if (campaigns.Count == 0)
        {
            return;
        }

        // Console.WriteLine($"{DateTime.UtcNow:O} - RetryPendingViberCampaignSendsJob: {campaigns.Count} in-progress Viber campaign(s).");

        foreach (Campaign campaign in campaigns)
        {
            await ProcessCampaignAsync(campaign, cancellationToken);
        }
    }

    private async Task ProcessCampaignAsync(Campaign campaign, CancellationToken cancellationToken)
    {
        List<Lead> pendingLeads =
            await _campaignLeadRepository.GetLeadsByCampaignIdWithViberStatusNoneAsync(campaign.Id, cancellationToken);

        DateTime launchUtc = campaign.SendingDatetime.ToUniversalTime();
        bool pastRetryWindow = DateTime.UtcNow > launchUtc.AddHours(RetryWindowHours);

        if (pendingLeads.Count == 0)
        {
            await _mediator.Send(new CompleteCampaignCommand(campaign.Id.Value), cancellationToken);
            // Console.WriteLine($"{DateTime.UtcNow:O} - RetryPendingViber: campaign {campaign.Id.Value} — no Viber None left, marked Done.");
            return;
        }

        if (pastRetryWindow)
        {
            await _mediator.Send(new CompleteCampaignCommand(campaign.Id.Value), cancellationToken);
            // Console.WriteLine(
            //     $"{DateTime.UtcNow:O} - RetryPendingViber: campaign {campaign.Id.Value} — 2h window expired with {pendingLeads.Count} lead(s) still None, marked Done.");
            return;
        }

        try
        {
            await _messageSendingService.SendMessagesForCampaignId(
                campaign.Id,
                pendingLeads,
                viberOnlyForProvidedLeads: true);

            pendingLeads = await _campaignLeadRepository.GetLeadsByCampaignIdWithViberStatusNoneAsync(
                campaign.Id,
                cancellationToken);

            if (pendingLeads.Count == 0)
            {
                await _mediator.Send(new CompleteCampaignCommand(campaign.Id.Value), cancellationToken);
                // Console.WriteLine($"{DateTime.UtcNow:O} - RetryPendingViber: campaign {campaign.Id.Value} — retry cleared all None, marked Done.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{DateTime.UtcNow:O} - RetryPendingViber: campaign {campaign.Id.Value} FAILED: {ex}");
        }
    }
}
