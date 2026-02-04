using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.ApiMessageAggregate.Repositories;
using OneClickEcho.Domain.Common.Queries;
using OneClickEcho.Domain.Common.Shared;

namespace OneClickEcho.Application.ApiMessage.Queries.GetApiMessages;

public class GetApiMessagesHandler(IApiMessageRepository apiMessageRepository)
    : IQueryHandler<GetApiMessagesQuery, GetApiMessagesResponse>
{
    public async Task<Result<GetApiMessagesResponse>> Handle(GetApiMessagesQuery query,
        CancellationToken cancellationToken)
    {
        IPagedList<Domain.ApiMessageAggregate.ApiMessage> apiMessages = await apiMessageRepository
            .GetPagedAsync(query, cancellationToken);

        return new GetApiMessagesResponse(apiMessages);
    }
}