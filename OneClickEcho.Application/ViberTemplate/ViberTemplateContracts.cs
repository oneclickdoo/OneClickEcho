using OneClickEcho.Application.Common.Messaging;

namespace OneClickEcho.Application.ViberTemplate;

public record ViberTemplateResponse(
    Guid Id,
    Guid CompanyId,
    string Display,
    string Name,
    string Language,
    string Body,
    string? Tag,
    int ComtradeId,
    string? ProviderTemplateId,
    string Status,
    bool CanSend,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record CreateViberTemplateCommand(
    Guid CompanyId,
    string Display,
    string Name,
    string Language,
    string Body,
    string? Tag) : ICommand<ViberTemplateResponse>;

public record CreateViberTemplateApiCommand(
    Guid CompanyId,
    string ApiPassword,
    string Display,
    string Name,
    string Language,
    string Body,
    string? Tag) : ICommand<ViberTemplateResponse>;

public record ListViberTemplatesQuery(Guid CompanyId) : IQuery<List<ViberTemplateResponse>>;

public record ListViberTemplatesApiQuery(Guid CompanyId, string ApiPassword) : IQuery<List<ViberTemplateResponse>>;

public record DeleteViberTemplateCommand(Guid CompanyId, Guid TemplateId) : ICommand;

public record DeleteViberTemplateApiCommand(Guid CompanyId, string ApiPassword, Guid TemplateId) : ICommand;

public record RefreshViberTemplateCommand(Guid CompanyId, Guid TemplateId) : ICommand<ViberTemplateResponse>;

public record RefreshViberTemplateApiCommand(Guid CompanyId, string ApiPassword, Guid TemplateId) : ICommand<ViberTemplateResponse>;
