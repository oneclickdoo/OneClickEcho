using OneClickEcho.Application.Common.Messaging;

namespace OneClickEcho.Application.User.Queries.GetCurrentUser;

public record GetCurrentUserQuery(
    string Email,
    ICollection<string> Roles,
    ICollection<Guid> CompanyIds
) : IQuery<GetCurrentUserResponse>;