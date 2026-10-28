using OneClickEcho.Application.Common.Services.ViberService.Request.Common;
using OneClickEcho.Application.Common.Services.ViberService.Request.Enum;
using OneClickEcho.Application.Common.Viber;

namespace OneClickEcho.Infrastructure.Services.MessageHandling.Viber;

internal static class ViberOutboundEnricher
{
    public static void Apply(ViberMessage message, string? media, string? imageRatio, string? explicitFormat, short? surveyUiType)
    {
        if (message.MessageType is ViberSendMessageType.OneWayImageOnly or ViberSendMessageType.OneWayTextImageButton)
        {
            message.ImageFormat = ViberImageFields.ResolveFormat(media, explicitFormat);
            message.ImageRatio = ViberImageFields.NormalizeRatio(imageRatio);
        }

        if (message.Survey is not null && surveyUiType is 1 or 2)
        {
            message.Survey.ListMessageUiType = surveyUiType.Value;
        }
    }
}
