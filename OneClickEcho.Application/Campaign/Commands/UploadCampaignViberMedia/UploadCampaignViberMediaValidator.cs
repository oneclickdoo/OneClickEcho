using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace OneClickEcho.Application.Campaign.Commands.UploadCampaignViberMedia
{
    public class UploadCampaignViberMediaValidator : AbstractValidator<UploadCampaignViberMediaCommand>
    {
        public UploadCampaignViberMediaValidator()
        {
            RuleFor(x => x.CampaignId)
                .NotEmpty()
                .WithMessage("CampaignId is required.");

            RuleFor(x => x.File)
                .NotNull()
                .WithMessage("Media is required.")
                .Must(file => file.Length > 0)
                .WithMessage("Media cannot be empty.")
                .Must(file => CheckFileType(file))
                .WithMessage("Only image or video files are allowed.");

            RuleFor(x => x.File.Length)
                .LessThanOrEqualTo(5 * 1024 * 1024)
                .WithMessage("File size must be less than 5 MB.");
        }

        private static bool CheckFileType(IFormFile file)
        {
            string[] allowedExtensions = [".jpg", ".jpeg", ".png", ".mp4"];

            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            return allowedExtensions.Contains(extension);
        }
    }
}
