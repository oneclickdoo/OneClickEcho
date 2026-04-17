using OneClickEcho.Application.Common.Services.SmsService.Request;
using OneClickEcho.Application.Common.Services.SmsService.Response;

namespace OneClickEcho.Application.Common.Services.SmsService
{
    public interface ISmsService
    {
        public Task<SendSmsResponseDto?> Send(SendSmsRequestDto request, string smsUsername, string smsPassword);
    }
}
