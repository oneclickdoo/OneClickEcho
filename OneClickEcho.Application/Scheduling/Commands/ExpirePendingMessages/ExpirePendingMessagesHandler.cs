using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.CampaignAggregate.Repositories;
using OneClickEcho.Domain.CampaignLeadAggregate;
using OneClickEcho.Domain.CampaignLeadAggregate.Enums;
using OneClickEcho.Domain.CampaignLeadAggregate.Repositories;
using OneClickEcho.Domain.Common.Repositories;
using OneClickEcho.Domain.Common.Shared;

namespace OneClickEcho.Application.Scheduling.Commands.ExpirePendingMessages;

public class ExpirePendingMessagesHandler(ICampaignRepository campaignRepository,
    ICampaignLeadRepository campaignLeadRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<ExpirePendingMessagesCommand, ExpirePendingMessagesResponse>
{
    private readonly ICampaignRepository _campaignRepository = campaignRepository;
    private readonly ICampaignLeadRepository _campaignLeadRepository = campaignLeadRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    
    public async Task<Result<ExpirePendingMessagesResponse>> Handle(ExpirePendingMessagesCommand request, CancellationToken cancellationToken)
    {
        List<Domain.CampaignAggregate.Campaign> expiredCampaigns = await _campaignRepository.GetExpiredViberCampaigns(cancellationToken);

        int totalCampaignLeadsCount = 0;

        foreach (Domain.CampaignAggregate.Campaign expiredCampaign in expiredCampaigns)
        {
            List<CampaignLead> pendingCampaignLeads = await _campaignLeadRepository.GetPendingCampaignLeadsAsync(expiredCampaign.Id, cancellationToken);

            foreach (CampaignLead pendingCampaignLead in pendingCampaignLeads)
            {
                pendingCampaignLead.ViberStatus = CampaignLeadViberStatus.Expired;
            }
            
            totalCampaignLeadsCount += pendingCampaignLeads.Count;
        }
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ExpirePendingMessagesResponse(totalCampaignLeadsCount);
    }
}