using FluentValidation;

namespace OneClickEcho.Application.Lead.Commands.DeleteLead
{
    public sealed class DeleteLeadValidator : AbstractValidator<DeleteLeadCommand>
    {
        public DeleteLeadValidator()
        {
            RuleFor(x => x.LeadId)
                .NotEmpty()
                .WithMessage("LeadId is required.");
        }
    }
}
