using Microsoft.Extensions.Options;
using OneClickEcho.Application.Common.Helpers;
using OneClickEcho.Application.Common.Services;
using OneClickEcho.Application.Common.Services.ViberService.Request;
using OneClickEcho.Application.Common.Services.ViberService.Request.Common;
using OneClickEcho.Application.Common.Services.ViberService.Request.Enum;
using OneClickEcho.Application.Common.Services.ViberService.Response;
using OneClickEcho.Application.Common.Services.ViberService.Response.Common;
using OneClickEcho.Application.Common.Services.ViberService.Response.Enum;
using OneClickEcho.Domain.ApiMessageAggregate;
using OneClickEcho.Domain.CampaignAggregate;
using OneClickEcho.Domain.CampaignLeadAggregate;
using OneClickEcho.Domain.CampaignLeadAggregate.Enums;
using OneClickEcho.Domain.CampaignLeadAggregate.Repositories;
using OneClickEcho.Domain.Common.Repositories;
using OneClickEcho.Domain.LeadAggregate;
using OneClickEcho.Domain.TestMessageAggregate;
using OneClickEcho.Infrastructure.Settings;

namespace OneClickEcho.Infrastructure.Services.MessageHandling.Viber
{
    public class ViberSendingService
    {
        private static readonly int MAX_RECORDS_PER_REQUEST = 200;
        private static readonly string UploadsUrl = "https://api.echo.oneclick.rs/uploads";

