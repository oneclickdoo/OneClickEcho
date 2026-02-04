using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OneClickEcho.App.Abstractions;
using OneClickEcho.App.Abstractions.Queries;
using OneClickEcho.Application.Company.Commands.CreateCompany;
using OneClickEcho.Application.Company.Commands.DeleteCompany;
using OneClickEcho.Application.Company.Commands.EditCompany;
using OneClickEcho.Application.Company.Commands.UploadBlacklist;
using OneClickEcho.Application.Company.Queries.ExportCompanyLeads;
using OneClickEcho.Application.Company.Queries.GetCompanies;
using OneClickEcho.Application.Company.Queries.GetCompanyAnalytics;
using OneClickEcho.Application.Company.Queries.GetCompanyById;
using OneClickEcho.Application.Company.Queries.GetCompanyImagesById;
using OneClickEcho.Application.Sender.Queries.GetSenders;
using OneClickEcho.Domain.Common.Shared;
using OpenIddict.Validation.AspNetCore;

namespace OneClickEcho.App.Controllers
{
    [Route("api/Company")]
    public class CompanyController(IMediator mediator) : ApiController(mediator)
    {
        [Authorize(Roles = "Administrator")]
        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [HttpGet]
        public async Task<IActionResult> GetCompanies([FromQuery] PagedQueryParams pagedQueryParams,
            CancellationToken cancellationToken)
        {
            GetCompaniesQuery query = pagedQueryParams.ConvertToBasePagedQuery<GetCompaniesQuery>();

            Result<GetCompaniesResponse> response = await Mediator.Send(query, cancellationToken);

            return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
        }

        [Authorize(Roles = "Administrator")]
        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [HttpGet("{companyId}")]
        public async Task<IActionResult> GetCompanyById(Guid companyId, CancellationToken cancellationToken)
        {
            GetCompanyByIdQuery query = new(companyId);

            Result<GetCompanyByIdResponse> response = await Mediator.Send(query, cancellationToken);

            return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
        }

        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [HttpGet("{companyId}/Images")]
        public async Task<IActionResult> GetCompanyImagesById(Guid companyId, CancellationToken cancellationToken)
        {
            GetCompanyImagesByIdQuery query = new(companyId);

            Result<GetCompanyImagesByIdResponse> response = await Mediator.Send(query, cancellationToken);

            return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
        }

        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [HttpGet("{companyId}/Analytics")]
        public async Task<IActionResult> GetCompanyAnalytics(Guid companyId, [FromQuery] string? startDate, [FromQuery] string? endDate,
            CancellationToken cancellationToken)
        {
            GetCompanyAnalyticsQuery query = new(companyId, startDate, endDate);

            Result<GetCompanyAnalyticsResponse> response = await Mediator.Send(query, cancellationToken);

            return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
        }

        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [HttpGet("{companyId}/ExportLeads")]
        public async Task<IActionResult> ExportCompanyLeads(Guid companyId, CancellationToken cancellationToken)
        {
            ExportCompanyLeadsQuery query = new(companyId);

            Result<ExportCompanyLeadsResponse> response = await Mediator.Send(query, cancellationToken);

            return response.IsSuccess ?
                File(response.Value.FileBytes, "text/csv", $"leads-{companyId}.csv")
                :
                NotFound(response.Error);
        }
        
        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [HttpGet("{companyId}/ExportLeadsByCollection/{collectionId}")]
        public async Task<IActionResult> ExportCompanyLeadsByCollection(Guid companyId, Guid collectionId, CancellationToken cancellationToken)
        {
            ExportCompanyLeadsQuery query = new(companyId, collectionId);

            Result<ExportCompanyLeadsResponse> response = await Mediator.Send(query, cancellationToken);

            return response.IsSuccess ?
                File(response.Value.FileBytes, "text/csv", $"leads-{companyId}.csv")
                :
                NotFound(response.Error);
        }
        
        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [HttpPost("{companyId}/UploadBlacklist")]
        public async Task<IActionResult> UploadBlacklist(
            IFormFile file,
            Guid companyId,
            CancellationToken cancellationToken)
        {
            Result<UploadBlacklistResponse> response = await Mediator.Send(
                new UploadBlacklistCommand(file, companyId), cancellationToken);

            return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
        }

        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [HttpGet("{companyId}/Senders")]
        public async Task<IActionResult> GetCompanySenders([FromRoute] Guid companyId, CancellationToken cancellationToken)
        {
            GetSendersQuery query = new(companyId);

            Result<List<GetSendersResponse>> response = await Mediator.Send(query, cancellationToken);

            return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
        }

        [Authorize(Roles = "Administrator")]
        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [HttpPost]
        public async Task<IActionResult> CreateCompany([FromBody] CreateCompanyCommand createCompanyCommand,
            CancellationToken cancellationToken)
        {
            Result<CreateCompanyResponse> response = await Mediator.Send(createCompanyCommand, cancellationToken);

            return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
        }

        [Authorize(Roles = "Administrator")]
        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [HttpPut]
        public async Task<IActionResult> EditCompany([FromBody] EditCompanyCommand request, CancellationToken cancellationToken)
        {
            Result<EditCompanyResponse> response = await Mediator.Send(request, cancellationToken);

            return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
        }

        [Authorize(Roles = "Administrator")]
        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [HttpDelete("{companyId}")]
        public async Task<IActionResult> DeleteLeadCampaign([FromRoute] Guid companyId, CancellationToken cancellationToken)
        {
            DeleteCompanyCommand command = new(companyId);

            Result<DeleteCompanyResponse> response = await Mediator.Send(command, cancellationToken);

            return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
        }
    }
}
