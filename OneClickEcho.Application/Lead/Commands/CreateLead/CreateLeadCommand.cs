using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.LeadAggregate.Enums;

namespace OneClickEcho.Application.Lead.Commands.CreateLead;

public sealed record CreateLeadCommand(
    Guid CompanyId,
    Guid? CampaignId,
    string PhoneNumber,
    string? FirstName,
    string? LastName,
    LeadGender? Gender,
    string? Email,
    DateOnly? DateOfBirth,
    string? City,
    string? State,
    string? Country
    ) : ICommand<CreateLeadResponse>;