        public static async Task SendViberMessagesToTestPhoneNumbers(
            Campaign campaign,
            TestMessage testMessage,
            IHttpClientFactory httpClientFactory,
            IOptions<ViberSettings> viberSettings)
        {
            // Setup HTTP request
            HttpClient httpClient = httpClientFactory.CreateClient("ViberHttpClient");

            ViberService viberService = new(httpClient);

            ViberUserCredentials credentials = new()
            {
                UserName = viberSettings.Value.Username,
                Password = viberSettings.Value.Password
            };

            string[][] dividedPhoneNumbers = new string[1][];
            dividedPhoneNumbers[0] = new string[1];
            dividedPhoneNumbers[0][0] = testMessage.PhoneNumber;

            // Get data for Media
            int? duration = null;
            int? fileSize = null;
            string? imageUrl = null;
            string? videoUrl = null;
            string? videoThumbnailUrl = null;

            if (campaign.ViberMedia is not null)
            {
                // get campaign media type
                CampaignMediaType? mediaType = MediaHelper.GetMediaType(campaign.ViberMedia);

                if (mediaType == CampaignMediaType.Image)
                {
                    imageUrl = $"{UploadsUrl}/{campaign.ViberMedia}";
                }
                else
                {
                    videoUrl = $"{UploadsUrl}/{campaign.ViberMedia}";
                    videoThumbnailUrl = $"{UploadsUrl}/{campaign.ViberVideoThumbnail}";
                    duration = campaign.ViberVideoDuration;
                    fileSize = campaign.ViberFileSize;
                }
            }

            ViberSendMessageType messageType = ViberService.DetermineMessageType(campaign);

            for (int i = 0; i < dividedPhoneNumbers.Length; i++)
            {
                List<ViberMessage> viberMessages = [];

                // aggregate viber messages for every test phone number
                foreach (string testPhoneNumber in dividedPhoneNumbers[i])
                {
                    ViberMessage viberMessage;
                    
                    int validity = campaign.ViberValidity ?? 86400;

                    switch (messageType)
                    {
                        case ViberSendMessageType.OneWayTextOnly:
                            viberMessage = new()
                            {
                                // Text
                                MessageText = campaign.ViberMessage!,
                                // Must have
                                Display = campaign.ViberSender!,
                                Label = "promotion",
                                MSISDN = testPhoneNumber,
                                MessageId = testMessage.ViberId,
                                MessageType = messageType,
                                Priority = 255,
                                Tag = "tag",
                                Validity = validity
                            };
                            break;
                        case ViberSendMessageType.OneWayTextButton:
                            viberMessage = new()
                            {
                                // Text
                                MessageText = campaign.ViberMessage!,
                                // Button
                                ButtonCaption = campaign.ViberButtonUrlTitle,
                                ButtonUrl = campaign.ViberButtonUrl,
                                // Must have
                                Display = campaign.ViberSender!,
                                Label = "promotion",
                                MSISDN = testPhoneNumber,
                                MessageId = testMessage.ViberId,
                                MessageType = messageType,
                                Priority = 255,
                                Tag = "tag",
                                Validity = validity
                            };
                            break;
                        case ViberSendMessageType.OneWayTextImageButton:
                            viberMessage = new()
                            {
                                // Text
                                MessageText = campaign.ViberMessage!,
                                // Image
                                ImageUrl = imageUrl, // "{UploadsUrl}/abf1617b-c5a1-4eb7-bec8-86f35e85a583.png"
                                // Button
                                ButtonCaption = campaign.ViberButtonUrlTitle,
                                ButtonUrl = campaign.ViberButtonUrl,
                                // Must have
                                Display = campaign.ViberSender!,
                                Label = "promotion",
                                MSISDN = testPhoneNumber,
                                MessageId = testMessage.ViberId,
                                MessageType = messageType,
                                Priority = 255,
                                Tag = "tag",
                                Validity = validity
                            };
                            break;
                        case ViberSendMessageType.OneWayVideoText:
                            viberMessage = new()
                            {
                                // Video
                                ButtonUrl = videoUrl, // "{UploadsUrl}/f5673afe-0710-4253-897e-0f4eed79b7d3.mp4"
                                Thumbnail = videoThumbnailUrl, // "{UploadsUrl}/41f86ea7-38b5-4fa2-bdbd-37b82a065e07.jpeg"
                                FileSize = fileSize,
                                Duration = duration,
                                // Text
                                MessageText = campaign.ViberMessage!,
                                // Must have
                                Display = campaign.ViberSender!,
                                Label = "promotion",
                                MSISDN = testPhoneNumber,
                                MessageId = testMessage.ViberId,
                                MessageType = messageType,
                                Priority = 255,
                                Tag = "tag",
                                Validity = validity
                            };
                            break;
                        case ViberSendMessageType.OneWayVideoTextButton:
                            viberMessage = new()
                            {
                                // Video
                                ButtonUrl = videoUrl, // "{UploadsUrl}/f5673afe-0710-4253-897e-0f4eed79b7d3.mp4"
                                Thumbnail = videoThumbnailUrl, // "{UploadsUrl}/41f86ea7-38b5-4fa2-bdbd-37b82a065e07.jpeg"
                                FileSize = fileSize,
                                Duration = duration,
                                // Text
                                MessageText = campaign.ViberMessage!,
                                // Button
                                ButtonCaption = campaign.ViberButtonUrlTitle,
                                // Must have
                                Display = campaign.ViberSender!,
                                Label = "promotion",
                                MSISDN = testPhoneNumber,
                                MessageId = testMessage.ViberId,
                                MessageType = messageType,
                                Priority = 255,
                                Tag = "tag",
                                Validity = validity
                            };
                            break;
                        default:
                            throw new Exception("Unhandled message type");
                    }

                    viberMessages.Add(viberMessage);
                }

                // send Viber messages to all test phone numbers in bulk
                SendViberMessageDto request = new()
                {
                    UserCredentials = credentials,
                    ViberMessages = viberMessages
                };

                SendViberMessageResponseDto? response = await viberService.Send(request);

                if (response != null)
                {
                    foreach (SendViberMessageResponse viberMessage in response.ViberMessageResponses)
                    {
                        Console.WriteLine($"Message id: {viberMessage.MessageId}, status: {viberMessage.Status}");
                    }
                }
            }
        }

