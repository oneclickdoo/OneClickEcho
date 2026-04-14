namespace OneClickEcho.Persistence.Options;

/// <summary>
/// When running a new deployment in parallel with a legacy system, set <see cref="Floor"/> above the legacy
/// <c>MAX(viber_message_id)</c> (plus headroom) so outbound Comtrade/Viber message ids stay in a non-overlapping range.
/// </summary>
public sealed class CampaignLeadViberMessageIdOptions
{
    public const string SectionName = "Messaging:CampaignLeadViberMessageId";

    /// <summary>When greater than zero, the next assigned id is at least <c>Floor + 1</c> even if local MAX is lower.</summary>
    public long Floor { get; set; }
}
