using FluentValidation;

namespace OneClickEcho.Application.Company.Commands.DeleteCompany;

public sealed class DeleteCompanyValidator : AbstractValidator<DeleteCompanyCommand>
{
    public DeleteCompanyValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("CompanyId must not be empty.");
    }
}

