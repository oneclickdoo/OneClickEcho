using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OneClickEcho.App.Abstractions;
using OneClickEcho.App.Abstractions.Queries;
using OneClickEcho.App.Infrastructure.Utils;
using OneClickEcho.Application.Lead.Commands.CreateLead;
using OneClickEcho.Application.Lead.Commands.DeleteLead;
using OneClickEcho.Application.Lead.Commands.EditLead;
using OneClickEcho.Application.Lead.Commands.UploadLeads;
using OneClickEcho.Application.Lead.Queries.DownloadExampleLeadCsv;
using OneClickEcho.Application.Lead.Queries.GetLeadById;
using OneClickEcho.Application.Lead.Queries.GetLeads;
using OneClickEcho.Domain.Common.Shared;
using OpenIddict.Validation.AspNetCore;

namespace OneClickEcho.App.Controllers
{
    [Route("api/Lead")]
    public class LeadController(IMediator mediator) : ApiController(mediator)
    {
        
        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [HttpPost("UploadLeads")]
        public async Task<IActionResult> UploadLeads([FromForm] IFormFile file, [FromForm] Guid companyId, [FromForm] Guid? campaignId,
            CancellationToken cancellationToken)
        {
            if (file is null || file.Length == 0)
            {
                return BadRequest("No file was uploaded (multipart field name must be \"file\").");
            }

            UploadLeadsCommand command = new(file, companyId, campaignId);

            Result<UploadLeadsResponse> response = await Mediator.Send(command, cancellationToken);

            return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
        }

        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [HttpPost]
        public async Task<IActionResult> CreateLead([FromBody] CreateLeadCommand request, CancellationToken cancellationToken)
        {
            Result<CreateLeadResponse> response = await Mediator.Send(request, cancellationToken);

            return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
        }

        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [HttpPut]
        public async Task<IActionResult> EditLead([FromBody] EditLeadCommand request, CancellationToken cancellationToken)
        {
            Result<EditLeadResponse> response = await Mediator.Send(request, cancellationToken);

            return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
        }

        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [HttpGet("{leadId}")]
        public async Task<IActionResult> GetLead(Guid leadId, CancellationToken cancellationToken)
        {
            GetLeadByIdQuery query = new(leadId);

            Result<GetLeadByIdResponse> response = await Mediator.Send(query, cancellationToken);

            return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
        }

        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [HttpGet]
        public async Task<IActionResult> GetLeads([FromQuery] PagedQueryParams pagedQueryParams, CancellationToken cancellationToken)
        {
            GetLeadsQuery query = pagedQueryParams.ConvertToBasePagedQuery<GetLeadsQuery>();
            
            if ((!User.IsInRole("Administrator") && !string.IsNullOrEmpty(query.Filter) && !query.Filter.Contains("CompanyId")) ||
                (!User.IsInRole("Administrator") && string.IsNullOrEmpty(query.Filter)))
            {
                return BadRequest("You are not authorized to filter by CompanyId.");
            }
            pagedQueryParams.Filter = UpdateFilter.WithCompanyId(User, query.Filter);

            Result<GetLeadsResponse> response = await Mediator.Send(query, cancellationToken);

            return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
        }

        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [HttpGet("Download")]
        public async Task<IActionResult> DownloadExampleCsv(CancellationToken cancellationToken)
        {
            DownloadExampleLeadCsvQuery query = new();

            Result<DownloadExampleLeadCsvResponse> response = await Mediator.Send(query, cancellationToken);

            return response.IsSuccess ?
                File(response.Value.FileBytes, "text/csv", $"leads.csv")
                :
                NotFound(response.Error);
        }

        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [HttpDelete("{leadId}")]
        public async Task<IActionResult> DeleteLead([FromRoute] Guid leadId, CancellationToken cancellationToken)
        {
            DeleteLeadCommand command = new(leadId);

            Result<DeleteLeadResponse> response = await Mediator.Send(command, cancellationToken);

            return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
        }
    }
}