using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OneClickEcho.App.Abstractions;
using OneClickEcho.Application.Sender.Commands.CreateSender;
using OneClickEcho.Application.Sender.Commands.DeleteSender;
using OneClickEcho.Domain.Common.Shared;
using OpenIddict.Validation.AspNetCore;

namespace OneClickEcho.App.Controllers
{
    [Route("api/Sender")]
    public class SenderController(IMediator mediator) : ApiController(mediator)
    {
        [Authorize(Roles = "Administrator")]
        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [HttpPost]
        public async Task<IActionResult> CreateSender([FromBody] CreateSenderCommand createSenderCommand,
            CancellationToken cancellationToken)
        {
            Result<CreateSenderResponse> response = await Mediator.Send(createSenderCommand, cancellationToken);

            return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
        }

        [Authorize(Roles = "Administrator")]
        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [HttpDelete("{senderId}")]
        public async Task<IActionResult> DeleteSender([FromRoute] Guid senderId, CancellationToken cancellationToken)
        {
            DeleteSenderCommand command = new(senderId);

            Result<DeleteSenderResponse> response = await Mediator.Send(command, cancellationToken);

            return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
        }
    }
}
