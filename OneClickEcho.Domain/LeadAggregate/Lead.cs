using OneClickEcho.Domain.Common.Primitives;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;
using OneClickEcho.Domain.LeadAggregate.Enums;
using OneClickEcho.Domain.LeadAggregate.ValueObjects;

namespace OneClickEcho.Domain.LeadAggregate;

public sealed class Lead : AggregateRoot<LeadId>
{
    public Lead(
        LeadId id,
        CompanyId companyId,
        string phoneNumber,
        string? firstName,
        string? lastName,
        LeadGender? gender,
        string? email,
        DateOnly? dateOfBirth,
        string? city,
        string? state,
        string? country
        ) : base(id)
    {
        CompanyId = companyId;
        PhoneNumber = phoneNumber;
        FirstName = firstName;
        LastName = lastName;
        Gender = gender;
        Email = email;
        DateOfBirth = dateOfBirth;
        City = city;
        State = state;
        Country = country;
    }

    public Lead(
        CompanyId companyId,
        string phoneNumber,
        string? firstName,
        string? lastName,
        LeadGender? gender,
        string? email,
        DateOnly? dateOfBirth,
        string? city,
        string? state,
        string? country
        ) : base(LeadId.CreateUnique())
    {
        CompanyId = companyId;
        PhoneNumber = phoneNumber;
        FirstName = firstName;
        LastName = lastName;
        Gender = gender;
        Email = email;
        DateOfBirth = dateOfBirth;
        City = city;
        State = state;
        Country = country;
    }

    public CompanyId CompanyId { get; set; } = default!;

    public string PhoneNumber { get; set; } = string.Empty;

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public LeadGender? Gender { get; set; }

    public string? Email { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? Country { get; set; }

    public bool IsBlacklisted { get; set; } = false;
    
    public bool IsUnsubscribed { get; set; } = false;

    // Used for EFCore
    public Lead() : base(LeadId.CreateUnique()) { }
}