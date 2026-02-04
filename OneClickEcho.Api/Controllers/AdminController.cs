using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OneClickEcho.App.Abstractions;
using OneClickEcho.Application.Admin.Queries.GetAdminAnalytics;
using OneClickEcho.Domain.Common.Shared;
using OpenIddict.Validation.AspNetCore;

namespace OneClickEcho.App.Controllers
{
    [Route("api/Admin")]
    public class AdminController(IMediator mediator) : ApiController(mediator)
    {
        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [HttpGet("Analytics")]
        public async Task<IActionResult> GetAnalytics([FromQuery] string? startDate, [FromQuery] string? endDate, CancellationToken cancellationToken)
        {
            GetAdminAnalyticsQuery query = new(startDate, endDate);

            Result<GetAdminAnalyticsResponse> response = await Mediator.Send(query, cancellationToken);

            return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
        }
    }
}
