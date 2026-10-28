using OneClickEcho.Domain.Common.Primitives;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;
using OneClickEcho.Domain.ViberTemplateAggregate.ValueObjects;

namespace OneClickEcho.Domain.ViberTemplateAggregate;

public sealed class ViberTemplate : AggregateRoot<ViberTemplateId>
{
    public ViberTemplate(
        CompanyId companyId,
        string display,
        string name,
        string language,
        string body,
        string? tag,
        int comtradeId,
        string? viberTemplateId,
        string status) : base(ViberTemplateId.CreateUnique())
    {
        CompanyId = companyId;
        Display = display;
        Name = name;
        Language = language;
        Body = body;
        Tag = tag;
        ComtradeId = comtradeId;
        ProviderTemplateId = viberTemplateId;
        Status = status;
        UpdatedAt = DateTime.UtcNow;
    }

    public CompanyId CompanyId { get; set; } = default!;

    public string Display { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Language { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public string? Tag { get; set; }

    public string Category { get; set; } = "Transactional";

    public int ComtradeId { get; set; }

    /// <summary>Viber-side id used as TemplateId on message type 1702. Null until approved.</summary>
    public string? ProviderTemplateId { get; set; }

    public string Status { get; set; } = "CREATED";

    public DateTime UpdatedAt { get; set; }

    public bool CanSend =>
        string.Equals(Status, "APPROVED", StringComparison.OrdinalIgnoreCase) &&
        !string.IsNullOrWhiteSpace(ProviderTemplateId);

    public ViberTemplate() : base(ViberTemplateId.CreateUnique())
    {
    }
}
