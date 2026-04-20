using OneClickEcho.Application.Common.Services.SmsService;
using OneClickEcho.Application.Common.Services.SmsService.Request;
using OneClickEcho.Application.Common.Services.SmsService.Response;
using System.Text;
using System.Text.Json;

namespace OneClickEcho.Infrastructure.Services.MessageHandling.Sms
{
    public class SmsService(HttpClient httpClient) : ISmsService
    {
        private readonly HttpClient _httpClient = httpClient;

        private readonly JsonSerializerOptions jsonSerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        /// <summary>SMS gateway credentials: <c>username</c> and <c>pwd</c> on each request.</summary>
        private static void AddSmsAuthHeaders(HttpRequestMessage message, string smsUsername, string smsPassword)
        {
            message.Headers.TryAddWithoutValidation("username", smsUsername);
            message.Headers.TryAddWithoutValidation("pwd", smsPassword);
        }

        public async Task<SendSmsResponseDto?> Send(SendSmsRequestDto request, string smsUsername, string smsPassword)
        {
            StringContent content = new(JsonSerializer.Serialize(request, jsonSerializerOptions), Encoding.UTF8, "application/json");

            HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, "/SmsWebBulkApi/SendMsg.aspx")
            {
                Content = content
            };

            AddSmsAuthHeaders(httpRequestMessage, smsUsername, smsPassword);

            try
            {
                HttpResponseMessage response = await _httpClient.SendAsync(httpRequestMessage);

                string responseStream = await response.Content.ReadAsStringAsync();

                SendSmsResponseDto? responseDto = JsonSerializer
                    .Deserialize<SendSmsResponseDto>(responseStream, jsonSerializerOptions);

                return responseDto;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        public async Task<SendSmsDeliveryResponseDto?> GetDelivery(
            SendSmsDeliveryRequestDto request,
            string smsUsername,
            string smsPassword)
        {
            StringContent content = new(JsonSerializer.Serialize(request, jsonSerializerOptions), Encoding.UTF8, "application/json");

            HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, "/SmsWebBulkApi/SentMsgDR.aspx")
            {
                Content = content
            };

            AddSmsAuthHeaders(httpRequestMessage, smsUsername, smsPassword);

            try
            {
                HttpResponseMessage response = await _httpClient.SendAsync(httpRequestMessage);

                string responseStream = await response.Content.ReadAsStringAsync();

                SendSmsDeliveryResponseDto? responseDto = JsonSerializer
                    .Deserialize<SendSmsDeliveryResponseDto>(responseStream, jsonSerializerOptions);

                return responseDto;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }
    }
}
