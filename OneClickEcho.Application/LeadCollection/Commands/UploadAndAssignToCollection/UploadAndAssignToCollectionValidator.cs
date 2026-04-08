using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace OneClickEcho.Application.LeadCollection.Commands.UploadAndAssignToCollection;

public class UploadAndAssignLeadsToCollectionValidator : AbstractValidator<UploadAndAssignLeadsToCollectionCommand>
{
    public UploadAndAssignLeadsToCollectionValidator()
    {
        // Cascade: do not evaluate .Length / file name when File is null (avoids 500 from validator).
        RuleFor(x => x.File)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithMessage("File is required.")
            .Must(file => file!.Length > 0)
            .WithMessage("File cannot be empty.")
            .Must(file => CheckFileType(file!))
            .WithMessage("Only CSV files are allowed.")
            .Must(file => file!.Length <= 5 * 1024 * 1024)
            .WithMessage("File size must be less than 5 MB.");
    }

    private static bool CheckFileType(IFormFile file)
    {
        string[] allowedExtensions = [".csv"];

        string extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        return allowedExtensions.Contains(extension);
    }
}
