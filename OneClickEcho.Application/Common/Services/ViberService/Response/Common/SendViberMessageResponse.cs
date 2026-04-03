using OneClickEcho.Application.Common.Services.ViberService.Response.Enum;

namespace OneClickEcho.Application.Common.Services.ViberService.Response.Common
{
    public class SendViberMessageResponse
    {
        public long MessageId { get; set; }

        public ViberMessageResponseStatus Status { get; set; }
    }
}
