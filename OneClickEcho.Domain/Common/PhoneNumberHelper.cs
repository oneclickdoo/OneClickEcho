namespace OneClickEcho.Domain.Common;

/// <summary>
/// Canonical phone form for storage and deduplication (E.164-style with leading +).
/// </summary>
public static class PhoneNumberHelper
{
    public static string Standardize(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return string.Empty;
        }

        string d = new(phoneNumber.Where(char.IsDigit).ToArray());
        if (d.Length == 0)
        {
            return string.Empty;
        }

        if (d.StartsWith("381", StringComparison.Ordinal) && d.Length >= 11)
        {
            return "+" + d;
        }

        if (d.StartsWith("387", StringComparison.Ordinal) && d.Length >= 10)
        {
            return "+" + d;
        }

        if (d[0] == '0' && d.Length >= 9 && d[1] == '6')
        {
            return "+381" + d[1..];
        }

        if (d[0] == '6'
            && d.Length is >= 8 and <= 10
            && !d.StartsWith("381", StringComparison.Ordinal)
            && !d.StartsWith("387", StringComparison.Ordinal))
        {
            return "+381" + d;
        }

        return "+" + d;
    }

    public static string NormalizeKey(string phoneNumber) => Standardize(phoneNumber);

    /// <summary>
    /// International digits only, no leading + (e.g. <c>381641234567</c>). Used for SMS HTTP APIs that expect <c>381…</c> not E.164.
    /// </summary>
    public static string ForSmsGateway(string phoneNumber)
    {
        string std = Standardize(phoneNumber);
        if (string.IsNullOrEmpty(std))
        {
            return string.Empty;
        }

        return std[0] == '+' ? std[1..] : std;
    }
}
