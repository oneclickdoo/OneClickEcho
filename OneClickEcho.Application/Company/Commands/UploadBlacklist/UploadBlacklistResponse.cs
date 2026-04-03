namespace OneClickEcho.Application.Company.Commands.UploadBlacklist
{
    public sealed record UploadBlacklistResponse(
        Guid Id,
        int LeadsAdded);
}
