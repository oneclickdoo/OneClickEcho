using OneClickEcho.Domain.CampaignAggregate.ValueObjects;
using OneClickEcho.Domain.Common.Primitives;
using OneClickEcho.Domain.GptRequestAggregate.Enums;
using OneClickEcho.Domain.GptRequestAggregate.ValueObjects;
using OneClickEcho.Domain.NounCaseAggregate.ValueObjects;

namespace OneClickEcho.Domain.GptRequestAggregate;

public class GptRequest : AggregateRoot<GptRequestId>
{
    public GptRequest(
        GptRequestId id,
        string requestMessage,
        string responseMessage,
        GptRequestType gptRequestType,
        CampaignId? campaignId,
        NounCaseId? nounCaseId) : base(id)
    {
        RequestMessage = requestMessage;
        ResponseMessage = responseMessage;
        GptRequestType = gptRequestType;
        CampaignId = campaignId;
        NounCaseId = nounCaseId;
    }

    public GptRequest(
        string requestMessage,
        string responseMessage,
        GptRequestType gptRequestType,
        CampaignId? campaignId,
        NounCaseId? nounCaseId) : base(GptRequestId.CreateUnique())
    {
        RequestMessage = requestMessage;
        ResponseMessage = responseMessage;
        GptRequestType = gptRequestType;
        CampaignId = campaignId;
        NounCaseId = nounCaseId;
    }

    public string RequestMessage { get; set; } = string.Empty;

    public string ResponseMessage { get; set; } = string.Empty;

    public GptRequestType GptRequestType { get; set; }

    public CampaignId? CampaignId { get; set; }

    public NounCaseId? NounCaseId { get; set; }

    // Used for EFCore
    public GptRequest() : base(GptRequestId.CreateUnique()) { }
}