        public static async Task SendViberMessagesToLeads(
            Campaign campaign,
            List<Lead> leads,
            IHttpClientFactory httpClientFactory,
            IOptions<ViberSettings> viberSettings,
            ICampaignLeadRepository campaignLeadRepository,
            IStringTemplatingService stringTemplatingService)
        {
            // Setup HTTP request
            HttpClient httpClient = httpClientFactory.CreateClient("ViberHttpClient");

            ViberService viberService = new(httpClient);

            ViberUserCredentials credentials = new()
            {
                UserName = viberSettings.Value.Username,
                Password = viberSettings.Value.Password
            };

            // Get data for Media
            int? duration = null;
            int? fileSize = null;
            string? imageUrl = null;
            string? videoUrl = null;
            string? videoThumbnailUrl = null;

            if (campaign.ViberMedia is not null)
            {
                // get campaign media type
                CampaignMediaType? mediaType = MediaHelper.GetMediaType(campaign.ViberMedia);

                if (mediaType == CampaignMediaType.Image)
                {
                    imageUrl = $"{UploadsUrl}/{campaign.ViberMedia}";
                }
                else
                {
                    videoUrl = $"{UploadsUrl}/{campaign.ViberMedia}";
                    videoThumbnailUrl = $"{UploadsUrl}/{campaign.ViberVideoThumbnail}";
                    duration = campaign.ViberVideoDuration;
                    fileSize = campaign.ViberFileSize;
                }
            }

            ViberSendMessageType messageType = ViberService.DetermineMessageType(campaign);

            // Split leads into batches
            List<List<Lead>> dividedLeads = [];

            for (int i = 0; i < leads.Count; i += MAX_RECORDS_PER_REQUEST)
            {
                dividedLeads.Add(leads.GetRange(i, Math.Min(MAX_RECORDS_PER_REQUEST, leads.Count - i)));
            }

            for (int i = 0; i < dividedLeads.Count; i++)
            {
                List<ViberMessage> viberMessages = [];

                // Aggregate viber messages for every test phone number
                foreach (Lead lead in dividedLeads[i])
                {
                    // Get campaign lead
                    CampaignLead campaignLead = await campaignLeadRepository
                        .GetByCampaignAndLeadId(campaign.Id, lead.Id)
                        ?? throw new Exception($"CampaignLead for Lead [{lead.Id}] is not found.");

                    // Message personalization
                    string message = stringTemplatingService.SubstituteLeadInfo(campaign.ViberMessage!, lead);

                    ViberMessage viberMessage;
                    
                    int validity = campaign.ViberValidity ?? 86400;

                    switch (messageType)
                    {
                        case ViberSendMessageType.OneWayTextOnly:
                            viberMessage = new()
                            {
                                // Text
                                MessageText = message,
                                // Must have
                                Display = campaign.ViberSender!,
                                Label = "promotion",
                                MSISDN = lead.PhoneNumber,
                                MessageId = campaignLead.ViberMessageId,
                                MessageType = messageType,
                                Priority = 255,
                                Tag = "tag",
                                Validity = validity
                            };
                            break;
                        case ViberSendMessageType.OneWayTextButton:
                            viberMessage = new()
                            {
                                // Text
                                MessageText = message,
                                // Button
                                ButtonCaption = campaign.ViberButtonUrlTitle,
                                ButtonUrl = campaign.ViberButtonUrl,
                                // Must have
                                Display = campaign.ViberSender!,
                                Label = "promotion",
                                MSISDN = lead.PhoneNumber,
                                MessageId = campaignLead.ViberMessageId,
                                MessageType = messageType,
                                Priority = 255,
                                Tag = "tag",
                                Validity = validity
                            };
                            break;
                        case ViberSendMessageType.OneWayTextImageButton:
                            viberMessage = new()
                            {
                                // Text
                                MessageText = message,
                                // Image
                                ImageUrl = imageUrl, // "{UploadsUrl}/abf1617b-c5a1-4eb7-bec8-86f35e85a583.png"
                                // Button
                                ButtonCaption = campaign.ViberButtonUrlTitle,
                                ButtonUrl = campaign.ViberButtonUrl,
                                // Must have
                                Display = campaign.ViberSender!,
                                Label = "promotion",
                                MSISDN = lead.PhoneNumber,
                                MessageId = campaignLead.ViberMessageId,
                                MessageType = messageType,
                                Priority = 255,
                                Tag = "tag",
                                Validity = validity
                            };
                            break;
                        case ViberSendMessageType.OneWayVideoText:
                            viberMessage = new()
                            {
                                // Video
                                ButtonUrl = videoUrl, // "{UploadsUrl}/f5673afe-0710-4253-897e-0f4eed79b7d3.mp4"
                                Thumbnail = videoThumbnailUrl, // "{UploadsUrl}/41f86ea7-38b5-4fa2-bdbd-37b82a065e07.jpeg"
                                FileSize = fileSize,
                                Duration = duration,
                                // Text
                                MessageText = message,
                                // Must have
                                Display = campaign.ViberSender!,
                                Label = "promotion",
                                MSISDN = lead.PhoneNumber,
                                MessageId = campaignLead.ViberMessageId,
                                MessageType = messageType,
                                Priority = 255,
                                Tag = "tag",
                                Validity = validity
                            };
                            break;
                        case ViberSendMessageType.OneWayVideoTextButton:
                            viberMessage = new()
                            {
                                // Video
                                ButtonUrl = videoUrl, // "{UploadsUrl}/f5673afe-0710-4253-897e-0f4eed79b7d3.mp4"
                                Thumbnail = videoThumbnailUrl, // "{UploadsUrl}/41f86ea7-38b5-4fa2-bdbd-37b82a065e07.jpeg"
                                FileSize = fileSize,
                                Duration = duration,
                                // Text
                                MessageText = message,
                                // Button
                                ButtonCaption = campaign.ViberButtonUrlTitle,
                                // Must have
                                Display = campaign.ViberSender!,
                                Label = "promotion",
                                MSISDN = lead.PhoneNumber,
                                MessageId = campaignLead.ViberMessageId,
                                MessageType = messageType,
                                Priority = 255,
                                Tag = "tag",
                                Validity = validity
                            };
                            break;
                        default:
                            throw new Exception("Unhandled message type");
                    }

                    viberMessages.Add(viberMessage);
                }

                // Send Viber messages to all test phone numbers in bulk
                SendViberMessageDto request = new()
                {
                    UserCredentials = credentials,
                    ViberMessages = viberMessages
                };

                SendViberMessageResponseDto? response = await viberService.Send(request);

                if (response is not null)
                {
                    foreach (SendViberMessageResponse viberMessage in response.ViberMessageResponses)
                    {
                        Console.WriteLine($"Message id: {viberMessage.MessageId}, status: {viberMessage.Status}");
                    }
                }
            }
        }

