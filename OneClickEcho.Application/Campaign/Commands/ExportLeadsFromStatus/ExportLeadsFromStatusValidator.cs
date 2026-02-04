using FluentValidation;

namespace OneClickEcho.Application.Campaign.Commands.ExportLeadsFromStatus;

public class ExportLeadsFromStatusValidator : AbstractValidator<ExportLeadsFromStatusCommand>
{
    public ExportLeadsFromStatusValidator()
    {
        RuleFor(x => x.CampaignId)
            .NotEmpty()
            .WithMessage("CampaignId is required.");
    }
}