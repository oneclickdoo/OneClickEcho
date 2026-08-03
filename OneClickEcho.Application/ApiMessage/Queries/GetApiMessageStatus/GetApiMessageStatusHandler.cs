using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Application.Common.Viber;
using OneClickEcho.Domain.ApiMessageAggregate.Repositories;
using OneClickEcho.Domain.ApiMessageAggregate.ValueObjects;
using OneClickEcho.Domain.Common.Shared;
using OneClickEcho.Domain.CompanyAggregate.Repositories;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;

namespace OneClickEcho.Application.ApiMessage.Queries.GetApiMessageStatus;

public sealed class GetApiMessageStatusHandler(
    IApiMessageRepository apiMessageRepository,
    ICompanyRepository companyRepository)
    : IQueryHandler<GetApiMessageStatusQuery, GetApiMessageStatusResponse>
{
    public async Task<Result<GetApiMessageStatusResponse>> Handle(
        GetApiMessageStatusQuery request,
        CancellationToken cancellationToken)
    {
        CompanyId companyId = CompanyId.Create(request.CompanyId);
        if (!await companyRepository.CompanyApiValidation(companyId, request.ApiPassword, cancellationToken))
        {
            return Result.Failure<GetApiMessageStatusResponse>(new Error(
                "Company.InvalidApiPassword",
                "The company api password is invalid."));
        }

        Domain.ApiMessageAggregate.ApiMessage? apiMessage = await apiMessageRepository
            .GetByIdAsync(ApiMessageId.Create(request.Id), cancellationToken);

        if (apiMessage is null || apiMessage.CompanyId != companyId)
        {
            return Result.Failure<GetApiMessageStatusResponse>(new Error(
                "ApiMessage.NotFound",
                $"The API message with Id:\"{request.Id}\" does not exist."));
        }

        string? viberDescription = string.IsNullOrWhiteSpace(apiMessage.ViberStatusDescription)
            ? CampaignLeadViberStatusDescriptions.ForStatus(apiMessage.ViberStatus)
            : apiMessage.ViberStatusDescription;

        return new GetApiMessageStatusResponse(
            Id: apiMessage.Id.Value,
            IsSent: apiMessage.IsSent,
            ViberStatus: (short)apiMessage.ViberStatus,
            ViberStatusDescription: viberDescription,
            SmsStatus: (short)apiMessage.SMSStatus,
            SmsStatusDescription: apiMessage.SMSStatusDescription,
            PhoneNumber: apiMessage.PhoneNumber,
            ViberMessageId: apiMessage.ViberMessageId,
            CreatedAt: apiMessage.CreatedAt);
    }
}
