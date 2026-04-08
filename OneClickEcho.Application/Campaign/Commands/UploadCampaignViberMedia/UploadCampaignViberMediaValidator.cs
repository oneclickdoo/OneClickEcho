using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace OneClickEcho.Application.Campaign.Commands.UploadCampaignViberMedia
{
    public class UploadCampaignViberMediaValidator : AbstractValidator<UploadCampaignViberMediaCommand>
    {
        public UploadCampaignViberMediaValidator()
        {
            const long maxBytes = 100L * 1024 * 1024; // align with typical nginx client_max_body_size

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
                .WithMessage("File size must be less than 100 MB.");
        }

        private static bool CheckFileType(IFormFile file)
        {
            string[] allowedExtensions = [".jpg", ".jpeg", ".png", ".mp4"];

            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            return allowedExtensions.Contains(extension);
        }
    }
}
