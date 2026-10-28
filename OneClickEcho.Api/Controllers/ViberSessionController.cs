using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OneClickEcho.App.Abstractions;
using OneClickEcho.App.Infrastructure.Utils;
using OneClickEcho.Application.ViberSession;
using OneClickEcho.Domain.Common.Shared;
using OpenIddict.Validation.AspNetCore;

namespace OneClickEcho.App.Controllers;

[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
[Route("api/app/viber-sessions")]
public class ViberSessionController(IMediator mediator) : ApiController(mediator)
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        if (!CampaignTenantFilter.UserMayAccessCampaignCompany(User, companyId))
        {
            return Forbid();
        }

        Result<List<ViberSessionResponse>> response = await Mediator.Send(new ListViberSessionsQuery(companyId), cancellationToken);
        return response.IsSuccess ? Ok(response.Value) : BadRequest(response.Error);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get([FromRoute] Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        if (!CampaignTenantFilter.UserMayAccessCampaignCompany(User, companyId))
        {
            return Forbid();
        }

        Result<ViberSessionResponse> response = await Mediator.Send(new GetViberSessionQuery(companyId, id), cancellationToken);
        return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
    }

    [HttpPost("send")]
    public async Task<IActionResult> Send([FromBody] SendViberSessionCommand command, CancellationToken cancellationToken)
    {
        if (!CampaignTenantFilter.UserMayAccessCampaignCompany(User, command.CompanyId))
        {
            return Forbid();
        }

        Result<ViberSessionResponse> response = await Mediator.Send(command, cancellationToken);
        if (response.IsSuccess)
        {
            return Ok(response.Value);
        }

        return response.Error.Code == "ViberSession.Closed" ? Conflict(response.Error) : BadRequest(response.Error);
    }
}
