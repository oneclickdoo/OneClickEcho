using System.Text.Json;
using OneClickEcho.Domain.CampaignLeadAggregate.Repositories;
using OneClickEcho.Domain.ViberSessionAggregate;
using OneClickEcho.Domain.ViberSessionAggregate.Entities;
using SessionEntity = OneClickEcho.Domain.ViberSessionAggregate.ViberSession;

namespace OneClickEcho.Application.ViberSession;

public static class ViberSessionMapper
{
    public static ViberSessionResponse Map(
        SessionEntity session,
        IReadOnlyList<ViberSessionMessage> messages,
        DateTime utcNow,
        IReadOnlyList<CampaignPhoneExchange>? campaignExchanges = null)
    {
        List<ViberSessionMessageResponse> thread = messages.Select(m => new ViberSessionMessageResponse(
            m.Id.Value,
            m.Direction.ToString(),
            m.ComtradeMessageType,
            m.Body,
            m.Media,
            m.SurveyOptionsJson,
            m.ViberMessageId,
            m.IsSent,
            m.BilledAsPromotion,
            m.SendError,
            m.CreatedAt)).ToList();

        HashSet<long> outboundIds = thread.Select(m => m.ViberMessageId).ToHashSet();
        HashSet<string> inboundTexts = thread
            .Where(m => m.Direction == "In" && !string.IsNullOrWhiteSpace(m.Body))
            .Select(m => m.Body!)
            .ToHashSet(StringComparer.Ordinal);

        foreach (CampaignPhoneExchange exchange in campaignExchanges ?? [])
        {
            if (!outboundIds.Contains(exchange.ViberMessageId))
            {
                thread.Add(new ViberSessionMessageResponse(
                    exchange.CampaignLeadId,
                    "Out",
                    string.IsNullOrWhiteSpace(exchange.SurveyOptionsJson) ? 0 : 801,
                    QuestionText(exchange.Question, exchange.SurveyOptionsJson),
                    exchange.Media,
                    exchange.SurveyOptionsJson,
                    exchange.ViberMessageId,
                    true,
                    false,
                    null,
                    exchange.SentAt));
            }

            foreach (CampaignPhoneReply reply in exchange.Replies)
            {
                if (inboundTexts.Contains(reply.Text))
                {
                    continue;
                }

                inboundTexts.Add(reply.Text);
                thread.Add(new ViberSessionMessageResponse(
                    reply.Id,
                    "In",
                    0,
                    reply.Text,
                    null,
                    null,
                    0,
                    true,
                    false,
                    null,
                    reply.At));
            }
        }

        thread.Sort((a, b) => a.CreatedAt.CompareTo(b.CreatedAt));

        return new ViberSessionResponse(
            session.Id.Value,
            session.CompanyId.Value,
            session.Sender,
            session.PhoneNumber,
            session.ComtradeSessionId,
            session.StartedAt,
            session.ExpiresAt,
            session.LastInboundAt,
            session.TotalOutbound,
            session.ConsecutiveOutbound,
            session.IsOpen(utcNow),
            thread);
    }

    private static string? QuestionText(string? question, string? surveyOptionsJson)
    {
        if (string.IsNullOrWhiteSpace(surveyOptionsJson))
        {
            return question;
        }

        try
        {
            List<string>? options = JsonSerializer.Deserialize<List<string>>(surveyOptionsJson);
            if (options is null || options.Count == 0)
            {
                return question;
            }

            string lines = string.Join("\n", options.Where(o => !string.IsNullOrWhiteSpace(o)).Select(o => "• " + o.Trim()));
            return string.IsNullOrWhiteSpace(question) ? lines : question.Trim() + "\n" + lines;
        }
        catch (JsonException)
        {
            return question;
        }
    }
}
