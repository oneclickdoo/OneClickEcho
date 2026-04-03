using Microsoft.AspNetCore.Http;
using OneClickEcho.Application.Common.Services;

namespace OneClickEcho.Infrastructure.Services.DataManagement;

public class FileStorageService : IFileStorageService
{
    public const string UploadPath = "UploadedFiles";

    public async Task<string> SaveFileAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        // create upload directory (if doesn't exist)
        string folderPath = Path.Combine(Directory.GetCurrentDirectory(), UploadPath);

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // save the file
        string fileExtension = Path.GetExtension(file.FileName);

        string fileName = Guid.NewGuid() + fileExtension;

        string filePath = Path.Combine(folderPath, fileName);

        await using FileStream stream = new(filePath, FileMode.Create);

        await file.CopyToAsync(stream, cancellationToken);

        return fileName;
    }
}