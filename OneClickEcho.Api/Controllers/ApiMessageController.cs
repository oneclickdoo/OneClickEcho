using MediatR;
using Microsoft.AspNetCore.Mvc;
using OneClickEcho.App.Abstractions;
using OneClickEcho.App.Abstractions.Queries;
using OneClickEcho.App.Infrastructure.Utils;
using OneClickEcho.Application.ApiMessage.Commands.SendApiMessage;
using OneClickEcho.Application.ApiMessage.Queries.GetApiMessageStatus;
using OneClickEcho.Application.ApiMessage.Queries.GetApiMessages;
using OneClickEcho.Domain.Common.Shared;

namespace OneClickEcho.App.Controllers;

[Route("api/Message")]
public class ApiMessageController(IMediator mediator) : ApiController(mediator)
{
    /// <summary>
    /// Enqueues a message for async send. Response includes initial status; poll status endpoint for delivery updates.
    /// </summary>
    [HttpPost("Send"), IgnoreAntiforgeryToken, Produces("application/json")]
    public async Task<IActionResult> SendApiMessage([FromBody] SendApiMessageCommand sendApiMessageCommand, CancellationToken cancellationToken)
    {
        Result<SendApiMessageResponse> response = await Mediator.Send(sendApiMessageCommand, cancellationToken);
        return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
    }

    /// <summary>
    /// Public status poll (same auth as Send: companyId + apiPassword). Delivery is updated by background jobs.
    /// </summary>
    [HttpGet("{id:guid}/status"), IgnoreAntiforgeryToken, Produces("application/json")]
    public async Task<IActionResult> GetApiMessageStatus(
        [FromRoute] Guid id,
        [FromQuery] Guid companyId,
        [FromQuery] string apiPassword,
        CancellationToken cancellationToken)
    {
        GetApiMessageStatusQuery query = new(id, companyId, apiPassword ?? string.Empty);
        Result<GetApiMessageStatusResponse> response = await Mediator.Send(query, cancellationToken);

        if (!response.IsSuccess)
        {
            if (response.Error.Code == "ApiMessage.NotFound")
            {
                return NotFound(response.Error);
            }

            return Unauthorized(response.Error);
        }

        return Ok(response.Value);
    }
    
    [HttpGet, Produces("application/json")]
    public async Task<IActionResult> GetApiMessages([FromQuery] PagedQueryParams pagedQueryParams, CancellationToken cancellationToken)
    {
        pagedQueryParams.Filter = UpdateFilter.WithCompanyId(User, pagedQueryParams.Filter);

        GetApiMessagesQuery query = pagedQueryParams.ConvertToBasePagedQuery<GetApiMessagesQuery>();

        if ((!User.IsInRole("Administrator") && !string.IsNullOrEmpty(query.Filter) && !query.Filter.Contains("CompanyId")) ||
            (!User.IsInRole("Administrator") && string.IsNullOrEmpty(query.Filter)))
        {
            return BadRequest("You are not authorized to filter by CompanyId.");
        }

        Result<GetApiMessagesResponse> response = await Mediator.Send(query, cancellationToken);

        return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
    }
}