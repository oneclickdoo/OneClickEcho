namespace OneClickEcho.Application.Scheduling.Commands.EnqueueApiMessages;

public record EnqueueApiMessagesResponse(List<Domain.ApiMessageAggregate.ApiMessage> ApiMessages);