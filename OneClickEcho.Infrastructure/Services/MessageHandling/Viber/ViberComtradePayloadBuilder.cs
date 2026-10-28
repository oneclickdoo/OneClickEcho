using OneClickEcho.Application.Common.Services.ViberService.Request.Common;
using OneClickEcho.Application.Common.Services.ViberService.Request.Enum;
using OneClickEcho.Application.Common.Viber;
using OneClickEcho.Domain.ViberTemplateAggregate;

namespace OneClickEcho.Infrastructure.Services.MessageHandling.Viber;

internal static class ViberComtradePayloadBuilder
{
    public static ViberMessage? TryBuild(
        ViberSendMessageType messageType,
        string display,
        string msisdn,
        long messageId,
        int validity,
        string? messageText,
        string? carouselJson,
        string? surveyJson,
        short? surveyUiType,
        string? templateParamsJson,
        ViberTemplate? template,
        string uploadsBase)
    {
        if (messageType == ViberSendMessageType.OneWayCarousel)
        {
            ViberCarouselPayload carousel = ViberCarouselHelper.ParseRequired(carouselJson);
            foreach (ViberCarouselItem item in carousel.Items)
            {
                item.ImageUrl = Resolve(uploadsBase, item.ImageUrl);
            }

            return new ViberMessage
            {
                Carousel = carousel,
                MessageText = messageText,
                Display = display,
                Label = "promotion",
                MSISDN = msisdn,
                MessageId = messageId,
                MessageType = messageType,
                Priority = 255,
                Tag = "tag",
                Validity = validity
            };
        }

        if (messageType == ViberSendMessageType.OneWaySurveyList && !string.IsNullOrWhiteSpace(surveyJson))
        {
            List<string> options = ViberSurveyOptionsHelper.ParseRequired(surveyJson);
            ViberSurveyPayload survey = new() { Options = options };
            if (surveyUiType is 1 or 2)
            {
                survey.ListMessageUiType = surveyUiType.Value;
            }

            return new ViberMessage
            {
                MessageText = messageText,
                Survey = survey,
                Display = display,
                Label = "promotion",
                MSISDN = msisdn,
                MessageId = messageId,
                MessageType = messageType,
                Priority = 255,
                Tag = "no tag",
                Validity = validity
            };
        }

        if (messageType == ViberSendMessageType.TransactionalTemplate)
        {
            if (template is null || !template.CanSend)
            {
                throw new InvalidOperationException("Transactional template must be approved before it can be sent.");
            }

            Dictionary<string, string> parameters = ViberTemplateRules.ParseParams(templateParamsJson, template.Body);
            return new ViberMessage
            {
                Display = display,
                Label = "transaction",
                MSISDN = msisdn,
                MessageId = messageId,
                MessageType = messageType,
                Priority = 100,
                Tag = template.Tag ?? "tag",
                Validity = validity,
                TemplateId = template.ProviderTemplateId,
                TemplateLang = template.Language,
                CustomTemplateParams = parameters.Count == 0 ? null : parameters
            };
        }

        return null;
    }

    private static string Resolve(string uploadsBase, string media) =>
        media.Contains("://", StringComparison.Ordinal)
            ? media
            : $"{uploadsBase.TrimEnd('/')}/{media.TrimStart('/')}";
}
