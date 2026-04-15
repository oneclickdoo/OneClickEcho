using OneClickEcho.Application.Common.Helpers;
using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Application.Common.Services;
using OneClickEcho.Domain.CampaignAggregate.Repositories;
using OneClickEcho.Domain.CampaignAggregate.ValueObjects;
using OneClickEcho.Domain.Common.Repositories;
using OneClickEcho.Domain.Common.Shared;

namespace OneClickEcho.Application.Campaign.Commands.UploadCampaignViberMedia
{
    public class UploadCampaignViberMediaHandler(IFileStorageService fileStorageService,
        ICampaignRepository campaignRepository,
        IUnitOfWork unitOfWork) : ICommandHandler<UploadCampaignViberMediaCommand, UploadCampaignViberMediaResponse>
    {
        private readonly IFileStorageService _fileStorageService = fileStorageService;
        private readonly ICampaignRepository _campaignRepository = campaignRepository;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<Result<UploadCampaignViberMediaResponse>> Handle(UploadCampaignViberMediaCommand request,
            CancellationToken cancellationToken)
        {
            // get campaign
            Domain.CampaignAggregate.Campaign? campaign = await _campaignRepository
                .GetByIdAsync(CampaignId.Create(request.CampaignId), cancellationToken);

            if (campaign is null)
            {
                return Result.Failure<UploadCampaignViberMediaResponse>(new Error(
                    "Campaign.NotFound",
                    $"The Campaign with Id:\"{request.CampaignId}\" does not exist."
                ));
            }

            // check if campaign supports Viber channel
            if (!campaign.IsViber)
            {
                return Result.Failure<UploadCampaignViberMediaResponse>(new Error(
                    "Campaign.BadRequest",
                    $"The Campaign doesn't support Viber channel."
                ));
            }

            // save file
            string filename = await _fileStorageService.SaveFileAsync(request.File, cancellationToken);

            // check if uploaded media is for thumbnail
            if (request.IsThumbnail)
            {
                campaign.ViberVideoThumbnail = filename;
            }
            else
            {
                campaign.ViberMedia = filename;
                campaign.ViberFileSize = (int?)request.File.Length;

                if (MediaHelper.TryGetViberDocumentFileType(filename, out _))
                {
                    campaign.ViberVideoDuration = null;
                }
                else
                {
                    CampaignMediaType? mediaType = MediaHelper.GetMediaType(campaign.ViberMedia);
                    if (mediaType is CampaignMediaType.Video)
                    {
                        campaign.ViberVideoDuration = request.Duration;
                    }
                    else
                    {
                        campaign.ViberVideoDuration = null;
                    }
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new UploadCampaignViberMediaResponse(filename);
        }
    }
}
