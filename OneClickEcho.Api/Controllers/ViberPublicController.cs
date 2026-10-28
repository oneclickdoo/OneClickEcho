using MediatR;
using Microsoft.AspNetCore.Mvc;
using OneClickEcho.App.Abstractions;
using OneClickEcho.Application.ViberSession;
using OneClickEcho.Application.ViberTemplate;
using OneClickEcho.Domain.Common.Shared;

namespace OneClickEcho.App.Controllers;

[Route("api/viber")]
public class ViberPublicController(IMediator mediator) : ApiController(mediator)
{
    [HttpPost("templates"), IgnoreAntiforgeryToken]
    public async Task<IActionResult> CreateTemplate([FromBody] CreateViberTemplateApiCommand command, CancellationToken cancellationToken)
    {
        Result<ViberTemplateResponse> response = await Mediator.Send(command, cancellationToken);
        return Map(response);
    }

    [HttpGet("templates"), IgnoreAntiforgeryToken]
    public async Task<IActionResult> ListTemplates([FromQuery] Guid companyId, [FromQuery] string apiPassword, CancellationToken cancellationToken)
    {
        Result<List<ViberTemplateResponse>> response = await Mediator.Send(new ListViberTemplatesApiQuery(companyId, apiPassword ?? string.Empty), cancellationToken);
        return Map(response);
    }

    [HttpDelete("templates/{id:guid}"), IgnoreAntiforgeryToken]
    public async Task<IActionResult> DeleteTemplate([FromRoute] Guid id, [FromQuery] Guid companyId, [FromQuery] string apiPassword, CancellationToken cancellationToken)
    {
        Result response = await Mediator.Send(new DeleteViberTemplateApiCommand(companyId, apiPassword ?? string.Empty, id), cancellationToken);
        return Map(response);
    }

    [HttpPost("templates/{id:guid}/refresh"), IgnoreAntiforgeryToken]
    public async Task<IActionResult> RefreshTemplate([FromRoute] Guid id, [FromQuery] Guid companyId, [FromQuery] string apiPassword, CancellationToken cancellationToken)
    {
        Result<ViberTemplateResponse> response = await Mediator.Send(new RefreshViberTemplateApiCommand(companyId, apiPassword ?? string.Empty, id), cancellationToken);
        return Map(response);
    }

    [HttpPost("session/send"), IgnoreAntiforgeryToken]
    public async Task<IActionResult> SendSession([FromBody] SendViberSessionApiCommand command, CancellationToken cancellationToken)
    {
        Result<ViberSessionResponse> response = await Mediator.Send(command, cancellationToken);
        if (!response.IsSuccess && response.Error.Code == "ViberSession.Closed")
        {
            return Conflict(response.Error);
        }

        return Map(response);
    }

    [HttpGet("session"), IgnoreAntiforgeryToken]
    public async Task<IActionResult> GetSession(
        [FromQuery] Guid companyId,
        [FromQuery] string apiPassword,
        [FromQuery] string sender,
        [FromQuery] string phoneNumber,
        CancellationToken cancellationToken)
    {
        Result<ViberSessionResponse> response = await Mediator.Send(
            new GetViberSessionByPhoneApiQuery(companyId, apiPassword ?? string.Empty, sender ?? string.Empty, phoneNumber ?? string.Empty),
            cancellationToken);
        return Map(response);
    }

    private IActionResult Map(Result result)
    {
        if (result.IsSuccess)
        {
            return Ok();
        }

        return result.Error.Code == "Company.InvalidApiPassword" ? Unauthorized(result.Error) : BadRequest(result.Error);
    }

    private IActionResult Map<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        if (result.Error.Code == "Company.InvalidApiPassword")
        {
            return Unauthorized(result.Error);
        }

        return result.Error.Code.EndsWith("NotFound", StringComparison.Ordinal) ? NotFound(result.Error) : BadRequest(result.Error);
    }
}
