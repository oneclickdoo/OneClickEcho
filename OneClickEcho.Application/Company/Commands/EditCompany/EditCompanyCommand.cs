using OneClickEcho.Application.Common.Messaging;

namespace OneClickEcho.Application.Company.Commands.EditCompany;

public record EditCompanyCommand(
    Guid CompanyId,
    string Name,
    string? SmsUsername,
    string? SmsPassword,
    decimal ViberPricePerMesssage,
    decimal SmsPricePerMesssage,
    string? ApiPassword
    ) : ICommand<EditCompanyResponse>;
