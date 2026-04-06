using System.Linq;
using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.CampaignAggregate.Repositories;
using OneClickEcho.Domain.Common.Shared;

namespace OneClickEcho.Application.Scheduling.Commands.QueueImmediateCampaigns
{
    public class QueueImmediateCampaignsHandler(ICampaignRepository campaignRepository)
        : ICommandHandler<QueueImmediateCampaignsCommand, QueueImmediateCampaignsResponse>
    {
        private readonly ICampaignRepository _campaignRepository = campaignRepository;

        public async Task<Result<QueueImmediateCampaignsResponse>> Handle(QueueImmediateCampaignsCommand request, CancellationToken cancellationToken)
        {
            // Console.WriteLine($"[{DateTime.UtcNow:O}] Checking immediate queued campaigns (Status=Queued, SendingType=Immediate)...");

            List<Domain.CampaignAggregate.Campaign> immediateCampaigns = await _campaignRepository
                .GetImmediateQueuedCampaigns(cancellationToken);

            string ids = immediateCampaigns.Count == 0
                ? "(none)"
                : string.Join(", ", immediateCampaigns.Select(c => c.Id.Value));
            // Console.WriteLine($"[{DateTime.UtcNow:O}] Immediate queued campaigns: {immediateCampaigns.Count}. IDs: {ids}");

            return new QueueImmediateCampaignsResponse(immediateCampaigns);
        }
    }
}
