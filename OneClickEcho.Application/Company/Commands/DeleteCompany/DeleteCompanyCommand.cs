using OneClickEcho.Application.Common.Messaging;

namespace OneClickEcho.Application.Company.Commands.DeleteCompany;

public record DeleteCompanyCommand(Guid Id) : ICommand<DeleteCompanyResponse>;

