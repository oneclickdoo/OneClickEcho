using FluentValidation;

namespace OneClickEcho.Application.Sender.Queries.GetSenders
{
    public class GetSendersValidator : AbstractValidator<GetSendersQuery>
    {
        public GetSendersValidator()
        {
            RuleFor(query => query.CompanyId)
                .NotEmpty()
                .WithMessage("CompanyId must not be empty.");
        }
    }
}
