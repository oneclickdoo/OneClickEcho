using OneClickEcho.Application.Common.Messaging;

namespace OneClickEcho.Application.Sender.Commands.SetSenderTwoWay
{
    public sealed record SetSenderTwoWayCommand(Guid Id, bool IsTwoWay) : ICommand<SetSenderTwoWayResponse>;
}
