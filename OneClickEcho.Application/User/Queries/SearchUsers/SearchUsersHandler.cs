using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Application.User.Queries.GetUsers;
using OneClickEcho.Domain.ApplicationUserAggregate;
using OneClickEcho.Domain.ApplicationUserAggregate.Repositories;
using OneClickEcho.Domain.Common.Shared;

namespace OneClickEcho.Application.User.Queries.SearchUsers
{
    public class SearchUsersHandler(IApplicationUserRepository applicationUserRepository)
        : IQueryHandler<SearchUsersQuery, List<GetUserDto>>
    {
        private readonly IApplicationUserRepository _applicationUserRepository = applicationUserRepository;

        public async Task<Result<List<GetUserDto>>> Handle(SearchUsersQuery request, CancellationToken cancellationToken)
        {
            List<ApplicationUser> applicationUsers = await _applicationUserRepository
                .Search(request.CompanyId, request.Email, cancellationToken);

#pragma warning disable CS8604 // Possible null reference argument.
            return applicationUsers.Select(user => new GetUserDto(user.Id, user.Email)).ToList();
#pragma warning restore CS8604 // Possible null reference argument.
        }
    }
}
