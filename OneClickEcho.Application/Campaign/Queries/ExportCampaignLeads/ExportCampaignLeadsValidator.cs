using FluentValidation;

namespace OneClickEcho.Application.Campaign.Queries.ExportCampaignLeads
{
    public class ExportCampaignLeadsValidator : AbstractValidator<ExportCampaignLeadsQuery>
    {
        public ExportCampaignLeadsValidator()
        {
            RuleFor(query => query.CampaignId)
                .NotEmpty()
                .WithMessage("CampaignId must not be empty.");
        }
    }
}
