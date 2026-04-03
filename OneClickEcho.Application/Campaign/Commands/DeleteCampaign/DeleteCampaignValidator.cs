using FluentValidation;

namespace OneClickEcho.Application.Campaign.Commands.DeleteCampaign;

public sealed class DeleteCampaignValidator : AbstractValidator<DeleteCampaignCommand>
{
    public DeleteCampaignValidator()
    {
        RuleFor(x => x.CampaignId)
            .NotEmpty()
            .WithMessage("CampaignId is required.");
    }
}