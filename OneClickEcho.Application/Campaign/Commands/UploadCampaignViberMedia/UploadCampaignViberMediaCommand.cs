using Microsoft.AspNetCore.Http;
using OneClickEcho.Application.Common.Messaging;

namespace OneClickEcho.Application.Campaign.Commands.UploadCampaignViberMedia
{
    public sealed record UploadCampaignViberMediaCommand(
        Guid CampaignId,
        IFormFile File,
        int? Duration,
        bool IsThumbnail
        ) : ICommand<UploadCampaignViberMediaResponse>;
}
