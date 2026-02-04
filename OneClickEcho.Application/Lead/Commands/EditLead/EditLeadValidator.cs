using FluentValidation;
using OneClickEcho.Application.Common.Helpers;

namespace OneClickEcho.Application.Lead.Commands.EditLead;

public class EditLeadValidator : AbstractValidator<EditLeadCommand>
{
    public EditLeadValidator()
    {
        RuleFor(x => x.LeadId)
            .NotEmpty()
            .WithMessage("LeadId is required.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithMessage("Phone number is required.")
            .Matches(RegexHelper.PHONE_NUMBER_REGEX)
            .WithMessage("Invalid phone number format.");

        RuleFor(x => x.Email)
            .EmailAddress()
            .WithMessage("Email is not valid.")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.FirstName)
            .MinimumLength(2)
            .WithMessage("FirstName must be at least 2 characters long.")
            .When(x => !string.IsNullOrEmpty(x.FirstName));

        RuleFor(x => x.LastName)
            .MinimumLength(2)
            .WithMessage("LastName must be at least 2 characters long.")
            .When(x => !string.IsNullOrEmpty(x.LastName));

        RuleFor(x => x.Gender)
            .IsInEnum()
            .WithMessage("Gender must be a valid enum value.")
            .When(x => x.Gender.HasValue);

        RuleFor(x => x.DateOfBirth)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("DateOfBirth must be in the past.")
            .When(x => x.DateOfBirth.HasValue);

        RuleFor(x => x.City)
            .MaximumLength(100)
            .WithMessage("City must be less than 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.City));

        RuleFor(x => x.State)
            .MaximumLength(100)
            .WithMessage("State must be less than 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.State));

        RuleFor(x => x.Country)
            .MaximumLength(100)
            .WithMessage("Country must be less than 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.Country));
    }
}