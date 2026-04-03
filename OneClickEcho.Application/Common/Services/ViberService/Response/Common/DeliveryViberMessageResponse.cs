namespace OneClickEcho.Application.Common.Services.ViberService.Response.Common
{
    public class DeliveryViberMessageResponse
    {
        public required long MessageId { get; set; }

        public required string Delivered { get; set; } = string.Empty;

        public required string MSISDN { get; set; } = string.Empty;

        public required DeliveryViberMessageStatus MessageStatus { get; set; }

        public required ViberClickInfo ClickInfo { get; set; }
    }
}
