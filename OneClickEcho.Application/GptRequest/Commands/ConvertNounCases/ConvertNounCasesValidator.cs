using FluentValidation;

namespace OneClickEcho.Application.GptRequest.Commands.ConvertNounCases;

public class ConvertNounCasesValidator : AbstractValidator<ConvertNounCasesCommand>
{
    public ConvertNounCasesValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required.");
    }
}