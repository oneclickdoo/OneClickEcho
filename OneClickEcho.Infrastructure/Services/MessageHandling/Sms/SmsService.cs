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
        };

        public async Task<SendSmsResponseDto?> Send(SendSmsRequestDto request)
        {
            // serialize the request body to JSON
            StringContent content = new(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            // create a new HttpRequestMessage
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, "/SmsWebBulkApi/SendMsg.aspx")
            {
                Content = content
            };

            try
            {
                // send request
                HttpResponseMessage response = await _httpClient.SendAsync(httpRequestMessage);

                // get response
                string responseStream = await response.Content.ReadAsStringAsync();

                // serialize response
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

        public async Task<SendSmsDeliveryResponseDto?> GetDelivery(SendSmsDeliveryRequestDto request)
        {
            // serialize the request body to JSON
            StringContent content = new(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            // create a new HttpRequestMessage
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, "/SmsWebBulkApi/SentMsgDR.aspx")
            {
                Content = content
            };

            try
            {
                // send request
                HttpResponseMessage response = await _httpClient.SendAsync(httpRequestMessage);

                // get response
                string responseStream = await response.Content.ReadAsStringAsync();

                // serialize response
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
