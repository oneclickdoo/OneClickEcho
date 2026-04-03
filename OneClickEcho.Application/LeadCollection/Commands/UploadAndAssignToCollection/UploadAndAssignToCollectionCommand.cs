using Microsoft.AspNetCore.Http;
using OneClickEcho.Application.Common.Messaging;

namespace OneClickEcho.Application.LeadCollection.Commands.UploadAndAssignToCollection;

public sealed record UploadAndAssignLeadsToCollectionCommand(
    IFormFile File,
    Guid LeadCollectionId
) : ICommand<UploadAndAssignLeadsToCollectionResponse>;
