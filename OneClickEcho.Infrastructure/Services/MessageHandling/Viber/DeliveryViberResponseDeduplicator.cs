using System.Globalization;
using System.Text.RegularExpressions;
using OneClickEcho.Application.Common.Services.ViberService.Response.Common;
using OneClickEcho.Domain.CampaignLeadAggregate.Enums;

namespace OneClickEcho.Infrastructure.Services.MessageHandling.Viber;

/// <summary>
/// Comtrade/Viber delivery API can return multiple rows for the same <see cref="DeliveryViberMessageResponse.MessageId"/>
/// (e.g. Seen and later Pending). Processing them in order would overwrite a better status with a stale one.
/// </summary>
internal static class DeliveryViberResponseDeduplicator
{
    private static readonly Regex MsJsonDate = new(@"/Date\((\d+)([+-]\d{4})?\)/", RegexOptions.Compiled);

    /// <summary>
    /// One row per <c>MessageId</c>: prefer undelivered if any; otherwise highest delivery progress, then click, then newest timestamp.
    /// </summary>
    internal static List<DeliveryViberMessageResponse> Deduplicate(IReadOnlyList<DeliveryViberMessageResponse> items)
    {
        if (items.Count <= 1)
        {
            return items.ToList();
        }

        return items
            .GroupBy(x => x.MessageId)
            .Select(PickBestForMessageId)
            .ToList();
    }

    private static DeliveryViberMessageResponse PickBestForMessageId(IGrouping<long, DeliveryViberMessageResponse> g)
    {
        List<DeliveryViberMessageResponse> list = g.ToList();
        if (list.Count == 1)
        {
            return list[0];
        }

        List<DeliveryViberMessageResponse> undelivered = list
            .Where(x => (CampaignLeadViberStatus)x.MessageStatus.Status == CampaignLeadViberStatus.Undelivered)
            .ToList();

        if (undelivered.Count > 0)
        {
            return undelivered
                .OrderByDescending(x => ParseDeliveredUtc(x.Delivered))
                .First();
        }

        // Any row with a click must win over a stale row without (same MessageId).
        DeliveryViberMessageResponse best = list
            .OrderByDescending(x => x.ClickInfo.ClickCount > 0 ? 1 : 0)
            .ThenByDescending(ProgressScore)
            .ThenByDescending(x => x.ClickInfo.ClickCount)
            .ThenByDescending(x => ParseDeliveredUtc(x.Delivered))
            .First();

        MergeClickInfoAcrossDuplicates(best, list);
        return best;
    }

    /// <summary>
    /// Comtrade may put <c>ClickCount</c> on one duplicate and a newer <c>Pending</c> row without click on another.
    /// Persist max clicks + URL onto the chosen row so downstream sets <see cref="CampaignLeadViberStatus.Clicked"/>.
    /// </summary>
    private static void MergeClickInfoAcrossDuplicates(DeliveryViberMessageResponse best, List<DeliveryViberMessageResponse> list)
    {
        int maxClicks = list.Max(x => x.ClickInfo?.ClickCount ?? 0);
        if (maxClicks <= 0)
        {
            return;
        }

        DeliveryViberMessageResponse? withClick = list
            .Where(x => x.ClickInfo != null && x.ClickInfo.ClickCount > 0)
            .OrderByDescending(x => x.ClickInfo!.ClickCount)
            .FirstOrDefault();

        best.ClickInfo ??= new ViberClickInfo();
        if (best.ClickInfo.ClickCount < maxClicks)
        {
            best.ClickInfo.ClickCount = maxClicks;
        }

        if (withClick != null && !string.IsNullOrEmpty(withClick.ClickInfo.Url))
        {
            best.ClickInfo.Url = withClick.ClickInfo.Url;
        }
    }

    /// <summary>Higher = further in successful delivery funnel.</summary>
    private static int ProgressScore(DeliveryViberMessageResponse item)
    {
        if (item.ClickInfo.ClickCount > 0)
        {
            return 100;
        }

        return (CampaignLeadViberStatus)item.MessageStatus.Status switch
        {
            CampaignLeadViberStatus.Clicked => 95,
            CampaignLeadViberStatus.Seen => 80,
            CampaignLeadViberStatus.Delivered => 65,
            CampaignLeadViberStatus.Pending => 40,
            CampaignLeadViberStatus.Received => 25,
            CampaignLeadViberStatus.Expired => 30,
            CampaignLeadViberStatus.Undelivered => 10,
            CampaignLeadViberStatus.None => 0,
            _ => 0
        };
    }

    internal static DateTime ParseDeliveredUtc(string delivered)
    {
        if (string.IsNullOrWhiteSpace(delivered))
        {
            return DateTime.MinValue;
        }

        Match m = MsJsonDate.Match(delivered);
        if (m.Success && long.TryParse(m.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out long ms))
        {
            try
            {
                return DateTimeOffset.FromUnixTimeMilliseconds(ms).UtcDateTime;
            }
            catch (ArgumentOutOfRangeException)
            {
                return DateTime.MinValue;
            }
        }

        if (DateTime.TryParse(delivered, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime dt))
        {
            return dt.Kind == DateTimeKind.Utc ? dt : dt.ToUniversalTime();
        }

        return DateTime.MinValue;
    }
}
