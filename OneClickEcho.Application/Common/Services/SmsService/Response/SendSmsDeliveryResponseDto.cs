namespace OneClickEcho.Application.Common.Services.SmsService.Response
{
    public class SendSmsDeliveryResponseDto
    {
        public int StatusVal { get; set; }

        public string ReferenceId { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }
}
