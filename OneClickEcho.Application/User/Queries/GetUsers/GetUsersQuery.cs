using OneClickEcho.Application.Common.Abstractions;
using OneClickEcho.Application.Common.Messaging;

namespace OneClickEcho.Application.User.Queries.GetUsers;

public class GetUsersQuery : BasePagedQuery, IQuery<GetUsersResponse>;