using OneClickEcho.Application.Common.Services.ViberService.Response.Common;

namespace OneClickEcho.Application.Common.Services.ViberService.Response
{
    public class DeliveryViberMessageResponseDto
    {
        public required List<DeliveryViberMessageResponse> ViberMessageResponses { get; set; }
    }
}
