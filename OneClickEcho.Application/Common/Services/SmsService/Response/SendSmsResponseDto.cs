using OneClickEcho.Application.Common.Services.SmsService.Response.Enums;

namespace OneClickEcho.Application.Common.Services.SmsService.Response
{
    public class SendSmsResponseDto
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public string Reference { get; set; } = string.Empty;

        public SmsStatus Status { get; set; }
    }
}
