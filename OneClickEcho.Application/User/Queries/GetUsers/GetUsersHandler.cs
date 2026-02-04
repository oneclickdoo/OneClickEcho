using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.ApplicationUserAggregate;
using OneClickEcho.Domain.ApplicationUserAggregate.Repositories;
using OneClickEcho.Domain.Common.Queries;
using OneClickEcho.Domain.Common.Shared;

namespace OneClickEcho.Application.User.Queries.GetUsers;

public class GetUsersHandler(IApplicationUserRepository applicationUserRepository)
    : IQueryHandler<GetUsersQuery, GetUsersResponse>
{
    private readonly IApplicationUserRepository _applicationUserRepository = applicationUserRepository;

    public async Task<Result<GetUsersResponse>> Handle(GetUsersQuery query, CancellationToken cancellationToken)
    {
        IPagedList<ApplicationUser> applicationUsers = await _applicationUserRepository.GetAllAsync(query, cancellationToken);

        return new GetUsersResponse(applicationUsers);
    }
}