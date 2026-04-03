using Microsoft.AspNetCore.Http;
using OneClickEcho.Application.Common.Messaging;

namespace OneClickEcho.Application.Lead.Commands.UploadLeads;

public record UploadLeadsCommand(
    IFormFile File,
    Guid CompanyId,
    Guid? CampaignId
    ) : ICommand<UploadLeadsResponse>;