using OneClickEcho.Domain.Common.Primitives;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;
using OneClickEcho.Domain.LeadAggregate.ValueObjects;
using OneClickEcho.Domain.LeadCollectionAggregate.Entities;
using OneClickEcho.Domain.LeadCollectionAggregate.ValueObjects;

namespace OneClickEcho.Domain.LeadCollectionAggregate;

public sealed class LeadCollection : AggregateRoot<LeadCollectionId>
{
    public LeadCollection(
        LeadCollectionId leadCollectionId,
        CompanyId companyId,
        string collectionName
    ) : base(leadCollectionId)
    {
        CollectionName = collectionName;
        CompanyId = companyId;
    }

    public LeadCollection(
        string collectionName,
        CompanyId companyId
    ) : base(LeadCollectionId.CreateUnique())
    {
        CollectionName = collectionName;
        CompanyId = companyId;
    }

    public string CollectionName { get; set; } = string.Empty;

    public CompanyId CompanyId { get; set; } = default!;

    public ICollection<LeadAssignment> LeadAssignments { get; set; } = [];

    public int AddLeadAssignments(IEnumerable<LeadId> leadIds)
    {
        int leadAssignmentsAdded = 0;

        foreach (LeadId leadId in leadIds)
        {
            if (LeadAssignments.Any(a => a.LeadId == leadId))
            {
                continue;
            }

            AddLeadAssignment(leadId);

            leadAssignmentsAdded++;
        }

        return leadAssignmentsAdded;
    }

    public int BulkAddLeadAssignments(IEnumerable<LeadId> leadIds)
    {
        int leadAssignmentsAdded = 0;
        
        var duplicateLeadAssignments = LeadAssignments
            .Where(x => leadIds.Contains(x.LeadId))
            .Select(x => x.LeadId)
            .ToList();
        
        var newLeadAssignments = leadIds
            .Where(x => !duplicateLeadAssignments.Contains(x))
            .ToList();

        foreach (LeadId leadId in newLeadAssignments)
        {
            AddLeadAssignment(leadId);
            leadAssignmentsAdded++;
        }
        
        return leadAssignmentsAdded;
    }

    private void AddLeadAssignment(LeadId leadId)
    {
        LeadAssignments.Add(new LeadAssignment(leadId));
    }

    // Used for EFCore
    public LeadCollection() : base(LeadCollectionId.CreateUnique()) { }
}