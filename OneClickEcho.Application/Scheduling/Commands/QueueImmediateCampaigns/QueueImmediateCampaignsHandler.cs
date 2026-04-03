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
            Console.WriteLine("Checking immediate queued campaigns...");

            List<Domain.CampaignAggregate.Campaign> immediateCampaigns = await _campaignRepository
                .GetImmediateQueuedCampaigns(cancellationToken);

            return new QueueImmediateCampaignsResponse(immediateCampaigns);
        }
    }
}
