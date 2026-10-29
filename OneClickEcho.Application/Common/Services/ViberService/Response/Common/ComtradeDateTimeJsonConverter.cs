using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace OneClickEcho.Application.Common.Services.ViberService.Response.Common;

/// <summary>
/// Comtrade WCF JSON dates look like <c>/Date(1520602889943+0100)/</c>.
/// The number is Unix epoch milliseconds. System.Text.Json does not read that into <see cref="DateTime"/>.
/// </summary>
public sealed partial class ComtradeDateTimeJsonConverter : JsonConverter<DateTime>
{
    [GeneratedRegex(@"^/Date\((-?\d+)(?:[+-]\d{4})?\)/$", RegexOptions.CultureInvariant)]
    private static partial Regex MicrosoftDate();

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt64(out long unixMs))
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(unixMs).UtcDateTime;
        }

        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException("Comtrade date must be a string or unix milliseconds.");
        }

        string? raw = reader.GetString();
        if (string.IsNullOrWhiteSpace(raw))
        {
            throw new JsonException("Comtrade date is empty.");
        }

        Match match = MicrosoftDate().Match(raw);
        if (match.Success && long.TryParse(match.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out long ms))
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(ms).UtcDateTime;
        }

        if (DateTimeOffset.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTimeOffset parsed))
        {
            return parsed.UtcDateTime;
        }

        throw new JsonException($"Unrecognized Comtrade date: {raw}");
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture));
    }
}
