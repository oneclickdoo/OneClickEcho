namespace OneClickEcho.Domain.CampaignAggregate.Enums;

/// <summary>Viber payload shape (Comtrade one-way). Legacy rows use <see cref="Text"/> with <c>viber_media</c> set — infer kind when sending.</summary>
public enum CampaignViberContentKind : short
{
    Text = 0,
    Image = 1,
    Video = 2,
    File = 3,
    Survey = 4
}
