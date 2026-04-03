using OneClickEcho.Application.Common.Abstractions;
using OneClickEcho.Domain.ApiMessageAggregate.Enums;
using OneClickEcho.Domain.CampaignLeadAggregate.Enums;
using OneClickEcho.Domain.Common.Queries;

namespace OneClickEcho.Application.ApiMessage.Queries.GetApiMessages;

public class GetApiMessagesResponse(IPagedList<Domain.ApiMessageAggregate.ApiMessage> items)
    : PagedListDto<Domain.ApiMessageAggregate.ApiMessage, GetApiMessageDto>(items)
{
    public override List<GetApiMessageDto> ConvertTToTDto(List<Domain.ApiMessageAggregate.ApiMessage> items)
    {
        List<GetApiMessageDto> result = [];

        foreach (Domain.ApiMessageAggregate.ApiMessage apiMessage in items)
        {
            result.Add(new GetApiMessageDto(
                ApiMessageId: apiMessage.Id.Value,
                CompanyId: apiMessage.CompanyId.Value,
                PhoneNumber: apiMessage.PhoneNumber,
                Message: apiMessage.Message,
                MessageType: apiMessage.MessageType,
                HasSmsFallback: apiMessage.HasSmsFallback,
                ViberSender: apiMessage.Sender,
                ViberMedia: apiMessage.ViberMedia,
                ViberButtonUrl: apiMessage.ViberButtonUrl,
                ViberButtonUrlTitle: apiMessage.ViberButtonUrlTitle,
                ViberMessageId: apiMessage.ViberMessageId,
                ViberStatus: apiMessage.ViberStatus,
                ViberStatusDescription: apiMessage.ViberStatusDescription,
                SMSStatus: apiMessage.SMSStatus,
                SMSStatusDescription: apiMessage.SMSStatusDescription,
                SMSReferenceId: apiMessage.SMSReferenceId,
                CreatedAt: apiMessage.CreatedAt
            ));
        }

        return result;
    }
}

public record GetApiMessageDto(
    Guid ApiMessageId,
    Guid CompanyId,
    string PhoneNumber,
    string Message,
    ApiMessageType MessageType,
    bool HasSmsFallback,
    string? ViberSender,
    string? ViberMedia,
    string? ViberButtonUrl,
    string? ViberButtonUrlTitle,
    long ViberMessageId,
    CampaignLeadViberStatus ViberStatus,
    string? ViberStatusDescription, 
    CampaignLeadSMSStatus SMSStatus,
    string? SMSStatusDescription,
    string? SMSReferenceId,
    DateTime CreatedAt
);