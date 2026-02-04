using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OneClickEcho.App.Abstractions;
using OneClickEcho.Application.NounCase.Queries.GetNounCaseByNominative;
using OneClickEcho.Domain.Common.Shared;
using OpenIddict.Validation.AspNetCore;

namespace OneClickEcho.App.Controllers
{
    [Route("api/NounCase")]
    public class NounCaseController(IMediator mediator) : ApiController(mediator)
    {
        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [HttpGet("{nominative}")]
        public async Task<IActionResult> GetNounCaseByNominative(string nominative, CancellationToken cancellationToken)
        {
            GetNounCaseByNominativeQuery query = new(nominative);

            Result<GetNounCaseByNominativeResponse> response = await Mediator.Send(query, cancellationToken);

            return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
        }
    }
}