        public static async Task SendApiViberMessages(
            List<ApiMessage> apiMessages,
            IHttpClientFactory httpClientFactory,
            IOptions<ViberSettings> viberSettings,
            IUnitOfWork unitOfWork)
        {
            HttpClient httpClient = httpClientFactory.CreateClient("ViberHttpClient");

            ViberService viberService = new(httpClient);

            ViberUserCredentials credentials = new()
            {
                UserName = viberSettings.Value.Username,
                Password = viberSettings.Value.Password
            };
            
            List<List<ApiMessage>> dividedApiMessages = [];

            for (int i = 0; i < apiMessages.Count; i += MAX_RECORDS_PER_REQUEST)
            {
                dividedApiMessages.Add(apiMessages.GetRange(i, Math.Min(MAX_RECORDS_PER_REQUEST, apiMessages.Count - i)));
            }
            
            for (int i = 0; i < dividedApiMessages.Count; i++)
            {
                List<ViberMessage> viberMessages = [];

                // Aggregate viber messages for every test phone number
                foreach (ApiMessage apiMessage in dividedApiMessages[i])
                {
                    ViberMessage viberMessage;
                    
                    // 24 hours
                    int validity = apiMessage.ViberValidity ?? 86400;
                    ViberSendMessageType messageType = ViberService.DetermineApiMessageType(apiMessage);
                    
                    // Get data for Media
                    string? imageUrl = null;

                    if (apiMessage.ViberMedia is not null)
                    {
                        // get campaign media type
                        CampaignMediaType? mediaType = MediaHelper.GetMediaType(apiMessage.ViberMedia);

                        if (mediaType == CampaignMediaType.Image)
                        {
                            imageUrl = apiMessage.ViberMedia;
                        }
                    }

                    switch (messageType)
                    {
                        case ViberSendMessageType.OneWayTextOnly:
                            viberMessage = new()
                            {
                                // Text
                                MessageText = apiMessage.Message,
                                // Must have
                                Display = apiMessage.Sender!,
                                Label = "promotion",
                                MSISDN = apiMessage.PhoneNumber,
                                MessageId = apiMessage.ViberMessageId,
                                MessageType = messageType,
                                Priority = 255,
                                Tag = "tag",
                                Validity = validity
                            };
                            break;
                        case ViberSendMessageType.OneWayTextButton:
                            viberMessage = new()
                            {
                                // Text
                                MessageText = apiMessage.Message,
                                // Button
                                ButtonCaption = apiMessage.ViberButtonUrlTitle,
                                ButtonUrl = apiMessage.ViberButtonUrl,
                                // Must have
                                Display = apiMessage.Sender!,
                                Label = "promotion",
                                MSISDN = apiMessage.PhoneNumber,
                                MessageId = apiMessage.ViberMessageId,
                                MessageType = messageType,
                                Priority = 255,
                                Tag = "tag",
                                Validity = validity
                            };
                            break;
                        case ViberSendMessageType.OneWayTextImageButton:
                            viberMessage = new()
                            {
                                // Text
                                MessageText = apiMessage.Message,
                                // Image
                                ImageUrl = imageUrl, // "{UploadsUrl}/abf1617b-c5a1-4eb7-bec8-86f35e85a583.png"
                                // Button
                                ButtonCaption = apiMessage.ViberButtonUrlTitle,
                                ButtonUrl = apiMessage.ViberButtonUrl,
                                // Must have
                                Display = apiMessage.Sender!,
                                Label = "promotion",
                                MSISDN = apiMessage.PhoneNumber,
                                MessageId = apiMessage.ViberMessageId,
                                MessageType = messageType,
                                Priority = 255,
                                Tag = "tag",
                                Validity = validity
                            };
                            break;
                        default:
                            throw new Exception("Unhandled message type");
                    }

                    viberMessages.Add(viberMessage);
                }

                // Send Viber messages to all test phone numbers in bulk
                SendViberMessageDto request = new()
                {
                    UserCredentials = credentials,
                    ViberMessages = viberMessages
                };
                
                SendViberMessageResponseDto? response = await viberService.Send(request, 1);

                if (response is not null)
                {
                    foreach (SendViberMessageResponse viberMessage in response.ViberMessageResponses)
                    {
                        Console.WriteLine($"Message id: {viberMessage.MessageId}, status: {viberMessage.Status}");
                        
                        var sentApiMessage = apiMessages.FirstOrDefault(x => x.ViberMessageId == viberMessage.MessageId);

                        if (sentApiMessage is not null)
                        {
                            if (viberMessage.Status != ViberMessageResponseStatus.MSG_SUCCESS)
                            {
                                sentApiMessage.ViberStatus = CampaignLeadViberStatus.Undelivered;
                            }
                            
                            sentApiMessage.IsSent = true;
                        }
                    }
                }

                await unitOfWork.SaveChangesAsync();
            }
        }
    }
}