using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.CampaignAggregate.Enums;
using OneClickEcho.Domain.CampaignAggregate.Repositories;
using OneClickEcho.Domain.CampaignAggregate.ValueObjects;
using OneClickEcho.Domain.CampaignLeadAggregate;
using OneClickEcho.Domain.CampaignLeadAggregate.Repositories;
using OneClickEcho.Domain.Common.Repositories;
using OneClickEcho.Domain.Common.Shared;
using OneClickEcho.Domain.LeadAggregate.Repositories;
using OneClickEcho.Domain.LeadCollectionAggregate.Repositories;

namespace OneClickEcho.Application.Campaign.Commands.LaunchCampaign
{
    public class LaunchCampaignHandler(ICampaignRepository campaignRepository,
        ICampaignLeadRepository campaignLeadRepository, 
        ILeadCollectionRepository leadCollectionRepository,
        ILeadRepository leadRepository,
        IUnitOfWork unitOfWork)
        : ICommandHandler<LaunchCampaignCommand, LaunchCampaignResponse>
    {
        private readonly ICampaignRepository _campaignRepository = campaignRepository;
        private readonly ILeadCollectionRepository _leadCollectionRepository = leadCollectionRepository;
        private readonly ILeadRepository _leadRepository = leadRepository;
        private readonly ICampaignLeadRepository _campaignLeadRepository = campaignLeadRepository;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<Result<LaunchCampaignResponse>> Handle(LaunchCampaignCommand request, CancellationToken cancellationToken)
        {
            // get campaign
            Domain.CampaignAggregate.Campaign? campaign = await _campaignRepository
                .GetByIdAsync(CampaignId.Create(request.CampaignId), cancellationToken);

            if (campaign is null)
            {
                return Result.Failure<LaunchCampaignResponse>(new Error(
                    "Campaign.NotFound",
                    $"The Campaign with Id:\"{request.CampaignId}\" does not exist."
                ));
            }

            // campaign must have SMS or Viber (or both)
            if (!campaign.IsSms && !campaign.IsViber)
            {
                return Result.Failure<LaunchCampaignResponse>(new Error(
                    "Campaign.BadRequest",
                    "The Campaign does not have selected channel."
                ));
            }
            
            // Assign Leads to the Campaign using Lead Collections
            
            // Fetch all lead collections related to the campaign
            List<Domain.LeadCollectionAggregate.LeadCollection> leadCollections = await _leadCollectionRepository
                .GetAllByCampaignIdAsync(campaign.Id, cancellationToken);

            if (leadCollections == null || leadCollections.Count == 0)
            {
                return Result.Failure<LaunchCampaignResponse>(new Error(
                    "Campaign.BadRequest",
                    "No lead collections found for this campaign."
                ));
            }
            
            var prevCampaignLeads = await _campaignLeadRepository.GetAllCampaignLeadsAsync(campaign.Id, cancellationToken);

            foreach (var prevCampaignLead in prevCampaignLeads)
            {
                _campaignLeadRepository.Delete(prevCampaignLead);
            }

            List<Domain.LeadAggregate.Lead> internalListOfLeads = new List<Domain.LeadAggregate.Lead>();
            // Assign the lead collections to the campaign
            foreach (var leadCollection in leadCollections)
            {
                var listOfLeads = await _leadRepository.GetAllByCollectionIdAsync(leadCollection.Id, cancellationToken);
                
                foreach (var lead in listOfLeads)
                {
                    if (lead.IsBlacklisted) continue;
                    if (lead.IsUnsubscribed) continue;
                    if (internalListOfLeads.Contains(lead)) continue;
                    internalListOfLeads.Add(lead);
                }
            }

            foreach (var lead in internalListOfLeads)
            {
                var campaignLead = new CampaignLead(lead.Id, campaign.Id);
                _campaignLeadRepository.Add(campaignLead);
            }
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // campaign must have leads
            List<Domain.LeadAggregate.Lead> leads = _campaignLeadRepository
                .GetAllLeadsByCampaignIdAsync(campaign.Id, cancellationToken).Result;

            if (leads.Count == 0)
            {
                return Result.Failure<LaunchCampaignResponse>(new Error(
                    "Campaign.BadRequest",
                    "The Campaign doesn't have leads."
                ));
            }

            if (campaign.SendingType == CampaignSendingType.Immediate)
            {
                // add campaign to queue
                campaign.Status = CampaignStatus.Queued;
                campaign.SendingDatetime = DateTime.UtcNow;
            }
            else
            {
                // campaign must have sending datetime
                if (campaign.SendingDatetime == null)
                {
                    return Result.Failure<LaunchCampaignResponse>(new Error(
                        "Campaign.BadRequest",
                        "The Campaign does not have sending datetime."
                    ));
                }

                DateTime currentDateTime = DateTime.UtcNow;

                // campaign must start in the future
                if (currentDateTime > campaign.SendingDatetime)
                {
                    return Result.Failure<LaunchCampaignResponse>(new Error(
                        "Campaign.BadRequest",
                        "The Campaign sending datetime is passed."
                    ));
                }

                // add campaign to queue
                campaign.Status = CampaignStatus.Queued;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new LaunchCampaignResponse(campaign.Id.Value);
        }
    }
}
