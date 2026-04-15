using System.Text.Json;

namespace OneClickEcho.Application.Common.Viber;

public static class ViberSurveyOptionsHelper
{
    public const int MinOptions = 2;

    public const int MaxOptions = 5;

    public const int MaxOptionLength = 50;

    public static List<string> ParseRequired(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new InvalidOperationException("Survey options JSON is missing.");
        }

        List<string>? list;
        try
        {
            list = JsonSerializer.Deserialize<List<string>>(json);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Invalid survey options JSON.", ex);
        }

        if (list is null || list.Count < MinOptions || list.Count > MaxOptions)
        {
            throw new InvalidOperationException($"Survey must have {MinOptions}–{MaxOptions} options.");
        }

        List<string> trimmed = [];
        foreach (string o in list)
        {
            string t = (o ?? string.Empty).Trim();
            if (t.Length == 0 || t.Length > MaxOptionLength)
            {
                throw new InvalidOperationException(
                    $"Each survey option must be 1–{MaxOptionLength} characters after trim.");
            }

            trimmed.Add(t);
        }

        return trimmed;
    }
}
