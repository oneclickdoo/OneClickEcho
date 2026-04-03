using FluentValidation;

namespace OneClickEcho.Application.Company.Commands.CreateCompany;

public sealed class CreateCompanyValidator : AbstractValidator<CreateCompanyCommand>
{
    public CreateCompanyValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Company name is required.")
            .Length(2, 100)
            .WithMessage("Company name must be between 2 and 100 characters.");
    }
}

