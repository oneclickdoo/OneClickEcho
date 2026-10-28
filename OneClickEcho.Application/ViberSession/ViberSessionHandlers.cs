using MediatR;
using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Application.Common.Viber;
using OneClickEcho.Domain.Common.Shared;
using OneClickEcho.Domain.CompanyAggregate.Repositories;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;
using OneClickEcho.Domain.ViberSessionAggregate;
using OneClickEcho.Domain.ViberSessionAggregate.Entities;
using SessionEntity = OneClickEcho.Domain.ViberSessionAggregate.ViberSession;
using OneClickEcho.Domain.ViberSessionAggregate.Repositories;
using OneClickEcho.Domain.ViberSessionAggregate.ValueObjects;

namespace OneClickEcho.Application.ViberSession;

public class ListViberSessionsHandler(IViberSessionRepository sessions, IViberSessionRepository messages)
    : IQueryHandler<ListViberSessionsQuery, List<ViberSessionResponse>>
{
    public async Task<Result<List<ViberSessionResponse>>> Handle(ListViberSessionsQuery query, CancellationToken cancellationToken)
    {
        List<SessionEntity> rows = await sessions.ListByCompanyAsync(CompanyId.Create(query.CompanyId), cancellationToken);
        List<ViberSessionResponse> result = [];
        foreach (SessionEntity row in rows)
        {
            List<ViberSessionMessage> thread = await messages.ListMessagesAsync(row.Id, cancellationToken);
            result.Add(ViberSessionMapper.Map(row, thread, DateTime.UtcNow));
        }

        return result;
    }
}

public class GetViberSessionHandler(IViberSessionRepository sessions)
    : IQueryHandler<GetViberSessionQuery, ViberSessionResponse>
{
    public async Task<Result<ViberSessionResponse>> Handle(GetViberSessionQuery query, CancellationToken cancellationToken)
    {
        SessionEntity? session = await sessions.GetByIdAsync(ViberSessionId.Create(query.SessionId), cancellationToken);
        if (session is null || session.CompanyId.Value != query.CompanyId)
        {
            return Result.Failure<ViberSessionResponse>(new Error("ViberSession.NotFound", "Session was not found."));
        }

        List<ViberSessionMessage> thread = await sessions.ListMessagesAsync(session.Id, cancellationToken);
        return ViberSessionMapper.Map(session, thread, DateTime.UtcNow);
    }
}

public class GetViberSessionByPhoneHandler(IViberSessionRepository sessions)
    : IQueryHandler<GetViberSessionByPhoneQuery, ViberSessionResponse>
{
    public async Task<Result<ViberSessionResponse>> Handle(GetViberSessionByPhoneQuery query, CancellationToken cancellationToken)
    {
        SessionEntity? session = await sessions.FindLatestAsync(
            CompanyId.Create(query.CompanyId),
            query.Sender.Trim(),
            ViberPhone.Normalize(query.PhoneNumber),
            cancellationToken);

        if (session is null)
        {
            return Result.Failure<ViberSessionResponse>(new Error("ViberSession.NotFound", "Session was not found."));
        }

        List<ViberSessionMessage> thread = await sessions.ListMessagesAsync(session.Id, cancellationToken);
        return ViberSessionMapper.Map(session, thread, DateTime.UtcNow);
    }
}

public class GetViberSessionByPhoneApiHandler(ICompanyRepository companies, IMediator mediator)
    : IQueryHandler<GetViberSessionByPhoneApiQuery, ViberSessionResponse>
{
    public async Task<Result<ViberSessionResponse>> Handle(GetViberSessionByPhoneApiQuery query, CancellationToken cancellationToken)
    {
        if (!await companies.CompanyApiValidation(CompanyId.Create(query.CompanyId), query.ApiPassword, cancellationToken))
        {
            return Result.Failure<ViberSessionResponse>(new Error("Company.InvalidApiPassword", "The company api password is invalid."));
        }

        return await mediator.Send(new GetViberSessionByPhoneQuery(query.CompanyId, query.Sender, query.PhoneNumber), cancellationToken);
    }
}

public class SendViberSessionHandler(IViberSessionSender sender)
    : ICommandHandler<SendViberSessionCommand, ViberSessionResponse>
{
    public Task<Result<ViberSessionResponse>> Handle(SendViberSessionCommand request, CancellationToken cancellationToken) =>
        sender.SendAsync(request, cancellationToken);
}

public class SendViberSessionApiHandler(ICompanyRepository companies, IMediator mediator)
    : ICommandHandler<SendViberSessionApiCommand, ViberSessionResponse>
{
    public async Task<Result<ViberSessionResponse>> Handle(SendViberSessionApiCommand request, CancellationToken cancellationToken)
    {
        if (!await companies.CompanyApiValidation(CompanyId.Create(request.CompanyId), request.ApiPassword, cancellationToken))
        {
            return Result.Failure<ViberSessionResponse>(new Error("Company.InvalidApiPassword", "The company api password is invalid."));
        }

        return await mediator.Send(new SendViberSessionCommand(
            request.CompanyId,
            request.Sender,
            request.PhoneNumber,
            request.Kind,
            request.Text,
            request.Media,
            request.QuickRepliesJson,
            request.SurveyUiType,
            request.AllowOutsideSession), cancellationToken);
    }
}
