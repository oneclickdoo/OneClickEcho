namespace OneClickEcho.Application.Common.Services.ViberService.Request.Common;

/// <summary>Comtrade message types 801 and 802 — <c>Survey.Options</c> (2–10 strings).</summary>
public sealed class ViberSurveyPayload
{
    public List<string> Options { get; set; } = [];

    /// <summary>1 radio buttons, 2 horizontal buttons. Omitted when null (Comtrade defaults to 1).</summary>
    public int? ListMessageUiType { get; set; }
}
