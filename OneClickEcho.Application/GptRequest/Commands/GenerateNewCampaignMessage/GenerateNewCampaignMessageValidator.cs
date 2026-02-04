using FluentValidation;

namespace OneClickEcho.Application.GptRequest.Commands.GenerateNewCampaignMessage;

public class GenerateNewCampaignMessageValidator : AbstractValidator<GenerateNewCampaignMessageCommand>
{
    public GenerateNewCampaignMessageValidator()
    {
        RuleFor(x => x.CampaignId)
            .NotEmpty()
            .WithMessage("CampaignID is required.");

        RuleFor(x => x.RequestMessage)
            .NotEmpty()
            .WithMessage("RequestMessage is required.");
    }
}