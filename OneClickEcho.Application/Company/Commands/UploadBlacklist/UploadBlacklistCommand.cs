using Microsoft.AspNetCore.Http;
using OneClickEcho.Application.Common.Messaging;

namespace OneClickEcho.Application.Company.Commands.UploadBlacklist;

public sealed record UploadBlacklistCommand(
    IFormFile File,
    Guid CompanyId
) : ICommand<UploadBlacklistResponse>;
