using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace OneClickEcho.Application.Lead.Commands.UploadLeads;

public class UploadLeadsValidator : AbstractValidator<UploadLeadsCommand>
{
    public UploadLeadsValidator()
    {
        RuleFor(x => x.File)
            .NotNull()
            .WithMessage("File is required.")
            .Must(file => file.Length > 0)
            .WithMessage("File cannot be empty.")
            .Must(file => CheckFileType(file))
            .WithMessage("Only CSV files are allowed.");

        RuleFor(x => x.File.Length)
            .LessThanOrEqualTo(5 * 1024 * 1024)
            .WithMessage("File size must be less than 5 MB.");

        RuleFor(x => x.CompanyId)
            .NotEmpty()
            .WithMessage("CompanyId is required.")
            .Must(id => id != Guid.Empty)
            .WithMessage("CompanyId must be a valid GUID.");
    }

    private static bool CheckFileType(IFormFile file)
    {
        string[] allowedExtensions = [".csv"];

        string extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        return allowedExtensions.Contains(extension);
    }
}