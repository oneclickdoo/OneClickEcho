using OneClickEcho.Application.Common.Abstractions;
using OneClickEcho.Application.Common.Messaging;

namespace OneClickEcho.Application.ApiMessage.Queries.GetApiMessages;

public class GetApiMessagesQuery : BasePagedQuery, IQuery<GetApiMessagesResponse>
{
    public Guid CompanyId;
}