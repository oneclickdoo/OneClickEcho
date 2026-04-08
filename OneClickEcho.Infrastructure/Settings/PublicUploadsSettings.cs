namespace OneClickEcho.Infrastructure.Settings;

/// <summary>
/// Public HTTPS base URL for files under <c>/uploads</c> (Viber fetches media from this URL).
/// Example: <c>https://api.yourdomain.com/uploads</c> — no trailing slash required.
/// </summary>
public class PublicUploadsSettings
{
    public const string SectionName = "PublicUploads";

    /// <summary>When empty, ViberSendingService falls back to the legacy production default.</summary>
    public string BaseUrl { get; set; } = "";
}
