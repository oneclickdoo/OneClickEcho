using Microsoft.Extensions.Options;
using OneClickEcho.Application.Common.Helpers;
using OneClickEcho.Application.Common.Services.ViberService;
using OneClickEcho.Application.Common.Services.ViberService.Request;
using OneClickEcho.Application.Common.Services.ViberService.Request.Common;
using OneClickEcho.Application.Common.Services.ViberService.Request.Enum;
using OneClickEcho.Application.Common.Services.ViberService.Response;
using OneClickEcho.Application.Common.Services.ViberService.Response.Common;
using OneClickEcho.Application.Common.Services.ViberService.Response.Enum;
using OneClickEcho.Application.Common.Viber;
using OneClickEcho.Application.ViberSession;
using OneClickEcho.Domain.Common.Repositories;
using OneClickEcho.Domain.Common.Shared;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;
using OneClickEcho.Domain.ViberSessionAggregate;
using OneClickEcho.Domain.ViberSessionAggregate.Entities;
using OneClickEcho.Domain.ViberSessionAggregate.Enums;
using OneClickEcho.Domain.ViberSessionAggregate.Repositories;
using OneClickEcho.Domain.ViberSessionAggregate.ValueObjects;
using OneClickEcho.Infrastructure.Settings;

namespace OneClickEcho.Infrastructure.Services.MessageHandling.Viber;

