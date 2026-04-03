using OneClickEcho.Domain.Common.Primitives;
using OneClickEcho.Domain.CompanyAggregate.Entities;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;

namespace OneClickEcho.Domain.CompanyAggregate;

public sealed class Company : AggregateRoot<CompanyId>
{
    public Company(
        CompanyId id,
        string name,
        string? smsUsername,
        string? smsPassword,
        decimal viberPricePerMesssage,
        decimal smsPricePerMesssage
        ) : base(id)
    {
        Name = name;
        SmsUsername = smsUsername;
        SmsPassword = smsPassword;
        ViberPricePerMesssage = viberPricePerMesssage;
        SmsPricePerMesssage = smsPricePerMesssage;
    }

    public Company(
        string name,
        string? smsUsername,
        string? smsPassword
        ) : base(CompanyId.CreateUnique())
    {
        Name = name;
        SmsUsername = smsUsername;
        SmsPassword = smsPassword;
    }

    public Company(string name) : base(CompanyId.CreateUnique())
    {
        Name = name;
    }

    public string Name { get; set; } = string.Empty;

    public string? SmsUsername { get; set; } = string.Empty;

    public string? SmsPassword { get; set; } = string.Empty;

    public decimal ViberPricePerMesssage { get; set; }

    public decimal SmsPricePerMesssage { get; set; }
    
    public string? ApiPassword { get; set; } = string.Empty;

    public ICollection<Sender> Senders { get; } = [];

    // Used for EFCore
    public Company() : base(CompanyId.CreateUnique()) { }
}