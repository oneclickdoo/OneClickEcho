using OneClickEcho.Domain.LeadAggregate.Enums;

namespace OneClickEcho.Application.Lead.Queries.GetLeadById;

public sealed record GetLeadByIdResponse(
    string PhoneNumber,
    string? FirstName,
    string? LastName,
    LeadGender? Gender,
    string? Email,
    DateOnly? DateOfBirth,
    string? City,
    string? State,
    string? Country
);
