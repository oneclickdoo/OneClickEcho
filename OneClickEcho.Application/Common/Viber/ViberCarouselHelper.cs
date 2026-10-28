using System.Text.Json;
using OneClickEcho.Application.Common.Services.ViberService.Request.Common;

namespace OneClickEcho.Application.Common.Viber;

public static class ViberCarouselHelper
{
    public const int MinItems = 2;
    public const int MaxItems = 5;
    public const int MinTitle = 2;
    public const int MaxTitle = 38;
    public const int MaxPrimaryLabel = 10;
    public const int MaxSecondaryLabel = 12;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static ViberCarouselPayload ParseRequired(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new InvalidOperationException("Carousel JSON is missing.");
        }

        ViberCarouselPayload? payload;
        try
        {
            payload = JsonSerializer.Deserialize<ViberCarouselPayload>(json, JsonOptions);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Invalid carousel JSON.", ex);
        }

        if (payload is null || payload.Items.Count < MinItems || payload.Items.Count > MaxItems)
        {
            throw new InvalidOperationException($"Carousel must have {MinItems}–{MaxItems} cards.");
        }

        foreach (ViberCarouselItem item in payload.Items)
        {
            item.Title = (item.Title ?? string.Empty).Trim();
            item.ImageUrl = (item.ImageUrl ?? string.Empty).Trim();
            item.PrimaryButton ??= new ViberCarouselButton();
            item.PrimaryButton.Label = (item.PrimaryButton.Label ?? string.Empty).Trim();
            item.PrimaryButton.ActionUrl = (item.PrimaryButton.ActionUrl ?? string.Empty).Trim();

            if (item.Title.Length < MinTitle || item.Title.Length > MaxTitle)
            {
                throw new InvalidOperationException($"Each carousel title must be {MinTitle}–{MaxTitle} characters.");
            }

            if (!IsImagePath(item.ImageUrl))
            {
                throw new InvalidOperationException("Each carousel card needs a PNG or JPEG image URL.");
            }

            if (item.PrimaryButton.Label.Length is < 1 or > MaxPrimaryLabel)
            {
                throw new InvalidOperationException($"Primary button label must be 1–{MaxPrimaryLabel} characters.");
            }

            if (!IsAbsoluteUrl(item.PrimaryButton.ActionUrl))
            {
                throw new InvalidOperationException("Primary button action URL must be an absolute http(s) URL.");
            }

            if (item.SecondaryButton is not null)
            {
                item.SecondaryButton.Label = (item.SecondaryButton.Label ?? string.Empty).Trim();
                item.SecondaryButton.ActionUrl = (item.SecondaryButton.ActionUrl ?? string.Empty).Trim();
                bool empty = item.SecondaryButton.Label.Length == 0 && item.SecondaryButton.ActionUrl.Length == 0;
                if (empty)
                {
                    item.SecondaryButton = null;
                    continue;
                }

                if (item.SecondaryButton.Label.Length is < 1 or > MaxSecondaryLabel)
                {
                    throw new InvalidOperationException($"Secondary button label must be 1–{MaxSecondaryLabel} characters.");
                }

                if (!IsAbsoluteUrl(item.SecondaryButton.ActionUrl))
                {
                    throw new InvalidOperationException("Secondary button action URL must be an absolute http(s) URL.");
                }
            }
        }

        return payload;
    }

    private static bool IsAbsoluteUrl(string url) =>
        Uri.TryCreate(url, UriKind.Absolute, out Uri? uri) &&
        (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

    private static bool IsImagePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        string p = path.Trim();
        int q = p.IndexOf('?', StringComparison.Ordinal);
        if (q >= 0)
        {
            p = p[..q];
        }

        return p.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
            || p.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
            || p.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase);
    }
}
