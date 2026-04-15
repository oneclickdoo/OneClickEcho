namespace OneClickEcho.Application.Common.Services.ViberService.Request.Common;

/// <summary>Comtrade message type 801 — <c>Survey.Options</c> (2–5 strings).</summary>
public sealed class ViberSurveyPayload
{
    public List<string> Options { get; set; } = [];
}
