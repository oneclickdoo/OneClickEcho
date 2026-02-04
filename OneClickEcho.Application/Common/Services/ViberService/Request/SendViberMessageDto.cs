using OneClickEcho.Application.Common.Services.ViberService.Request.Common;

namespace OneClickEcho.Application.Common.Services.ViberService.Request
{
    public class SendViberMessageDto
    {
        public required ViberUserCredentials UserCredentials { get; set; }

        // max 500 records
        public required List<ViberMessage> ViberMessages { get; set; }
    }
}
