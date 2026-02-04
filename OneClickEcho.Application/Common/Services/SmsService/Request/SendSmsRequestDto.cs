namespace OneClickEcho.Application.Common.Services.SmsService.Request
{
    public class SendSmsRequestDto
    {
        public string Sender { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;
    }
}
