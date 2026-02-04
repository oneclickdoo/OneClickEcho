using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.CompanyAggregate.Enums;

namespace OneClickEcho.Application.Sender.Commands.CreateSender
{
    public sealed record CreateSenderCommand(
        Guid CompanyId,
        string Name,
        SenderType Type
        ) : ICommand<CreateSenderResponse>;
}