public class ViberSessionSender(
    IViberSessionRepository sessions,
    IViberService viberService,
    IViberAccount account,
    IUnitOfWork unitOfWork,
    IOptions<PublicUploadsSettings> uploads) : IViberSessionSender
{
    public async Task<Result<ViberSessionResponse>> SendAsync(SendViberSessionCommand request, CancellationToken cancellationToken)
    {
        if (!account.IsConfigured)
        {
            return Fail("Viber.NotConfigured", "Viber credentials are not configured.");
        }

        if (string.IsNullOrWhiteSpace(request.Sender) || string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            return Fail("ViberSession.Invalid", "Sender and phone number are required.");
        }

        string phone = ViberPhone.Normalize(request.PhoneNumber);
        string sender = request.Sender.Trim();
        CompanyId companyId = CompanyId.Create(request.CompanyId);
        DateTime now = DateTime.UtcNow;

        ViberSession? session = await sessions.FindLatestAsync(companyId, sender, phone, cancellationToken);
        bool open = session?.IsOpen(now) == true;

        if (!open && !request.AllowOutsideSession)
        {
            return Fail(
                "ViberSession.Closed",
                "There is no open Viber session for this phone. Sending now would be billed as a promotional message.");
        }

        if (open && session!.ConsecutiveOutbound >= ViberSession.MaxConsecutive)
        {
            return Fail("ViberSession.ConsecutiveLimit", "Viber allows at most 10 session messages in a row. Wait for a reply.");
        }

        if (open && session!.TotalOutbound >= ViberSession.MaxOutbound)
        {
            return Fail("ViberSession.TotalLimit", "This Viber session already has 60 outgoing messages.");
        }

        ViberSendMessageType comtradeType;
        string? body = string.IsNullOrWhiteSpace(request.Text) ? null : request.Text.Trim();
        string? media = string.IsNullOrWhiteSpace(request.Media) ? null : request.Media.Trim();
        try
        {
            comtradeType = ResolveType(request.Kind, body, media, request.QuickRepliesJson);
        }
        catch (InvalidOperationException ex)
        {
            return Fail("ViberSession.Invalid", ex.Message);
        }

        if (session is null)
        {
            session = new ViberSession(companyId, sender, phone, now);
            sessions.Add(session);
        }

        bool billed = !open;
        ViberSessionMessage outgoing = new(
            session.Id,
            ViberSessionDirection.Out,
            (int)comtradeType,
            body,
            media,
            request.QuickRepliesJson,
            billed);
        sessions.AddMessage(outgoing);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        string uploadsBase = string.IsNullOrWhiteSpace(uploads.Value.BaseUrl)
            ? "https://api.echo.oneclick.rs/uploads"
            : uploads.Value.BaseUrl.Trim().TrimEnd('/');

        ViberMessage message = Build(comtradeType, sender, phone, outgoing.ViberMessageId, body, media, request.QuickRepliesJson, request.SurveyUiType, uploadsBase);

        SendViberMessageResponseDto? response = await viberService.Send(new SendViberMessageDto
        {
            UserCredentials = new ViberUserCredentials { UserName = account.Username, Password = account.Password },
            ViberMessages = [message]
        }, retryCountParam: 2);

        SendViberMessageResponse? row = response?.ViberMessageResponses.FirstOrDefault(r => r.MessageId == outgoing.ViberMessageId)
            ?? response?.ViberMessageResponses.FirstOrDefault();

        if (row is null || row.Status != ViberMessageResponseStatus.MSG_SUCCESS)
        {
            outgoing.SendError = row is null ? "Comtrade did not return a send status." : row.Status.ToString();
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Fail("ViberSession.ProviderError", outgoing.SendError);
        }

        outgoing.IsSent = true;
        if (open)
        {
            session.TotalOutbound += 1;
            session.ConsecutiveOutbound += 1;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return await MapAsync(session, cancellationToken);
    }

    private async Task<ViberSessionResponse> MapAsync(ViberSession session, CancellationToken cancellationToken)
    {
        List<ViberSessionMessage> messages = await sessions.ListMessagesAsync(session.Id, cancellationToken);
        return ViberSessionMapper.Map(session, messages, DateTime.UtcNow);
    }

    private static Result<ViberSessionResponse> Fail(string code, string message) =>
        Result.Failure<ViberSessionResponse>(new Error(code, message));

    private static ViberSendMessageType ResolveType(ViberSessionContentKind kind, string? body, string? media, string? quickReplies)
    {
        switch (kind)
        {
            case ViberSessionContentKind.Text:
                if (string.IsNullOrWhiteSpace(body))
                {
                    throw new InvalidOperationException("Session text is required.");
                }

                return ViberSendMessageType.SessionText;
            case ViberSessionContentKind.Image:
                if (string.IsNullOrWhiteSpace(media) || !IsImage(media))
                {
                    throw new InvalidOperationException("Session image must be a jpg, png, or gif URL.");
                }

                return ViberSendMessageType.SessionImage;
            case ViberSessionContentKind.File:
                if (string.IsNullOrWhiteSpace(media) || !MediaHelper.TryGetViberDocumentFileType(media, out _))
                {
                    throw new InvalidOperationException("Session file must use an allowed document extension.");
                }

                return ViberSendMessageType.SessionFile;
            case ViberSessionContentKind.QuickReplies:
                if (string.IsNullOrWhiteSpace(body))
                {
                    throw new InvalidOperationException("Quick replies require intro text.");
                }

                ViberSurveyOptionsHelper.ParseRequired(quickReplies);
                return ViberSendMessageType.SessionQuickReplies;
            default:
                throw new InvalidOperationException("Unsupported session content.");
        }
    }

    private static ViberMessage Build(
        ViberSendMessageType type,
        string sender,
        string phone,
        long messageId,
        string? body,
        string? media,
        string? quickReplies,
        short? surveyUi,
        string uploadsBase)
    {
        ViberMessage message = new()
        {
            Display = sender,
            Label = "promotion",
            MSISDN = phone,
            MessageId = messageId,
            MessageType = type,
            Priority = 255,
            Tag = "session",
            Validity = 86400
        };

        switch (type)
        {
            case ViberSendMessageType.SessionText:
                message.MessageText = body;
                break;
            case ViberSendMessageType.SessionImage:
                message.ImageUrl = Absolute(uploadsBase, media!);
                message.ImageFormat = ViberImageFields.ResolveFormat(media, null);
                break;
            case ViberSendMessageType.SessionFile:
                MediaHelper.TryGetViberDocumentFileType(media!, out string fileType);
                message.ButtonUrl = Absolute(uploadsBase, media!);
                message.FileType = fileType;
                message.NameOfFile = Path.GetFileName(message.ButtonUrl.Split('?')[0]);
                break;
            case ViberSendMessageType.SessionQuickReplies:
                message.MessageText = body;
                message.Survey = new ViberSurveyPayload
                {
                    Options = ViberSurveyOptionsHelper.ParseRequired(quickReplies)
                };
                if (surveyUi is 1 or 2)
                {
                    message.Survey.ListMessageUiType = surveyUi.Value;
                }

                break;
        }

        return message;
    }

    private static string Absolute(string uploadsBase, string media) =>
        media.Contains("://", StringComparison.Ordinal)
            ? media
            : $"{uploadsBase.TrimEnd('/')}/{media.TrimStart('/')}";

    private static bool IsImage(string media)
    {
        string p = media.Split('?')[0];
        return p.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
            || p.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
            || p.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
            || p.EndsWith(".gif", StringComparison.OrdinalIgnoreCase);
    }
}
