using OneClickEcho.Domain.Common.Primitives;
using OneClickEcho.Domain.LeadAggregate.ValueObjects;
using OneClickEcho.Domain.LeadCollectionAggregate.ValueObjects;

namespace OneClickEcho.Domain.LeadCollectionAggregate.Entities;

public sealed class LeadAssignment : Entity<LeadAssignmentId>
{
    public LeadAssignment(LeadAssignmentId leadAssignmentId, LeadId leadId) : base(leadAssignmentId)
    {
        LeadId = leadId;
    }

    public LeadAssignment(LeadId leadId) : base(LeadAssignmentId.CreateUnique())
    {
        LeadId = leadId;
    }

    public LeadId LeadId { get; set; } = default!;
    public LeadCollectionId LeadCollectionId { get; set; } = default!;

    // Used for EFCore
    public LeadAssignment() : base(LeadAssignmentId.CreateUnique()) { }
}