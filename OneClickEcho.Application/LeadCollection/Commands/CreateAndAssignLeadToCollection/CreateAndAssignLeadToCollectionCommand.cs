using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.LeadAggregate.Enums;

namespace OneClickEcho.Application.LeadCollection.Commands.CreateAndAssignLeadToCollection
{
    public sealed record CreateAndAssignLeadToCollectionCommand(
        Guid CompanyId,
        Guid LeadCollectionId,
        string PhoneNumber,
        string? FirstName,
        string? LastName,
        LeadGender? Gender,
        string? Email,
        DateOnly? DateOfBirth,
        string? City,
        string? State,
        string? Country
        ) : ICommand<CreateAndAssignLeadToCollectionResponse>;
}
