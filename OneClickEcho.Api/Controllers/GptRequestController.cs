using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OneClickEcho.App.Abstractions;
using OneClickEcho.Application.GptRequest.Commands.EnhanceCampaignMessage;
using OneClickEcho.Application.GptRequest.Commands.GenerateNewCampaignMessage;
using OneClickEcho.Domain.Common.Shared;
using OpenIddict.Validation.AspNetCore;

namespace OneClickEcho.App.Controllers
{
    [Route("api/[controller]")]
    public class GptRequestController(IMediator mediator) : ApiController(mediator)
    {
        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [HttpPost("Generate")]
        public async Task<ActionResult> GenerateNewCampaignMessage(
            [FromBody] GenerateNewCampaignMessageCommand generateNewCampaignMessageCommand, CancellationToken cancellationToken)
        {
            Result<GenerateNewCampaignMessageResponse> response = await Mediator
                .Send(generateNewCampaignMessageCommand, cancellationToken);

            if (response.IsFailure)
            {
                return BadRequest(response.Error);
            }

            return Ok(new
            {
                GptRequestId = response.Value.Id,
                response.Value.ResponseMessage
            });
        }

        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [HttpPost("Enhance")]
        public async Task<ActionResult> EnhanceCampaignMessage(
            EnhanceCampaignMessageCommand enhanceCampaignMessageCommand, CancellationToken cancellationToken)
        {
            Result<EnhanceCampaignMessageResponse> response = await Mediator
                .Send(enhanceCampaignMessageCommand, cancellationToken);

            if (response.IsFailure)
            {
                return BadRequest(response.Error);
            }

            return Ok(new
            {
                GptRequestId = response.Value.Id,
                response.Value.ResponseMessage
            });
        }
    }
}