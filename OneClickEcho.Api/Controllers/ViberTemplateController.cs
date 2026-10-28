using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OneClickEcho.App.Abstractions;
using OneClickEcho.App.Infrastructure.Utils;
using OneClickEcho.Application.ViberTemplate;
using OneClickEcho.Domain.Common.Shared;
using OpenIddict.Validation.AspNetCore;

namespace OneClickEcho.App.Controllers;

[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
[Route("api/app/viber-templates")]
public class ViberTemplateController(IMediator mediator) : ApiController(mediator)
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        if (!CampaignTenantFilter.UserMayAccessCampaignCompany(User, companyId))
        {
            return Forbid();
        }

        Result<List<ViberTemplateResponse>> response = await Mediator.Send(new ListViberTemplatesQuery(companyId), cancellationToken);
        return response.IsSuccess ? Ok(response.Value) : BadRequest(response.Error);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateViberTemplateCommand command, CancellationToken cancellationToken)
    {
        if (!CampaignTenantFilter.UserMayAccessCampaignCompany(User, command.CompanyId))
        {
            return Forbid();
        }

        Result<ViberTemplateResponse> response = await Mediator.Send(command, cancellationToken);
        return response.IsSuccess ? Ok(response.Value) : BadRequest(response.Error);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        if (!CampaignTenantFilter.UserMayAccessCampaignCompany(User, companyId))
        {
            return Forbid();
        }

        Result response = await Mediator.Send(new DeleteViberTemplateCommand(companyId, id), cancellationToken);
        return response.IsSuccess ? Ok() : BadRequest(response.Error);
    }

    [HttpPost("{id:guid}/refresh")]
    public async Task<IActionResult> Refresh([FromRoute] Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        if (!CampaignTenantFilter.UserMayAccessCampaignCompany(User, companyId))
        {
            return Forbid();
        }

        Result<ViberTemplateResponse> response = await Mediator.Send(new RefreshViberTemplateCommand(companyId, id), cancellationToken);
        return response.IsSuccess ? Ok(response.Value) : BadRequest(response.Error);
    }
}
