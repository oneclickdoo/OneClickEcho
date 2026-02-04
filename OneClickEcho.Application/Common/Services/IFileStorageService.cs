using Microsoft.AspNetCore.Http;

namespace OneClickEcho.Application.Common.Services;

public interface IFileStorageService
{
    public Task<string> SaveFileAsync(IFormFile file, CancellationToken cancellationToken = default);
}
