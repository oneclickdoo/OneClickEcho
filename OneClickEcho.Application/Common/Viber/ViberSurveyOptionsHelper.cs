using System.Text;
using System.Text.Json;

namespace OneClickEcho.Application.Common.Viber;

public static class ViberSurveyOptionsHelper
{
    public const int MinOptions = 2;

    public const int MaxOptions = 5;

    public const int MaxOptionLength = 50;

    /// <summary>Characters Comtrade/Viber survey options typically reject (line breaks, zero-width).</summary>
    public static string SanitizeOptionLine(string raw)
    {
        if (string.IsNullOrEmpty(raw))
        {
            return string.Empty;
        }

        StringBuilder sb = new(raw.Length);
        foreach (char ch in raw)
        {
            switch (ch)
            {
                case '\u200B':
                case '\u200C':
                case '\u200D':
                case '\uFEFF':
                    continue;
                case '\r':
                case '\n':
                case '\t':
                case '\v':
                case '\f':
                    sb.Append(' ');
                    break;
                default:
                    if (char.GetUnicodeCategory(ch) == System.Globalization.UnicodeCategory.Control)
                    {
                        sb.Append(' ');
                    }
                    else
                    {
                        sb.Append(ch);
                    }

                    break;
            }
        }

        string s = sb.ToString();
        while (s.Contains("  ", StringComparison.Ordinal))
        {
            s = s.Replace("  ", " ", StringComparison.Ordinal);
        }

        return s.Trim();
    }

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
            string t = SanitizeOptionLine((o ?? string.Empty).Trim());
            if (t.Length == 0 || t.Length > MaxOptionLength)
            {
                throw new InvalidOperationException(
                    $"Each survey option must be 1–{MaxOptionLength} characters after trim (single line, no control characters).");
            }

            trimmed.Add(t);
        }

        HashSet<string> seen = new(StringComparer.Ordinal);
        foreach (string o in trimmed)
        {
            if (!seen.Add(o))
            {
                throw new InvalidOperationException("Survey options must be unique (no duplicate answers).");
            }
        }

        return trimmed;
    }
}
