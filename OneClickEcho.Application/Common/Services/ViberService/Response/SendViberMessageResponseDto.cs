using OneClickEcho.Application.Common.Services.ViberService.Response.Common;

namespace OneClickEcho.Application.Common.Services.ViberService.Response
{
    public class SendViberMessageResponseDto
    {
        public required List<SendViberMessageResponse> ViberMessageResponses { get; set; }
    }
}
