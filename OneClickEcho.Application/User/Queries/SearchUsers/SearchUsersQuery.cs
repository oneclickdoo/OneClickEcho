using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Application.User.Queries.GetUsers;

namespace OneClickEcho.Application.User.Queries.SearchUsers
{
    public sealed record SearchUsersQuery(
        Guid CompanyId,
        string Email
        ) : IQuery<List<GetUserDto>>;
}
