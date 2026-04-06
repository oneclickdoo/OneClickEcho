using System.Linq;
using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.CampaignAggregate.Repositories;
using OneClickEcho.Domain.Common.Shared;

namespace OneClickEcho.Application.Scheduling.Commands.QueueUpcomingCampaigns;

public class QueueUpcomingCampaignsHandler(ICampaignRepository campaignRepository)
    : ICommandHandler<QueueUpcomingCampaignsCommand, QueueUpcomingCampaignsResponse>
{
    private readonly ICampaignRepository _campaignRepository = campaignRepository;

    public async Task<Result<QueueUpcomingCampaignsResponse>> Handle(QueueUpcomingCampaignsCommand request, CancellationToken cancellationToken)
    {
        DateTime startUtc = DateTime.UtcNow.AddMinutes(-1);
        DateTime endUtc = DateTime.UtcNow.AddMinutes(31);
        // Console.WriteLine($"[{DateTime.UtcNow:O}] Checking scheduled campaigns (UTC window {startUtc:O} .. {endUtc:O})...");

        List<Domain.CampaignAggregate.Campaign> upcomingCampaigns = await _campaignRepository
            .GetScheduledCampaignsByStartDate(startUtc, endUtc, cancellationToken);

        string ids = upcomingCampaigns.Count == 0
            ? "(none)"
            : string.Join(", ", upcomingCampaigns.Select(c => c.Id.Value));
        // Console.WriteLine($"[{DateTime.UtcNow:O}] Scheduled campaigns in window: {upcomingCampaigns.Count}. IDs: {ids}");

        return new QueueUpcomingCampaignsResponse(upcomingCampaigns);
    }
}