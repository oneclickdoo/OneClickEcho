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
        Console.WriteLine("Checking campaigns between " + DateTime.Now.AddMinutes(-1) + " and " + DateTime.Now.AddMinutes(31) + "...");

        List<Domain.CampaignAggregate.Campaign> upcomingCampaigns = await _campaignRepository
            .GetScheduledCampaignsByStartDate(DateTime.Now.AddMinutes(-1), DateTime.Now.AddMinutes(31), cancellationToken);

        return new QueueUpcomingCampaignsResponse(upcomingCampaigns);
    }
}