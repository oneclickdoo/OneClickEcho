using OneClickEcho.Application.Common.Helpers;
using OneClickEcho.Application.Common.Services.ViberService;
using OneClickEcho.Application.Common.Services.ViberService.Request;
using OneClickEcho.Application.Common.Services.ViberService.Request.Enum;
using OneClickEcho.Application.Common.Services.ViberService.Response;
using OneClickEcho.Domain.CampaignAggregate;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using OneClickEcho.Domain.ApiMessageAggregate;
using Polly;

namespace OneClickEcho.Infrastructure.Services.MessageHandling.Viber
{
    public class ViberService(HttpClient httpClient) : IViberService
    {
        private readonly HttpClient _httpClient = httpClient;

        public async Task<SendViberMessageResponseDto?> Send(SendViberMessageDto request, int retryCountParam = 6)
        {
            var retryPolicy = Policy
                .Handle<Exception>()
                .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .WaitAndRetryAsync(
                    retryCount: retryCountParam,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (outcome, timespan, retryCount, context) =>
                    {
                        // Console.WriteLine($"Retry {retryCount} of Viber Send after {timespan.TotalSeconds} seconds.");
                    });
            
            int outboundCount = request.ViberMessages?.Count ?? 0;
            string baseUrl = _httpClient.BaseAddress?.ToString().TrimEnd('/') ?? "(no BaseAddress)";
            // Console.WriteLine(
            //     $"[{DateTime.UtcNow:O}] [ComTrade] Viber bulk Send starting: {outboundCount} message(s) -> {baseUrl}/ViberCTOC/ReceivingService.svc/json/Send");

            try
            {
                // Build a fresh request per attempt — StringContent/HttpRequestMessage must not be reused across Polly retries.
                HttpResponseMessage response = await retryPolicy.ExecuteAsync(async () =>
                {
                    string json = JsonSerializer.Serialize(request, new JsonSerializerOptions()
                    {
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                    });
                    using StringContent content = new(json, Encoding.UTF8, "application/json");
                    using HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, "/ViberCTOC/ReceivingService.svc/json/Send")
                    {
                        Content = content
                    };
                    return await _httpClient.SendAsync(httpRequestMessage);
                });

                response.EnsureSuccessStatusCode();

                // get response
                string responseStream = await response.Content.ReadAsStringAsync();

                // serialize response
                SendViberMessageResponseDto? responseDto = JsonSerializer
                    .Deserialize<SendViberMessageResponseDto>(responseStream, new JsonSerializerOptions()
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (responseDto?.ViberMessageResponses is null)
                {
                    throw new InvalidOperationException(
                        $"Viber Send returned an empty or invalid JSON body. Raw length: {responseStream?.Length ?? 0}.");
                }

                // Console.WriteLine(
                //     $"[{DateTime.UtcNow:O}] [ComTrade] Viber Send HTTP OK: {(int)response.StatusCode} {response.StatusCode}, {responseDto.ViberMessageResponses.Count} status row(s) in JSON body.");

                return responseDto;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public async Task<DeliveryViberMessageResponseDto?> DeliveryById(DeliveryViberMessageDto request)
        {
            var retryPolicy = Policy
                .Handle<Exception>()
                .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .WaitAndRetryAsync(
                    retryCount: 6,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (outcome, timespan, retryCount, context) =>
                    {
                        // Console.WriteLine($"Retry {retryCount} of Viber Send after {timespan.TotalSeconds} seconds.");
                    });
            
            try
            {
                HttpResponseMessage response = await retryPolicy.ExecuteAsync(async () =>
                {
                    using StringContent content = new(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
                    using HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, "/ViberCTOC/ReceivingService.svc/json/DeliveryById")
                    {
                        Content = content
                    };
                    return await _httpClient.SendAsync(httpRequestMessage);
                });

                response.EnsureSuccessStatusCode();

                // get response
                string responseStream = await response.Content.ReadAsStringAsync();

                // serialize response
                DeliveryViberMessageResponseDto? responseDto = JsonSerializer
                    .Deserialize<DeliveryViberMessageResponseDto>(responseStream, new JsonSerializerOptions()
                    {
                        PropertyNameCaseInsensitive = true
                    });

                return responseDto;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        public async Task<AnswerViberMessageResponseDto?> AnswersById(AnswerViberMessageDto request)
        {
            var retryPolicy = Policy
                .Handle<Exception>()
                .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .WaitAndRetryAsync(
                    retryCount: 6,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (outcome, timespan, retryCount, context) =>
                    {
                        // Console.WriteLine($"Retry {retryCount} of Viber Send after {timespan.TotalSeconds} seconds.");
                    });
            
            try
            {
                HttpResponseMessage response = await retryPolicy.ExecuteAsync(async () =>
                {
                    using StringContent content = new(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
                    using HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, "/ViberCTOC/ReceivingService.svc/json/AnswersById")
                    {
                        Content = content
                    };
                    return await _httpClient.SendAsync(httpRequestMessage);
                });

                response.EnsureSuccessStatusCode();

                // get response
                string responseStream = await response.Content.ReadAsStringAsync();

                // serialize response
                AnswerViberMessageResponseDto? responseDto = JsonSerializer
                    .Deserialize<AnswerViberMessageResponseDto>(responseStream, new JsonSerializerOptions()
                    {
                        PropertyNameCaseInsensitive = true
                    });

                return responseDto;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        public static ViberSendMessageType DetermineMessageType(Campaign campaign)
        {
            // TODO: Ukoliko se ne koristi two way, obrisati ceo ovaj komentar
            // // two-way
            // if (campaign.IsViberReceivable)
            // {
            //     if (string.IsNullOrEmpty(campaign.ViberMedia) && string.IsNullOrEmpty(campaign.ViberButtonUrl))
            //     {
            //         return ViberSendMessageType.TwoWayTextOnly;
            //     }
            //
            //     if (string.IsNullOrEmpty(campaign.ViberMedia) && !string.IsNullOrEmpty(campaign.ViberButtonUrl))
            //     {
            //         return ViberSendMessageType.TwoWayTextButton;
            //     }
            //
            //     if (!string.IsNullOrEmpty(campaign.ViberMedia) && !string.IsNullOrEmpty(campaign.ViberButtonUrl))
            //     {
            //         return MediaHelper.GetMediaType(campaign.ViberMedia) == CampaignMediaType.Image
            //             ? ViberSendMessageType.TwoWayTextImageButton
            //             : ViberSendMessageType.TwoWayTextVideoButton;
            //     }
            //
            //     // default
            //     return ViberSendMessageType.TwoWayTextOnly;
            // }
            // // one-way
            // else
            // {
            //     if (string.IsNullOrEmpty(campaign.ViberMedia) && string.IsNullOrEmpty(campaign.ViberButtonUrl))
            //     {
            //         return ViberSendMessageType.OneWayTextOnly;
            //     }
            //
            //     if (string.IsNullOrEmpty(campaign.ViberMedia) && !string.IsNullOrEmpty(campaign.ViberButtonUrl))
            //     {
            //         return ViberSendMessageType.OneWayTextButton;
            //     }
            //
            //     if (!string.IsNullOrEmpty(campaign.ViberMedia) && MediaHelper.GetMediaType(campaign.ViberMedia)
            //         == CampaignMediaType.Video && string.IsNullOrEmpty(campaign.ViberButtonUrl))
            //     {
            //         return ViberSendMessageType.OneWayVideoText;
            //     }
            //
            //     if (!string.IsNullOrEmpty(campaign.ViberMedia) && !string.IsNullOrEmpty(campaign.ViberButtonUrl))
            //     {
            //         return MediaHelper.GetMediaType(campaign.ViberMedia) == CampaignMediaType.Image
            //             ? ViberSendMessageType.OneWayTextImageButton
            //             : ViberSendMessageType.OneWayVideoTextButton;
            //     }
            //
            //     // default
            //     return ViberSendMessageType.OneWayTextOnly;
            // }

            // Text only
            if (string.IsNullOrEmpty(campaign.ViberMedia) && string.IsNullOrEmpty(campaign.ViberButtonUrl))
            {
                return ViberSendMessageType.OneWayTextOnly;
            }

            // Text with button
            if (string.IsNullOrEmpty(campaign.ViberMedia) && !string.IsNullOrEmpty(campaign.ViberButtonUrl))
            {
                return ViberSendMessageType.OneWayTextButton;
            }

            // Video with text
            if (!string.IsNullOrEmpty(campaign.ViberMedia) && string.IsNullOrEmpty(campaign.ViberButtonUrl))
            {
                if (MediaHelper.GetMediaType(campaign.ViberMedia) == CampaignMediaType.Video)
                {
                    return ViberSendMessageType.OneWayVideoText;
                }
            }

            // Video or Image with text and button
            if (!string.IsNullOrEmpty(campaign.ViberMedia) && !string.IsNullOrEmpty(campaign.ViberButtonUrl))
            {
                if (MediaHelper.GetMediaType(campaign.ViberMedia) == CampaignMediaType.Video)
                {
                    return ViberSendMessageType.OneWayVideoTextButton;
                }

                if (MediaHelper.GetMediaType(campaign.ViberMedia) == CampaignMediaType.Image)
                {
                    return ViberSendMessageType.OneWayTextImageButton;
                }
            }

            // Text only
            return ViberSendMessageType.OneWayTextOnly;
        }
        
        public static ViberSendMessageType DetermineApiMessageType(ApiMessage apiMessage)
        {
            // Text only
            if (string.IsNullOrEmpty(apiMessage.ViberMedia) && string.IsNullOrEmpty(apiMessage.ViberButtonUrl))
            {
                return ViberSendMessageType.OneWayTextOnly;
            }

            // Text with button
            if (string.IsNullOrEmpty(apiMessage.ViberMedia) && !string.IsNullOrEmpty(apiMessage.ViberButtonUrl))
            {
                return ViberSendMessageType.OneWayTextButton;
            }

            // Video with text
            if (!string.IsNullOrEmpty(apiMessage.ViberMedia) && string.IsNullOrEmpty(apiMessage.ViberButtonUrl))
            {
                if (MediaHelper.GetMediaType(apiMessage.ViberMedia) == CampaignMediaType.Video)
                {
                    return ViberSendMessageType.OneWayVideoText;
                }
            }

            // Video or Image with text and button
            if (!string.IsNullOrEmpty(apiMessage.ViberMedia) && !string.IsNullOrEmpty(apiMessage.ViberButtonUrl))
            {
                if (MediaHelper.GetMediaType(apiMessage.ViberMedia) == CampaignMediaType.Video)
                {
                    return ViberSendMessageType.OneWayVideoTextButton;
                }

                if (MediaHelper.GetMediaType(apiMessage.ViberMedia) == CampaignMediaType.Image)
                {
                    return ViberSendMessageType.OneWayTextImageButton;
                }
            }

            // Text only
            return ViberSendMessageType.OneWayTextOnly;
        }
    }
}