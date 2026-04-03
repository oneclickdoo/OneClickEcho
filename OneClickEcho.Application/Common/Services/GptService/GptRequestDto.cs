using OneClickEcho.Domain.GptRequestAggregate.Enums;

namespace OneClickEcho.Application.Common.Services.GptService;

public sealed record GptRequestDto
{
    public string RequestMessage { get; set; } = string.Empty;

    public GptRequestType RequestType { get; set; }

    public Guid? CampaignId { get; set; }

    public Guid? NounCaseId { get; set; }
}