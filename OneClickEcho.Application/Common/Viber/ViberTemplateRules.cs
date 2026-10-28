using System.Text.Json;
using System.Text.RegularExpressions;

namespace OneClickEcho.Application.Common.Viber;

public static partial class ViberTemplateRules
{
    public const int MaxBodyLength = 1000;
    public const int MaxVariables = 8;
    public const int MaxVariableNameLength = 50;
    public const int MaxVariableValueLength = 125;

    public static readonly HashSet<string> Languages = new(StringComparer.Ordinal)
    {
        "ar", "hy", "be", "bs", "bg", "my", "zh-Hans", "zh-Hant", "hr", "cs", "da", "nl", "en", "fi",
        "fr_FR", "ka", "de", "el", "he", "hu", "id", "it", "ne", "no", "fa", "pl", "pt_BR", "pt_PT",
        "ro", "ru", "sr", "sk", "sl", "es_ES", "sv", "th", "tr", "uk", "vi"
    };

    public static void ValidateBody(string? body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            throw new InvalidOperationException("Template body is required.");
        }

        string text = body.Trim();
        if (text.Length > MaxBodyLength)
        {
            throw new InvalidOperationException($"Template body must be at most {MaxBodyLength} characters.");
        }

        if (text.StartsWith("{{", StringComparison.Ordinal) || text.EndsWith("}}", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Template body must start and end with static text, not a variable.");
        }

        MatchCollection matches = VariableRegex().Matches(text);
        if (matches.Count > MaxVariables)
        {
            throw new InvalidOperationException($"A template can contain at most {MaxVariables} variables.");
        }

        foreach (Match match in matches)
        {
            string name = match.Groups[1].Value;
            if (name.Length > MaxVariableNameLength)
            {
                throw new InvalidOperationException($"Variable names must be at most {MaxVariableNameLength} English letters.");
            }
        }
    }

    public static List<string> VariableNames(string body) =>
        VariableRegex().Matches(body).Select(m => m.Groups[1].Value).Distinct(StringComparer.Ordinal).ToList();

    public static Dictionary<string, string> ParseParams(string? json, string body)
    {
        Dictionary<string, string> values = [];
        if (!string.IsNullOrWhiteSpace(json))
        {
            try
            {
                values = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? [];
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("Template parameters must be a JSON object of strings.", ex);
            }
        }

        List<string> names = VariableNames(body);
        foreach (string name in names)
        {
            if (!values.TryGetValue(name, out string? value) || string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException($"Missing value for template variable {name}.");
            }

            if (value.Length > MaxVariableValueLength)
            {
                throw new InvalidOperationException($"Value for {name} must be at most {MaxVariableValueLength} characters.");
            }
        }

        return names.ToDictionary(n => n, n => values[n].Trim(), StringComparer.Ordinal);
    }

    public static void EnsureLanguage(string? language)
    {
        if (string.IsNullOrWhiteSpace(language) || !Languages.Contains(language.Trim()))
        {
            throw new InvalidOperationException("Template language is not a supported Comtrade code.");
        }
    }

    [GeneratedRegex(@"\{\{([A-Za-z]+)\}\}")]
    private static partial Regex VariableRegex();
}
