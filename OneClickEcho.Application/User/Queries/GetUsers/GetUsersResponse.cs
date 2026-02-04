using OneClickEcho.Application.Common.Abstractions;
using OneClickEcho.Domain.ApplicationUserAggregate;
using OneClickEcho.Domain.Common.Queries;

namespace OneClickEcho.Application.User.Queries.GetUsers;

public class GetUsersResponse(IPagedList<ApplicationUser> items)
    : PagedListDto<ApplicationUser, GetUserDto>(items)
{
    public override List<GetUserDto> ConvertTToTDto(List<ApplicationUser> items)
    {
        List<GetUserDto> result = [];

        foreach (ApplicationUser user in items)
        {
            try
            {
                if (user.Email == null)
                {
                    continue;
                }

                result.Add(new GetUserDto(
                    Id: user.Id,
                    Email: user.Email
                ));
            }
            catch { /* Continue. This case should be impossible */ }
        }

        return result;
    }
}

public record GetUserDto(
    Guid Id,
    string Email
);