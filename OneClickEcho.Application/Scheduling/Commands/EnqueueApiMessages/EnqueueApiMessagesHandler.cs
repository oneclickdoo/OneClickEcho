using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.ApiMessageAggregate.Repositories;
using OneClickEcho.Domain.Common.Shared;

namespace OneClickEcho.Application.Scheduling.Commands.EnqueueApiMessages;

public class EnqueueApiMessagesHandler(IApiMessageRepository apiMessageRepository)
    : ICommandHandler<EnqueueApiMessagesCommand, EnqueueApiMessagesResponse>
{
    private readonly IApiMessageRepository _apiMessageRepository = apiMessageRepository;

    public async Task<Result<EnqueueApiMessagesResponse>> Handle(EnqueueApiMessagesCommand request, CancellationToken cancellationToken)
    {
        List<Domain.ApiMessageAggregate.ApiMessage> apiMessages = await _apiMessageRepository.GetUnsentApiMessages(DateTime.Now.AddMinutes(-10), cancellationToken);
        
        return new EnqueueApiMessagesResponse(apiMessages);
    }
}