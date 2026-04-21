using FluentValidation;
using OneClickEcho.Application.Common.Abstractions;
using OneClickEcho.Domain.CampaignLeadAggregate.Enums;

namespace OneClickEcho.Application.Campaign.Queries.GetCampaignLeadReport;

public sealed class GetCampaignLeadReportValidator : BasePagedValidator<GetCampaignLeadReportQuery>
{
    public GetCampaignLeadReportValidator()
    {
        RuleFor(x => x.CampaignId)
            .NotEmpty()
            .WithMessage("CampaignId is required.");

        When(x => x.ViberStatus.HasValue, () =>
        {
            RuleFor(x => x.ViberStatus!.Value)
                .Must(v => Enum.IsDefined(typeof(CampaignLeadViberStatus), v))
                .WithMessage("ViberStatus is not a valid value.");
        });

        When(x => x.SmsStatus.HasValue, () =>
        {
            RuleFor(x => x.SmsStatus!.Value)
                .Must(v => Enum.IsDefined(typeof(CampaignLeadSMSStatus), v))
                .WithMessage("SmsStatus is not a valid value.");
        });
    }
}
