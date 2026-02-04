using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.Common.Identity;
using OneClickEcho.Domain.Common.Shared;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;

namespace OneClickEcho.Application.User.Commands.AssignUser
{
    public class AssignUserHandler(IUserManager userManager) : ICommandHandler<AssignUserCommand, AssignUserResponse>
    {
        private readonly IUserManager _userManager = userManager;

        public async Task<Result<AssignUserResponse>> Handle(AssignUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await _userManager.AssignUser(request.UserId, CompanyId.Create(request.CompanyId), cancellationToken);
            }
            catch
            {
                return Result.Failure<AssignUserResponse>(new Error(
                    "Request.Failed",
                    "Password failed to reset."
                    ));
            }

            return new AssignUserResponse();
        }
    }
}
