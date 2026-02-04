using OneClickEcho.Application.Common.Messaging;

namespace OneClickEcho.Application.Company.Commands.CreateCompany;

public record CreateCompanyCommand(string Name) : ICommand<CreateCompanyResponse>;

