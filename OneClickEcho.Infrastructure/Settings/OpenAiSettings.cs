namespace OneClickEcho.Infrastructure.Settings;

public class OpenAiSettings
{
    /// <summary>
    /// Base URL for OpenAI-compatible chat API (trailing slash optional). Empty = https://api.openai.com/
    /// Use when traffic goes through an org gateway (e.g. Blackbird) instead of public OpenAI.
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    public string ApiKey { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;
}