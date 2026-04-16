using MediatR;
using Microsoft.AspNetCore.Mvc;
using OneClickEcho.App.Abstractions;
using OneClickEcho.App.Abstractions.Queries;
using OneClickEcho.App.Infrastructure.Utils;
using OneClickEcho.Application.ApiMessage.Commands.SendApiMessage;
using OneClickEcho.Application.ApiMessage.Queries.GetApiMessages;
using OneClickEcho.Domain.Common.Shared;

namespace OneClickEcho.App.Controllers;

[Route("api/Message")]
public class ApiMessageController(IMediator mediator) : ApiController(mediator)
{
    [HttpPost("Send"), IgnoreAntiforgeryToken, Produces("application/json")]
    public async Task<IActionResult> SendApiMessage([FromBody] SendApiMessageCommand sendApiMessageCommand, CancellationToken cancellationToken)
    {
        Result<SendApiMessageResponse> response = await Mediator.Send(sendApiMessageCommand, cancellationToken);
        return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
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