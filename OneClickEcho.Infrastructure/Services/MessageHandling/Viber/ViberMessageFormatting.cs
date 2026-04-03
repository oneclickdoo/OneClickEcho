using System.Text.RegularExpressions;

namespace OneClickEcho.Infrastructure.Services.MessageHandling.Viber;

/// <summary>
/// Viber Business API expects markdown (*bold*, _italic_, ~strike~) in message text, not HTML.
/// </summary>
public static class ViberMessageFormatting
{
    private const int MaxHtmlNestPasses = 8;

    private static readonly Regex LegacyHtmlTagProbe = new(@"<\s*/?\s*[biu]\s*>", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex RxB = new(@"<b>([\s\S]*?)</b>", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex RxI = new(@"<i>([\s\S]*?)</i>", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex RxU = new(@"<u>([\s\S]*?)</u>", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static string MigrateLegacyHtmlToMarkdown(string text)
    {
        if (string.IsNullOrEmpty(text) || !LegacyHtmlTagProbe.IsMatch(text))
        {
            return text;
        }

        string output = text;
        for (int pass = 0; pass < MaxHtmlNestPasses; pass++)
        {
            string next = RxB.Replace(output, "*$1*");
            next = RxI.Replace(next, "_$1_");
            next = RxU.Replace(next, "$1");
            if (string.Equals(next, output, StringComparison.Ordinal))
            {
                break;
            }

            output = next;
        }

        return output;
    }
}
