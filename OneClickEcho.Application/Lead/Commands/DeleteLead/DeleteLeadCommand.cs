using OneClickEcho.Application.Common.Messaging;

namespace OneClickEcho.Application.Lead.Commands.DeleteLead
{
    public record DeleteLeadCommand(Guid LeadId) : ICommand<DeleteLeadResponse>;
}
