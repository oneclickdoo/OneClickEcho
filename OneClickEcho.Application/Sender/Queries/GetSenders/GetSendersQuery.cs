using OneClickEcho.Application.Common.Messaging;

namespace OneClickEcho.Application.Sender.Queries.GetSenders
{
    public record GetSendersQuery(Guid CompanyId) : IQuery<List<GetSendersResponse>>;
}
