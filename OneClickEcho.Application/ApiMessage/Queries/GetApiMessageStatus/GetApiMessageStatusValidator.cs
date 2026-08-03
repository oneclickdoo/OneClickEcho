using FluentValidation;

namespace OneClickEcho.Application.ApiMessage.Queries.GetApiMessageStatus;

public sealed class GetApiMessageStatusValidator : AbstractValidator<GetApiMessageStatusQuery>
{
    public GetApiMessageStatusValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Id must not be empty.");

        RuleFor(x => x.CompanyId)
            .NotEmpty()
            .WithMessage("CompanyId must not be empty.");

        RuleFor(x => x.ApiPassword)
            .NotEmpty()
            .WithMessage("ApiPassword must not be empty.");
    }
}
