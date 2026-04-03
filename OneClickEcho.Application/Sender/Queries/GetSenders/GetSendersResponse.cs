using OneClickEcho.Domain.CompanyAggregate.Enums;

namespace OneClickEcho.Application.Sender.Queries.GetSenders
{
    public record GetSendersResponse(
        Guid Id,
        string Name,
        SenderType Type
        );
}
