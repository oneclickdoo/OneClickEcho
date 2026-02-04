using FluentValidation;

namespace OneClickEcho.Application.Common.Abstractions;

public class BasePagedValidator<T> : AbstractValidator<T> where T : BasePagedQuery
{
    public BasePagedValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("PageNumber must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("PageSize must be greater than 0.")
            .LessThanOrEqualTo(100)
            .WithMessage("PageSize must be less than or equal to 100.");
    }
}