using FluentValidation;

namespace OneClickEcho.Application.Campaign.Commands.TestCampaign
{
    public class TestCampaignValidator : AbstractValidator<TestCampaignCommand>
    {
        public TestCampaignValidator()
        {
            RuleFor(x => x.CampaignId)
                .NotEmpty()
                .WithMessage("CampaignId is required.");
        
            RuleFor(x => x.PhoneNumber)
                .NotEmpty()
                .WithMessage("PhoneNumber is required.")
                .Matches(@"^\+?[1-9]\d{1,14}$")
                .WithMessage("PhoneNumber must be a valid international phone number.");
        }
    }
}
