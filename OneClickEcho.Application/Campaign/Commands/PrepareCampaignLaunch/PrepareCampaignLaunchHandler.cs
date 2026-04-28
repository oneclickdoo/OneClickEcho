using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Application.Common.Services;
using OneClickEcho.Domain.Common;
using OneClickEcho.Domain.CampaignAggregate.Enums;
using OneClickEcho.Domain.CampaignAggregate.Repositories;
using OneClickEcho.Domain.CampaignAggregate.ValueObjects;
using OneClickEcho.Domain.CampaignLeadAggregate;
using OneClickEcho.Domain.CampaignLeadAggregate.Repositories;
using OneClickEcho.Domain.Common.Repositories;
using OneClickEcho.Domain.Common.Shared;
using OneClickEcho.Domain.LeadAggregate.Repositories;
using OneClickEcho.Domain.LeadCollectionAggregate.Repositories;

namespace OneClickEcho.Application.Campaign.Commands.PrepareCampaignLaunch;

public class PrepareCampaignLaunchHandler(
    ICampaignRepository campaignRepository,
    ICampaignLeadRepository campaignLeadRepository,
    ILeadCollectionRepository leadCollectionRepository,
    ILeadRepository leadRepository,
    IUnitOfWork unitOfWork,
    ICampaignLaunchScheduler campaignLaunchScheduler)
    : ICommandHandler<PrepareCampaignLaunchCommand>
{
    private readonly ICampaignRepository _campaignRepository = campaignRepository;
    private readonly ICampaignLeadRepository _campaignLeadRepository = campaignLeadRepository;
    private readonly ILeadCollectionRepository _leadCollectionRepository = leadCollectionRepository;
    private readonly ILeadRepository _leadRepository = leadRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICampaignLaunchScheduler _campaignLaunchScheduler = campaignLaunchScheduler;

    public async Task<Result> Handle(PrepareCampaignLaunchCommand request, CancellationToken cancellationToken)
    {
        Domain.CampaignAggregate.Campaign? campaign = await _campaignRepository
            .GetByIdAsync(CampaignId.Create(request.CampaignId), cancellationToken);

        if (campaign is null)
        {
            return Result.Failure(new Error(
                "Campaign.NotFound",
                $"The Campaign with Id:\"{request.CampaignId}\" does not exist."));
        }

        if (campaign.Status != CampaignStatus.PreparingLaunch)
        {
            return Result.Success();
        }

        List<Domain.LeadCollectionAggregate.LeadCollection> leadCollections = await _leadCollectionRepository
            .GetAllByCampaignIdAsync(campaign.Id, cancellationToken);

        if (leadCollections == null || leadCollections.Count == 0)
        {
            campaign.Status = CampaignStatus.Draft;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Failure(new Error(
                "Campaign.BadRequest",
                "No lead collections found for this campaign."));
        }

        List<CampaignLead> prevCampaignLeads = await _campaignLeadRepository.GetAllCampaignLeadsAsync(campaign.Id, cancellationToken);

        foreach (CampaignLead prevCampaignLead in prevCampaignLeads)
        {
            _campaignLeadRepository.Delete(prevCampaignLead);
        }

        Dictionary<string, Domain.LeadAggregate.Lead> leadByNormalizedPhone = new(StringComparer.Ordinal);
        foreach (Domain.LeadCollectionAggregate.LeadCollection leadCollection in leadCollections)
        {
            List<Domain.LeadAggregate.Lead> listOfLeads =
                await _leadRepository.GetAllByCollectionIdAsync(leadCollection.Id, cancellationToken);

            foreach (Domain.LeadAggregate.Lead lead in listOfLeads)
            {
                if (lead.IsBlacklisted)
                {
                    continue;
                }

                if (lead.IsUnsubscribed)
                {
                    continue;
                }

                string key = PhoneNumberHelper.NormalizeKey(lead.PhoneNumber);
                if (string.IsNullOrEmpty(key))
                {
                    continue;
                }

                if (!leadByNormalizedPhone.TryGetValue(key, out Domain.LeadAggregate.Lead? existing))
                {
                    leadByNormalizedPhone[key] = lead;
                    continue;
                }

                leadByNormalizedPhone[key] = PickBetterLeadForCampaign(existing, lead, key);
            }
        }

        List<Domain.LeadAggregate.Lead> internalListOfLeads = leadByNormalizedPhone.Values.ToList();

        List<CampaignLead> newCampaignLeads = new(internalListOfLeads.Count);
        foreach (Domain.LeadAggregate.Lead lead in internalListOfLeads)
        {
            string canonical = PhoneNumberHelper.Standardize(lead.PhoneNumber);
            if (!string.IsNullOrEmpty(canonical) && lead.PhoneNumber != canonical)
            {
                lead.PhoneNumber = canonical;
            }

            CampaignLead campaignLead = new(lead.Id, campaign.Id);
            newCampaignLeads.Add(campaignLead);
            _campaignLeadRepository.Add(campaignLead);
        }

        await _campaignLeadRepository.AssignSequentialGlobalViberMessageIdsAsync(newCampaignLeads, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _campaignLeadRepository.SyncCampaignLeadViberMessageIdSequenceAsync(cancellationToken);

        List<Domain.LeadAggregate.Lead> leads = await _campaignLeadRepository
            .GetAllLeadsByCampaignIdAsync(campaign.Id, cancellationToken);

        if (leads.Count == 0)
        {
            campaign.Status = CampaignStatus.Draft;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Failure(new Error(
                "Campaign.BadRequest",
                "The Campaign doesn't have leads."));
        }

        campaign.Status = CampaignStatus.Queued;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (campaign.SendingType == CampaignSendingType.Immediate)
        {
            await _campaignLaunchScheduler.ScheduleImmediateSendJobAsync(campaign.Id.Value, cancellationToken);
        }

        return Result.Success();
    }

    private static Domain.LeadAggregate.Lead PickBetterLeadForCampaign(
        Domain.LeadAggregate.Lead a,
        Domain.LeadAggregate.Lead b,
        string canonicalPhone)
    {
        int scoreA = LeadRichnessScore(a, canonicalPhone);
        int scoreB = LeadRichnessScore(b, canonicalPhone);
        if (scoreB > scoreA)
        {
            return b;
        }

        if (scoreA > scoreB)
        {
            return a;
        }

        return a.Id.Value.CompareTo(b.Id.Value) <= 0 ? a : b;
    }

    private static int LeadRichnessScore(Domain.LeadAggregate.Lead lead, string canonicalPhone)
    {
        int score = 0;
        if (string.Equals(lead.PhoneNumber, canonicalPhone, StringComparison.Ordinal))
        {
            score += 100;
        }

        if (!string.IsNullOrWhiteSpace(lead.FirstName))
        {
            score += 4;
        }

        if (!string.IsNullOrWhiteSpace(lead.LastName))
        {
            score += 4;
        }

        if (!string.IsNullOrWhiteSpace(lead.Email))
        {
            score += 2;
        }

        if (lead.DateOfBirth.HasValue)
        {
            score += 2;
        }

        if (!string.IsNullOrWhiteSpace(lead.City))
        {
            score += 1;
        }

        score += Math.Min(lead.PhoneNumber?.Length ?? 0, 20);
        return score;
    }
}
