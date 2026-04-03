using FluentValidation;
using OneClickEcho.Application.Common.Helpers;

namespace OneClickEcho.Application.LeadCollection.Commands.CreateAndAssignLeadToCollection
{
    public class CreateAndAssignLeadToCollectionValidator : AbstractValidator<CreateAndAssignLeadToCollectionCommand>
    {
        public CreateAndAssignLeadToCollectionValidator()
        {
            RuleFor(x => x.PhoneNumber)
                .NotEmpty()
                .WithMessage("Phone number is required.")
                .Matches(RegexHelper.PHONE_NUMBER_REGEX)
                .WithMessage("Invalid phone number format.");

            RuleFor(x => x.FirstName)
                .MaximumLength(50)
                .WithMessage("First name cannot exceed 50 characters.");

            RuleFor(x => x.LastName)
                .MaximumLength(50)
                .WithMessage("Last name cannot exceed 50 characters.");

            RuleFor(x => x.Email)
                .EmailAddress()
                .WithMessage("A valid email address is required.");

            RuleFor(x => x.DateOfBirth)
                .LessThan(DateOnly.FromDateTime(DateTime.Now))
                .WithMessage("Date of birth must be in the past.");

            RuleFor(x => x.City)
                .MaximumLength(100)
                .WithMessage("City cannot exceed 100 characters.");

            RuleFor(x => x.State)
                .MaximumLength(100)
                .WithMessage("State cannot exceed 100 characters.");

            RuleFor(x => x.Country)
                .MaximumLength(100)
                .WithMessage("Country cannot exceed 100 characters.");

            RuleFor(x => x.LeadCollectionId)
                .NotEmpty()
                .WithMessage("LeadCollectionId is required.")
                .Must(id => id != Guid.Empty)
                .WithMessage("LeadCollectionId must be a valid GUID.");
        }
    }
}
