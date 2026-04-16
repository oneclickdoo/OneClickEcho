using System.Text.RegularExpressions;

namespace OneClickEcho.Infrastructure.Services.MessageHandling.Viber;

/// <summary>
/// Viber Business API expects markdown (*bold*, _italic_, ~strike~) in message text, not HTML.
/// </summary>
public static class ViberMessageFormatting
{
    private const int MaxHtmlNestPasses = 8;

    private const int MaxComtradeSurveyIntroChars = 1000;

    private const int MaxMarkdownStripPasses = 8;

    private static readonly Regex LegacyHtmlTagProbe = new(@"<\s*/?\s*[biu]\s*>", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex RxB = new(@"<b>([\s\S]*?)</b>", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex RxI = new(@"<i>([\s\S]*?)</i>", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex RxU = new(@"<u>([\s\S]*?)</u>", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex RxMdCodeFence = new(@"```([\s\S]*?)```", RegexOptions.Compiled);

    private static readonly Regex RxMdBold = new(@"\*([^*\n]+)\*", RegexOptions.Compiled);

    private static readonly Regex RxMdItalic = new(@"_([^_\n]+)_", RegexOptions.Compiled);

    private static readonly Regex RxMdStrike = new(@"~([^~\n]+)~", RegexOptions.Compiled);

    private static readonly Regex RxWhitespaceRun = new(@"\s+", RegexOptions.Compiled);

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

    /// <summary>
    /// Comtrade message type 801 (survey/list) is often rejected at delivery (<c>SRVC_SURVEY_VALIDATION_ERROR</c>)
    /// when <c>MessageText</c> contains Viber markdown or line breaks. This produces a single-line plain intro
    /// matching typical provider samples while keeping the same HTML → markdown migration as other Viber types.
    /// </summary>
    public static string ToComtradeSurveyIntroText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        string s = MigrateLegacyHtmlToMarkdown(text);
        for (int pass = 0; pass < MaxMarkdownStripPasses; pass++)
        {
            string next = RxMdCodeFence.Replace(s, "$1");
            next = RxMdBold.Replace(next, "$1");
            next = RxMdItalic.Replace(next, "$1");
            next = RxMdStrike.Replace(next, "$1");
            if (string.Equals(next, s, StringComparison.Ordinal))
            {
                break;
            }

            s = next;
        }

        s = RxWhitespaceRun.Replace(s, " ").Trim();
        if (s.Length > MaxComtradeSurveyIntroChars)
        {
            s = s[..MaxComtradeSurveyIntroChars];
        }

        return s;
    }
}
