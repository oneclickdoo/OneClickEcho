using OneClickEcho.Domain.CampaignAggregate;
using OneClickEcho.Domain.CampaignAggregate.ValueObjects;
using OneClickEcho.Domain.CampaignLeadAggregate.Entities;
using OneClickEcho.Domain.CampaignLeadAggregate.Enums;
using OneClickEcho.Domain.CampaignLeadAggregate.ValueObjects;
using OneClickEcho.Domain.Common.Models;
using OneClickEcho.Domain.Common.Queries;
using OneClickEcho.Domain.Common.Repositories;
using OneClickEcho.Domain.LeadAggregate;
using OneClickEcho.Domain.LeadAggregate.ValueObjects;

namespace OneClickEcho.Domain.CampaignLeadAggregate.Repositories;

// @TODO: Better place to put this?
public class MessageSendingCampaignLeadDto
{
    public CampaignId CampaignId { get; set; } = default!;

    public List<Lead> Leads { get; set; } = default!;
}

public class CountCampaignLeadsByStatusDto
{
    public int PendingCount { get; set; } = 0;
    public int DeliveredCount { get; set; } = 0;
    public int UndeliveredCount { get; set; } = 0;
}

public interface ICampaignLeadRepository : IRepository<CampaignLead, CampaignLeadId>
{
    Task<CampaignLead?> GetByIdAsync(CampaignLeadId id, CancellationToken cancellationToken = default);

    public Task<CampaignLead?> GetByCampaignAndLeadId(CampaignId campaignId, LeadId leadId,
        CancellationToken cancellationToken = default);

    Task<List<Lead>> GetAllLeadsByCampaignIdAsync(CampaignId campaignId, CancellationToken cancellationToken = default);

    /// <summary>Leads whose campaign_lead row still has ViberStatus None (not yet accepted by gateway for this campaign).</summary>
    Task<List<Lead>> GetLeadsByCampaignIdWithViberStatusNoneAsync(CampaignId campaignId,
        CancellationToken cancellationToken = default);

    /// <summary>Leads whose campaign_lead row still has SMSStatus None (SMS not yet sent for this campaign).</summary>
    Task<List<Lead>> GetLeadsByCampaignIdWithSmsStatusNoneAsync(CampaignId campaignId,
        CancellationToken cancellationToken = default);

    public Task<IPagedList<Lead>> GetPagedLeadsByCampaignIdAsync(CampaignId campaignId, IPagedQuery query,
        CancellationToken cancellationToken = default);

    Task<List<CampaignLead>> GetAllCampaignLeadsAsync(CampaignId campaignId, CancellationToken cancellationToken = default);
    
    Task<List<CampaignLead>> GetPendingCampaignLeadsAsync(CampaignId campaignId, CancellationToken cancellationToken = default);
    
    Task<List<CampaignLead>> GetNonTerminalCampaignLeadsForCampaignIdsAsync(List<CampaignId> campaignIds, CancellationToken cancellationToken = default);
    
    Task<List<CampaignLead>> GetAnswerableCampaignLeadsForCampaignIdsAsync(List<CampaignId> campaignIds, CancellationToken cancellationToken = default);
    
    Task<int> CountUnsubscribedLeadsForCampaignId(CampaignId campaignId, CancellationToken cancellationToken = default);
    
    Task<List<Lead>> GetLeadsByCampaignIdAndStatusAsync(Campaign campaign, CampaignLeadViberStatus? campaignLeadViberStatus,
        CampaignLeadSMSStatus? campaignLeadSmsStatus, CancellationToken cancellationToken = default);

    public Task<CountCampaignLeadsByStatusDto> CountCampaignLeadsByStatus(Campaign campaign, CancellationToken cancellationToken = default);

    public Task<List<LeadWithCampaignLeadDto>> ExportCampaignLeads(CampaignId campaignId,
        CampaignLeadSMSStatus? smsStatus = null, CampaignLeadViberStatus? viberStatus = null, CancellationToken cancellationToken = default);

    Task<List<MessageSendingCampaignLeadDto>> GetAllLeadsForDateOfBirthAsync(DateOnly dateOfBirth,
        CancellationToken cancellationToken = default);

    void Add(CampaignLead campaignLead);
    
    void Delete(CampaignLead campaignLead);

    Task AddReceivedMessages(List<ReceivedMessage> receivedMessages);

    /// <summary>
    /// Sets <see cref="CampaignLead.ViberMessageId"/> on each lead to consecutive values starting at global max + 1.
    /// Uses a short DB transaction and pg_advisory_xact_lock. Call after <see cref="Add"/> while leads are tracked, before <see cref="IUnitOfWork.SaveChangesAsync"/>.
    /// </summary>
    Task AssignSequentialGlobalViberMessageIdsAsync(IReadOnlyCollection<CampaignLead> campaignLeads,
        CancellationToken cancellationToken = default);

    /// <summary>Aligns the identity sequence for <c>campaign_leads.viber_message_id</c> with <c>MAX(viber_message_id)</c> after explicit ids were inserted.</summary>
    Task SyncCampaignLeadViberMessageIdSequenceAsync(CancellationToken cancellationToken = default);
}