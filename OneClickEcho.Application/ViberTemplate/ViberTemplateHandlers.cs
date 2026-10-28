using MediatR;
using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Application.Common.Services.ViberService;
using OneClickEcho.Application.Common.Services.ViberService.Request;
using OneClickEcho.Application.Common.Services.ViberService.Request.Common;
using OneClickEcho.Application.Common.Services.ViberService.Response;
using OneClickEcho.Application.Common.Viber;
using OneClickEcho.Domain.Common.Repositories;
using OneClickEcho.Domain.Common.Shared;
using OneClickEcho.Domain.CompanyAggregate.Repositories;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;
using OneClickEcho.Domain.ViberTemplateAggregate;
using OneClickEcho.Domain.ViberTemplateAggregate.Repositories;
using OneClickEcho.Domain.ViberTemplateAggregate.ValueObjects;

namespace OneClickEcho.Application.ViberTemplate;

public class CreateViberTemplateHandler(
    IViberTemplateRepository repository,
    IViberService viberService,
    IViberAccount account,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateViberTemplateCommand, ViberTemplateResponse>
{
    public async Task<Result<ViberTemplateResponse>> Handle(CreateViberTemplateCommand request, CancellationToken cancellationToken)
    {
        if (!account.IsConfigured)
        {
            return Fail("Viber.NotConfigured", "Viber credentials are not configured.");
        }

        try
        {
            ViberTemplateRules.EnsureLanguage(request.Language);
            ViberTemplateRules.ValidateBody(request.Body);
        }
        catch (InvalidOperationException ex)
        {
            return Fail("ViberTemplate.Invalid", ex.Message);
        }

        if (string.IsNullOrWhiteSpace(request.Display) || string.IsNullOrWhiteSpace(request.Name))
        {
            return Fail("ViberTemplate.Invalid", "Template name and sender are required.");
        }

        CreateViberTemplateResponseDto? created = await viberService.CreateTemplate(new CreateViberTemplateRequestDto
        {
            UserCredentials = Credentials(account),
            Name = request.Name.Trim(),
            Display = request.Display.Trim(),
            Language = request.Language.Trim(),
            Body = request.Body.Trim(),
            Tag = string.IsNullOrWhiteSpace(request.Tag) ? null : request.Tag.Trim(),
            Category = "Transactional"
        });

        if (created is not { Success: true, Template: not null })
        {
            return Fail("ViberTemplate.ProviderError", "Comtrade did not accept the template.");
        }

        Domain.ViberTemplateAggregate.ViberTemplate template = new(
            CompanyId.Create(request.CompanyId),
            request.Display.Trim(),
            created.Template.Name ?? request.Name.Trim(),
            created.Template.Language ?? request.Language.Trim(),
            created.Template.Body ?? request.Body.Trim(),
            created.Template.Tag ?? request.Tag,
            created.Template.Id,
            created.Template.ViberTemplateId,
            (created.Template.Status ?? "CREATED").ToUpperInvariant());

        repository.Add(template);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(template);
    }

    internal static ViberUserCredentials Credentials(IViberAccount account) => new()
    {
        UserName = account.Username,
        Password = account.Password
    };

    internal static ViberTemplateResponse Map(Domain.ViberTemplateAggregate.ViberTemplate template) => new(
        template.Id.Value,
        template.CompanyId.Value,
        template.Display,
        template.Name,
        template.Language,
        template.Body,
        template.Tag,
        template.ComtradeId,
        template.ProviderTemplateId,
        template.Status,
        template.CanSend,
        template.CreatedAt,
        template.UpdatedAt);

    internal static Result<ViberTemplateResponse> Fail(string code, string message) =>
        Result.Failure<ViberTemplateResponse>(new Error(code, message));
}

public class CreateViberTemplateApiHandler(ICompanyRepository companies, IMediator mediator)
    : ICommandHandler<CreateViberTemplateApiCommand, ViberTemplateResponse>
{
    public async Task<Result<ViberTemplateResponse>> Handle(CreateViberTemplateApiCommand request, CancellationToken cancellationToken)
    {
        if (!await companies.CompanyApiValidation(CompanyId.Create(request.CompanyId), request.ApiPassword, cancellationToken))
        {
            return Result.Failure<ViberTemplateResponse>(new Error("Company.InvalidApiPassword", "The company api password is invalid."));
        }

        return await mediator.Send(new CreateViberTemplateCommand(
            request.CompanyId, request.Display, request.Name, request.Language, request.Body, request.Tag), cancellationToken);
    }
}

public class ListViberTemplatesHandler(IViberTemplateRepository repository)
    : IQueryHandler<ListViberTemplatesQuery, List<ViberTemplateResponse>>
{
    public async Task<Result<List<ViberTemplateResponse>>> Handle(ListViberTemplatesQuery query, CancellationToken cancellationToken)
    {
        List<Domain.ViberTemplateAggregate.ViberTemplate> rows =
            await repository.ListByCompanyAsync(CompanyId.Create(query.CompanyId), cancellationToken);
        return rows.Select(CreateViberTemplateHandler.Map).ToList();
    }
}

public class ListViberTemplatesApiHandler(ICompanyRepository companies, IMediator mediator)
    : IQueryHandler<ListViberTemplatesApiQuery, List<ViberTemplateResponse>>
{
    public async Task<Result<List<ViberTemplateResponse>>> Handle(ListViberTemplatesApiQuery query, CancellationToken cancellationToken)
    {
        if (!await companies.CompanyApiValidation(CompanyId.Create(query.CompanyId), query.ApiPassword, cancellationToken))
        {
            return Result.Failure<List<ViberTemplateResponse>>(new Error("Company.InvalidApiPassword", "The company api password is invalid."));
        }

        return await mediator.Send(new ListViberTemplatesQuery(query.CompanyId), cancellationToken);
    }
}

public class DeleteViberTemplateHandler(
    IViberTemplateRepository repository,
    IViberService viberService,
    IViberAccount account,
    IUnitOfWork unitOfWork) : ICommandHandler<DeleteViberTemplateCommand>
{
    public async Task<Result> Handle(DeleteViberTemplateCommand request, CancellationToken cancellationToken)
    {
        Domain.ViberTemplateAggregate.ViberTemplate? template =
            await repository.GetByIdAsync(ViberTemplateId.Create(request.TemplateId), cancellationToken);
        if (template is null || template.CompanyId.Value != request.CompanyId)
        {
            return Result.Failure(new Error("ViberTemplate.NotFound", "Template was not found."));
        }

        DeleteViberTemplateResponseDto? deleted = await viberService.DeleteTemplate(new ViberTemplateIdRequestDto
        {
            UserCredentials = CreateViberTemplateHandler.Credentials(account),
            Id = template.ComtradeId
        });

        if (deleted is not { Success: true })
        {
            return Result.Failure(new Error("ViberTemplate.ProviderError", "Comtrade did not delete the template."));
        }

        repository.Remove(template);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class DeleteViberTemplateApiHandler(ICompanyRepository companies, IMediator mediator)
    : ICommandHandler<DeleteViberTemplateApiCommand>
{
    public async Task<Result> Handle(DeleteViberTemplateApiCommand request, CancellationToken cancellationToken)
    {
        if (!await companies.CompanyApiValidation(CompanyId.Create(request.CompanyId), request.ApiPassword, cancellationToken))
        {
            return Result.Failure(new Error("Company.InvalidApiPassword", "The company api password is invalid."));
        }

        return await mediator.Send(new DeleteViberTemplateCommand(request.CompanyId, request.TemplateId), cancellationToken);
    }
}

public class RefreshViberTemplateHandler(
    IViberTemplateRepository repository,
    IViberService viberService,
    IViberAccount account,
    IUnitOfWork unitOfWork) : ICommandHandler<RefreshViberTemplateCommand, ViberTemplateResponse>
{
    public async Task<Result<ViberTemplateResponse>> Handle(RefreshViberTemplateCommand request, CancellationToken cancellationToken)
    {
        Domain.ViberTemplateAggregate.ViberTemplate? template =
            await repository.GetByIdAsync(ViberTemplateId.Create(request.TemplateId), cancellationToken);
        if (template is null || template.CompanyId.Value != request.CompanyId)
        {
            return CreateViberTemplateHandler.Fail("ViberTemplate.NotFound", "Template was not found.");
        }

        ViberTemplateListResponseDto? refreshed = await viberService.RefreshTemplate(new ViberTemplateIdRequestDto
        {
            UserCredentials = CreateViberTemplateHandler.Credentials(account),
            Id = template.ComtradeId
        });

        ComtradeViberTemplateDto? row = refreshed?.Templates?.FirstOrDefault();
        if (refreshed is not { Success: true } || row is null)
        {
            return CreateViberTemplateHandler.Fail("ViberTemplate.ProviderError", "Comtrade did not return the template status.");
        }

        template.Status = (row.Status ?? template.Status).ToUpperInvariant();
        template.ProviderTemplateId = row.ViberTemplateId ?? template.ProviderTemplateId;
        template.Body = row.Body ?? template.Body;
        template.UpdatedAt = DateTime.UtcNow;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return CreateViberTemplateHandler.Map(template);
    }
}

public class RefreshViberTemplateApiHandler(ICompanyRepository companies, IMediator mediator)
    : ICommandHandler<RefreshViberTemplateApiCommand, ViberTemplateResponse>
{
    public async Task<Result<ViberTemplateResponse>> Handle(RefreshViberTemplateApiCommand request, CancellationToken cancellationToken)
    {
        if (!await companies.CompanyApiValidation(CompanyId.Create(request.CompanyId), request.ApiPassword, cancellationToken))
        {
            return CreateViberTemplateHandler.Fail("Company.InvalidApiPassword", "The company api password is invalid.");
        }

        return await mediator.Send(new RefreshViberTemplateCommand(request.CompanyId, request.TemplateId), cancellationToken);
    }
}
