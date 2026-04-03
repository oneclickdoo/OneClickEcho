using OneClickEcho.Application.Common.Messaging;

namespace OneClickEcho.Application.Scheduling.Commands.EnqueueApiMessages;

public sealed record EnqueueApiMessagesCommand: ICommand<EnqueueApiMessagesResponse>;