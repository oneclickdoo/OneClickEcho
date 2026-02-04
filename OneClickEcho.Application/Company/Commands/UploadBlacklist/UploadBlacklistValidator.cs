using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace OneClickEcho.Application.Company.Commands.UploadBlacklist;

public class UploadBlacklistValidator : AbstractValidator<UploadBlacklistCommand>
{
    public UploadBlacklistValidator()
    {
        RuleFor(x => x.File)
            .NotNull()
            .WithMessage("File is required.")
            .Must(file => file.Length > 0)
            .WithMessage("File cannot be empty.")
            .Must(file => CheckFileType(file))
            .WithMessage("Only CSV files are allowed.");

        // You can add more rules if needed, such as size checks
        RuleFor(x => x.File.Length)
            .LessThanOrEqualTo(500 * 1024 * 1024)
            .WithMessage("File size must be less than 500 MB."); // Arbitrary size limit
    }

    private static bool CheckFileType(IFormFile file)
    {
        string[] allowedExtensions = [".csv"];

        string extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        return allowedExtensions.Contains(extension);
    }
}
