using FluentValidation;

namespace OneClickEcho.Application.GptRequest.Commands.EnhanceCampaignMessage;

public class EnhanceCampaignMessageValidator : AbstractValidator<EnhanceCampaignMessageCommand>
{
    public EnhanceCampaignMessageValidator()
    {
        RuleFor(x => x.CampaignId)
            .NotEmpty()
            .WithMessage("CampaignID is required.");

        RuleFor(x => x.RequestMessage)
            .NotEmpty()
            .WithMessage("RequestMessage is required.");
    }
}