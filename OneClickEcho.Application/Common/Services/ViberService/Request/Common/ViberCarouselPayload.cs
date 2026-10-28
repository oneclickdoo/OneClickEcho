namespace OneClickEcho.Application.Common.Services.ViberService.Request.Common;

public sealed class ViberCarouselPayload
{
    public List<ViberCarouselItem> Items { get; set; } = [];
}

public sealed class ViberCarouselItem
{
    public string ImageUrl { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public ViberCarouselButton PrimaryButton { get; set; } = new();

    public ViberCarouselButton? SecondaryButton { get; set; }
}

public sealed class ViberCarouselButton
{
    public string ActionUrl { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;
}
