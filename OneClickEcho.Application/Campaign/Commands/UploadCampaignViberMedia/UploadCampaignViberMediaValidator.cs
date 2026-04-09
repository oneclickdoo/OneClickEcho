using FluentValidation;
using Microsoft.AspNetCore.Http;
using OneClickEcho.Application.Common.Viber;

namespace OneClickEcho.Application.Campaign.Commands.UploadCampaignViberMedia
{
    public class UploadCampaignViberMediaValidator : AbstractValidator<UploadCampaignViberMediaCommand>
    {
        public UploadCampaignViberMediaValidator()
        {
            const long maxBytes = 200L * 1024 * 1024; // align with typical nginx client_max_body_size (e.g. 200M)

            RuleFor(x => x.CampaignId)
                .NotEmpty()
                .WithMessage("CampaignId is required.");

            RuleFor(x => x.File)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .WithMessage("Media is required.")
                .Must(file => file!.Length > 0)
                .WithMessage("Media cannot be empty.")
                .Must(file => CheckFileType(file!))
                .WithMessage("Only image or video files are allowed.")
                .Must(file => file!.Length <= maxBytes)
                .WithMessage("File size must be less than 200 MB.");

            When(x => !x.IsThumbnail && x.File != null && IsMp4Video(x.File), () =>
            {
                RuleFor(x => x.Duration)
                    .NotNull()
                    .WithMessage(
                        $"Video duration in seconds is required (Viber allows up to {ViberVideoConstraints.MaxDurationSeconds} s). Wait for preview to finish loading or re-select the file.")
                    .InclusiveBetween(1, ViberVideoConstraints.MaxDurationSeconds)
                    .WithMessage(
                        $"Video duration must be between 1 and {ViberVideoConstraints.MaxDurationSeconds} seconds (Viber limit for message types 230–232).");
            });
        }

        private static bool IsMp4Video(IFormFile file)
        {
            string ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (ext == ".mp4")
            {
                return true;
            }

            string? ct = file.ContentType?.ToLowerInvariant();
            return ct is "video/mp4" or "application/mp4";
        }

        private static bool CheckFileType(IFormFile file)
        {
            string[] allowedExtensions = [".jpg", ".jpeg", ".png", ".mp4"];

            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            return allowedExtensions.Contains(extension);
        }
    }
}
