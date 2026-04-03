using OneClickEcho.Application.Common.Messaging;

namespace OneClickEcho.Application.Scheduling.Commands.ExpirePendingMessages;

public sealed record ExpirePendingMessagesCommand : ICommand<ExpirePendingMessagesResponse>;