namespace OneClickEcho.Application.Common.Viber;

public static class ViberImageFields
{
    public const int MaxGifBytes = 24 * 1024 * 1024;

    public static bool IsGif(string? media) =>
        string.Equals(Extension(media), ".gif", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// <c>gif</c> when the file is a GIF or the caller asked for gif.
    /// <c>img</c> only when the caller explicitly asks. Otherwise null so Comtrade treats the image as static.
    /// </summary>
    public static string? ResolveFormat(string? media, string? explicitFormat)
    {
        if (string.Equals(explicitFormat, "gif", StringComparison.OrdinalIgnoreCase) || IsGif(media))
        {
            return "gif";
        }

        if (string.Equals(explicitFormat, "img", StringComparison.OrdinalIgnoreCase))
        {
            return "img";
        }

        return null;
    }

    public static string NormalizeRatio(string? ratio) =>
        ratio?.Trim().ToLowerInvariant() switch
        {
            "portrait" => "portrait",
            "landscape" => "landscape",
            _ => "square"
        };

    public static bool IsAllowedRatio(string? ratio)
    {
        if (string.IsNullOrWhiteSpace(ratio))
        {
            return true;
        }

        string n = ratio.Trim().ToLowerInvariant();
        return n is "square" or "portrait" or "landscape";
    }

    public static bool IsAllowedFormat(string? format)
    {
        if (string.IsNullOrWhiteSpace(format))
        {
            return true;
        }

        string n = format.Trim().ToLowerInvariant();
        return n is "gif" or "img";
    }

    private static string Extension(string? media)
    {
        if (string.IsNullOrWhiteSpace(media))
        {
            return string.Empty;
        }

        string path = media.Trim();
        int q = path.IndexOf('?', StringComparison.Ordinal);
        if (q >= 0)
        {
            path = path[..q];
        }

        int h = path.IndexOf('#', StringComparison.Ordinal);
        if (h >= 0)
        {
            path = path[..h];
        }

        int slash = path.LastIndexOf('/');
        if (slash >= 0)
        {
            path = path[(slash + 1)..];
        }

        int dot = path.LastIndexOf('.');
        return dot < 0 ? string.Empty : path[dot..].ToLowerInvariant();
    }
}
