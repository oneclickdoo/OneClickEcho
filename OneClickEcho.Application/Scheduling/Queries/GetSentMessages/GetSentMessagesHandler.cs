using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.ApiMessageAggregate.Repositories;
using OneClickEcho.Domain.Common.Shared;

namespace OneClickEcho.Application.Scheduling.Queries.GetSentMessages;

public class GetSentMessagesHandler(IApiMessageRepository apiMessageRepository)
    : IQueryHandler<GetSentMessagesQuery, GetSentMessagesResponse>
{
    private readonly IApiMessageRepository _apiMessageRepository = apiMessageRepository;

    public async Task<Result<GetSentMessagesResponse>> Handle(GetSentMessagesQuery request, CancellationToken cancellationToken)
    {
        var apiMessages = await _apiMessageRepository.GetSentApiMessages(DateTime.Now.AddHours(-48), cancellationToken);
        
        return new GetSentMessagesResponse(apiMessages);
    }
} 