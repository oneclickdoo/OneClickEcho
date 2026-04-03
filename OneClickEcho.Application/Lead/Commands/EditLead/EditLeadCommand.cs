using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.LeadAggregate.Enums;

namespace OneClickEcho.Application.Lead.Commands.EditLead;

public sealed record EditLeadCommand(
    Guid LeadId,
    string PhoneNumber,
    string? FirstName,
    string? LastName,
    LeadGender? Gender,
    string? Email,
    DateOnly? DateOfBirth,
    string? City,
    string? State,
    string? Country,
    bool? IsBlacklisted
    ) : ICommand<EditLeadResponse>;