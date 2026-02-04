using OneClickEcho.Domain.ApiMessageAggregate;

namespace OneClickEcho.Application.Scheduling.Queries.GetSentMessages;

public record GetSentMessagesResponse(List<Domain.ApiMessageAggregate.ApiMessage> ApiMessages); 