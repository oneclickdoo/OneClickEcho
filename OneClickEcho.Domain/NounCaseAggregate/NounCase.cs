using OneClickEcho.Domain.Common.Primitives;
using OneClickEcho.Domain.NounCaseAggregate.ValueObjects;

namespace OneClickEcho.Domain.NounCaseAggregate;

public sealed class NounCase : AggregateRoot<NounCaseId>
{
    public NounCase(NounCaseId id, string nominative, string vocative) : base(id)
    {
        Nominative = nominative;
        Vocative = vocative;
    }

    public NounCase(string nominative) : base(NounCaseId.CreateUnique())
    {
        Nominative = nominative;
    }

    public string Nominative { get; set; } = string.Empty;

    public string? Vocative { get; set; }

    // Used for EFCore
    public NounCase() : base(NounCaseId.CreateUnique()) { }
}