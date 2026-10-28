namespace OneClickEcho.Application.Common.Viber;

public static class ViberPhone
{
    public static string Normalize(string phone)
    {
        string p = phone.Trim().Replace(" ", "", StringComparison.Ordinal);
        if (p.StartsWith("00", StringComparison.Ordinal))
        {
            p = "+" + p[2..];
        }

        if (!p.StartsWith('+'))
        {
            p = "+" + p;
        }

        return p;
    }
}
