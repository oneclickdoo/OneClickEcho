using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OneClickEcho.App.Abstractions;
using OneClickEcho.App.Abstractions.Queries;
using OneClickEcho.Application.User.Commands.AssignUser;
using OneClickEcho.Application.User.Commands.CreateUser;
using OneClickEcho.Application.User.Commands.DeleteUser;
using OneClickEcho.Application.User.Commands.ForceResetPassword;
using OneClickEcho.Application.User.Commands.ResetPassword;
using OneClickEcho.Application.User.Queries.GetCurrentUser;
using OneClickEcho.Application.User.Queries.GetUsers;
using OneClickEcho.Application.User.Queries.SearchUsers;
using OneClickEcho.Domain.Common.Shared;
using OpenIddict.Validation.AspNetCore;
using System.Security.Claims;

namespace OneClickEcho.App.Controllers
{
    [Route("api/User")]
    public class UserController(IMediator mediator) : ApiController(mediator)
    {
        [Authorize(Roles = "Administrator")]
        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand request, CancellationToken cancellationToken)
        {
            Result<CreateUserResponse> response = await Mediator.Send(request, cancellationToken);

            return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
        }

        [Authorize(Roles = "Administrator")]
        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [HttpPost("Assign")]
        public async Task<IActionResult> AssignUser([FromBody] AssignUserCommand request, CancellationToken cancellationToken)
        {
            Result<AssignUserResponse> response = await Mediator.Send(request, cancellationToken);

            return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
        }

        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [HttpGet("CurrentUser")]
        public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
        {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8604 // Possible null reference argument.
            GetCurrentUserQuery query = new(User.Identity.Name, GetUserRoles(HttpContext.User), GetUserCompanies(HttpContext.User));
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8602 // Dereference of a possibly null reference.

            Result<GetCurrentUserResponse> response = await Mediator.Send(query, cancellationToken);

            return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
        }

        [Authorize(Roles = "Administrator")]
        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] PagedQueryParams pagedQueryParams, CancellationToken cancellationToken)
        {
            GetUsersQuery query = pagedQueryParams.ConvertToBasePagedQuery<GetUsersQuery>();

            Result<GetUsersResponse> response = await Mediator.Send(query, cancellationToken);

            return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
        }

        [Authorize(Roles = "Administrator")]
        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [HttpGet("Search")]
        public async Task<IActionResult> SearchUsers([FromQuery] SearchUsersQuery query, CancellationToken cancellationToken)
        {
            Result<List<GetUserDto>> response = await Mediator.Send(query, cancellationToken);

            return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
        }

        [Authorize(Roles = "Administrator")]
        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [HttpPost("ForcePasswordUpdate")]
        public async Task<IActionResult> ForcePasswordUpdate([FromBody] ForceResetPasswordCommand request, CancellationToken cancellationToken)
        {
            Result<ForceResetPasswordResponse> response = await Mediator.Send(request, cancellationToken);

            return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
        }

        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [HttpPost("PasswordUpdate")]
        public async Task<IActionResult> PasswordUpdate([FromBody] ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            Result<ResetPasswordResponse> response = await Mediator.Send(request, cancellationToken);

            return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
        }

        [Authorize(Roles = "Administrator")]
        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteLeadCampaign(Guid userId, CancellationToken cancellationToken)
        {
            DeleteUserCommand command = new(userId);

            Result<DeleteUserResponse> response = await Mediator.Send(command, cancellationToken);

            return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
        }

        #region HelperFunction

        private ICollection<Guid> GetUserCompanies(ClaimsPrincipal user)
        {
            string? stringCompanyIds = User.Claims.FirstOrDefault(c => c.Type == "CompanyIds")?.Value;

            if (string.IsNullOrEmpty(stringCompanyIds))
            {
                return [];
            }

            ICollection<Guid>? companyIds = JsonConvert.DeserializeObject<ICollection<Guid>>(stringCompanyIds);

            return companyIds ?? [];
        }

        private static List<string> GetUserRoles(ClaimsPrincipal user)
        {
            return user.Claims
                .Where(c => c.Type == "role")
                .Select(c => c.Value)
                .ToList();
        }

        #endregion
    }
}