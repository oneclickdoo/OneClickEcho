namespace OneClickEcho.Application.Company.Queries.GetCompanyById;

public record GetCompanyByIdResponse(
    Guid CompanyId,
    string Name,
    string? SmsUsername,
    string? SmsPassword,
    decimal ViberPricePerMesssage,
    decimal SmsPricePerMesssage,
    string? ApiPassword,
    DateTime CreatedAt);