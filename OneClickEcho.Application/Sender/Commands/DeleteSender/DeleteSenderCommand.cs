using OneClickEcho.Application.Common.Messaging;

namespace OneClickEcho.Application.Sender.Commands.DeleteSender
{
    public record DeleteSenderCommand(Guid Id) : ICommand<DeleteSenderResponse>;
}
