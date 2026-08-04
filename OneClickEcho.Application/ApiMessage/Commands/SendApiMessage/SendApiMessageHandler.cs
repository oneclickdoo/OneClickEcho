using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Application.Common.Viber;
using OneClickEcho.Domain.ApiMessageAggregate.Repositories;
using OneClickEcho.Domain.CampaignLeadAggregate.Enums;
using OneClickEcho.Domain.Common.Repositories;
using OneClickEcho.Domain.Common.Shared;
using OneClickEcho.Domain.CompanyAggregate.Repositories;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;

namespace OneClickEcho.Application.ApiMessage.Commands.SendApiMessage;

public class SendApiMessageHandler(IApiMessageRepository apiMessageRepository, ICompanyRepository companyRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<SendApiMessageCommand, SendApiMessageResponse>
{
    public static readonly Error DuplicateSameDay = new(
        "ApiMessage.DuplicateSameDay",
        "An identical message to this phone number was already accepted today for this company.");

    public async Task<Result<SendApiMessageResponse>> Handle(SendApiMessageCommand request,
        CancellationToken cancellationToken)
    {
        var companyId = CompanyId.Create(request.CompanyId);
        if (!await companyRepository.CompanyApiValidation(companyId, request.ApiPassword, cancellationToken))
        {
            return Result.Failure<SendApiMessageResponse>(new Error(
                "Company.InvalidApiPassword",
                "The company api password is invalid."
            ));
        }

        (DateTime dayStartUtc, DateTime dayEndUtc) = GetLocalCalendarDayUtcRange(DateTime.UtcNow);

        Domain.ApiMessageAggregate.ApiMessage? duplicate = await apiMessageRepository.FindIdenticalSameDayAsync(
            companyId,
            request.PhoneNumber,
            request.Message,
            request.ApiMessageType,
            request.Sender,
            request.ViberMedia,
            request.ViberButtonUrl,
            request.ViberButtonUrlTitle,
            dayStartUtc,
            dayEndUtc,
            cancellationToken);

        if (duplicate is not null)
        {
            return Result.Failure<SendApiMessageResponse>(new Error(
                DuplicateSameDay.Code,
                $"{DuplicateSameDay.Message} ExistingId:{duplicate.Id.Value}"));
        }

        Domain.ApiMessageAggregate.ApiMessage apiMessage = new(
            companyId,
            request.PhoneNumber.Trim(),
            request.Message.Trim(),
            request.ApiMessageType,
            request.HasSmsFallback,
            request.Sender,
            request.ViberMedia,
            request.ViberButtonUrl,
            request.ViberButtonUrlTitle,
            request.SmsMessage,
            request.SmsSender,
            request.ViberValidity,
            viberVideoThumbnail: request.ViberVideoThumbnail,
            viberFileSize: request.ViberFileSize,
            viberVideoDuration: request.ViberVideoDuration
        );

        apiMessage.ViberStatus = CampaignLeadViberStatus.None;
        apiMessage.ViberStatusDescription = CampaignLeadViberStatusDescriptions.ForQueued();

        apiMessageRepository.Add(apiMessage);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new SendApiMessageResponse(
            Id: apiMessage.Id.Value,
            IsSent: apiMessage.IsSent,
            ViberStatus: (short)apiMessage.ViberStatus,
            ViberStatusDescription: apiMessage.ViberStatusDescription,
            SmsStatus: (short)apiMessage.SMSStatus,
            SmsStatusDescription: apiMessage.SMSStatusDescription,
            PhoneNumber: apiMessage.PhoneNumber,
            CreatedAt: apiMessage.CreatedAt);
    }

    /// <summary>Calendar day in Europe/Belgrade (falls back to Windows CET id).</summary>
    private static (DateTime DayStartUtc, DateTime DayEndUtc) GetLocalCalendarDayUtcRange(DateTime utcNow)
    {
        TimeZoneInfo tz = ResolveBelgradeTimeZone();
        DateTime localNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utcNow, DateTimeKind.Utc), tz);
        DateTime localDayStart = localNow.Date;
        DateTime dayStartUtc = TimeZoneInfo.ConvertTimeToUtc(localDayStart, tz);
        DateTime dayEndUtc = TimeZoneInfo.ConvertTimeToUtc(localDayStart.AddDays(1), tz);
        return (dayStartUtc, dayEndUtc);
    }

    private static TimeZoneInfo ResolveBelgradeTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Europe/Belgrade");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
        }
    }
}
