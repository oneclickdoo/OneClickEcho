using OneClickEcho.Application.Common.Services.ViberService.Response.Enum;

namespace OneClickEcho.Application.Common.Services.ViberService.Response.Common
{
    public class DeliveryViberMessageStatus
    {
        public DeliveryViberStatus Status { get; set; }

        public DeliveryViberSubstatus SubStatus { get; set; }
    }
}
