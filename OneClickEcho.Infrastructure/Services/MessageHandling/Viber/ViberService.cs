using OneClickEcho.Application.Common.Helpers;
using OneClickEcho.Application.Common.Services.ViberService;
using OneClickEcho.Application.Common.Services.ViberService.Request;
using OneClickEcho.Application.Common.Services.ViberService.Request.Enum;
using OneClickEcho.Application.Common.Services.ViberService.Response;
using OneClickEcho.Domain.CampaignAggregate;
using OneClickEcho.Domain.CampaignAggregate.Enums;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using OneClickEcho.Domain.ApiMessageAggregate;
using Polly;
using System.IO;

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
            CampaignViberContentKind kind = campaign.ViberContentKind;
            if (kind == CampaignViberContentKind.Text && !string.IsNullOrWhiteSpace(campaign.ViberSurveyOptionsJson))
            {
                kind = CampaignViberContentKind.Survey;
            }

            if (kind == CampaignViberContentKind.Text && !string.IsNullOrEmpty(campaign.ViberMedia))
            {
                kind = InferLegacyContentKindFromMedia(campaign.ViberMedia);
            }

            return kind switch
            {
                CampaignViberContentKind.Survey => ViberSendMessageType.OneWaySurveyList,
                CampaignViberContentKind.File => ViberSendMessageType.OneWayFile,
                CampaignViberContentKind.Image => string.IsNullOrWhiteSpace(campaign.ViberButtonUrl)
                    ? ViberSendMessageType.OneWayImageOnly
                    : ViberSendMessageType.OneWayTextImageButton,
                CampaignViberContentKind.Video => DetermineVideoMessageType(campaign),
                _ => DetermineTextOnlyMessageType(campaign)
            };
        }

        private static CampaignViberContentKind InferLegacyContentKindFromMedia(string media)
        {
            try
            {
                return MediaHelper.GetMediaType(media) == CampaignMediaType.Image
                    ? CampaignViberContentKind.Image
                    : CampaignViberContentKind.Video;
            }
            catch (Exception)
            {
                return MediaHelper.TryGetViberDocumentFileType(media, out _)
                    ? CampaignViberContentKind.File
                    : CampaignViberContentKind.Text;
            }
        }

        /// <summary>
        /// Comtrade 230–233: 230–232 carry the video URL in <c>ButtonUrl</c>; 233 carries video in <c>MediaUrl</c> and the action in <c>ButtonUrl</c>.
        /// Use <see cref="VideoButtonUrlMatchesMedia"/> so a duplicate/pasted video link does not force type 233.
        /// </summary>
        public static ViberSendMessageType DetermineVideoOutboundType(
            string? viberMedia,
            string? messageText,
            string? viberButtonUrl,
            string? viberButtonUrlTitle)
        {
            if (string.IsNullOrEmpty(viberMedia))
            {
                return ViberSendMessageType.OneWayTextOnly;
            }

            try
            {
                if (MediaHelper.GetMediaType(viberMedia) != CampaignMediaType.Video)
                {
                    return ViberSendMessageType.OneWayTextOnly;
                }
            }
            catch (Exception)
            {
                return ViberSendMessageType.OneWayTextOnly;
            }

            bool hasMsg = !string.IsNullOrWhiteSpace(messageText);
            bool hasCaption = !string.IsNullOrWhiteSpace(viberButtonUrlTitle);
            bool hasBtnUrl = !string.IsNullOrWhiteSpace(viberButtonUrl);

            if (!hasMsg)
            {
                return ViberSendMessageType.OneWayVideo;
            }

            if (!hasCaption)
            {
                if (!hasBtnUrl)
                {
                    return ViberSendMessageType.OneWayVideoText;
                }

                return VideoButtonUrlMatchesMedia(viberMedia, viberButtonUrl)
                    ? ViberSendMessageType.OneWayVideoText
                    : ViberSendMessageType.OneWayVideoTextActionButton;
            }

            if (!hasBtnUrl || VideoButtonUrlMatchesMedia(viberMedia, viberButtonUrl))
            {
                return ViberSendMessageType.OneWayVideoTextButton;
            }

            return ViberSendMessageType.OneWayVideoTextActionButton;
        }

        /// <summary>True if <paramref name="viberButtonUrl"/> refers to the same video asset as <paramref name="viberMedia"/> (equality or same file name in path/URL).</summary>
        public static bool VideoButtonUrlMatchesMedia(string? viberMedia, string? viberButtonUrl)
        {
            if (string.IsNullOrWhiteSpace(viberButtonUrl) || string.IsNullOrWhiteSpace(viberMedia))
            {
                return false;
            }

            string m = viberMedia.Trim();
            string b = viberButtonUrl.Trim();

            if (string.Equals(m, b, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            string fileNameFromRef(string s)
            {
                if (s.Contains("://", StringComparison.Ordinal))
                {
                    try
                    {
                        var u = new Uri(s, UriKind.Absolute);
                        return Path.GetFileName(u.AbsolutePath);
                    }
                    catch (UriFormatException)
                    {
                        int q = s.IndexOf('?', StringComparison.Ordinal);
                        if (q >= 0)
                        {
                            s = s[..q];
                        }

                        return Path.GetFileName(s.Replace('\\', '/'));
                    }
                }

                int q2 = s.IndexOf('?', StringComparison.Ordinal);
                if (q2 >= 0)
                {
                    s = s[..q2];
                }

                return Path.GetFileName(s.Replace('\\', '/'));
            }

            string fm = fileNameFromRef(m);
            string fb = fileNameFromRef(b);
            return fm.Length > 0 && fb.Length > 0 && string.Equals(fm, fb, StringComparison.OrdinalIgnoreCase);
        }

        private static ViberSendMessageType DetermineVideoMessageType(Campaign campaign) =>
            DetermineVideoOutboundType(
                campaign.ViberMedia,
                campaign.ViberMessage,
                campaign.ViberButtonUrl,
                campaign.ViberButtonUrlTitle);

        private static ViberSendMessageType DetermineTextOnlyMessageType(Campaign campaign)
        {
            if (string.IsNullOrEmpty(campaign.ViberMedia) && string.IsNullOrEmpty(campaign.ViberButtonUrl))
            {
                return ViberSendMessageType.OneWayTextOnly;
            }

            if (string.IsNullOrEmpty(campaign.ViberMedia) && !string.IsNullOrEmpty(campaign.ViberButtonUrl))
            {
                return ViberSendMessageType.OneWayTextButton;
            }

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

            if (!string.IsNullOrEmpty(apiMessage.ViberMedia))
            {
                ViberSendMessageType videoType = DetermineVideoOutboundType(
                    apiMessage.ViberMedia,
                    apiMessage.Message,
                    apiMessage.ViberButtonUrl,
                    apiMessage.ViberButtonUrlTitle);

                if (videoType != ViberSendMessageType.OneWayTextOnly)
                {
                    return videoType;
                }

                try
                {
                    if (MediaHelper.GetMediaType(apiMessage.ViberMedia) == CampaignMediaType.Image
                        && !string.IsNullOrEmpty(apiMessage.ViberButtonUrl))
                    {
                        return ViberSendMessageType.OneWayTextImageButton;
                    }
                }
                catch (Exception)
                {
                    // fall through
                }
            }

            return ViberSendMessageType.OneWayTextOnly;
        }
    }
}