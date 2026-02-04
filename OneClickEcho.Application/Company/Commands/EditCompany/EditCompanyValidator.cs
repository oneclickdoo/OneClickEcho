using FluentValidation;

namespace OneClickEcho.Application.Company.Commands.EditCompany;

public sealed class EditCompanyValidator : AbstractValidator<EditCompanyCommand>
{
    public EditCompanyValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Company name is required.")
            .Length(2, 100)
            .WithMessage("Company name must be between 2 and 100 characters.");
    }
}

