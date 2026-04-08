using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OneClickEcho.App.Abstractions;
using OneClickEcho.App.Abstractions.Queries;
using OneClickEcho.App.Infrastructure.Utils;
using OneClickEcho.Application.LeadCollection.Commands.AssignLeadsToCollection;
using OneClickEcho.Application.LeadCollection.Commands.CreateAndAssignLeadToCollection;
using OneClickEcho.Application.LeadCollection.Commands.CreateLeadCollection;
using OneClickEcho.Application.LeadCollection.Commands.DeleteLeadCollection;
using OneClickEcho.Application.LeadCollection.Commands.EditLeadCollection;
using OneClickEcho.Application.LeadCollection.Commands.UploadAndAssignToCollection;
using OneClickEcho.Application.LeadCollection.Queries.GetLeadCollectionById;
using OneClickEcho.Application.LeadCollection.Queries.GetLeadCollectionCount;
using OneClickEcho.Application.LeadCollection.Queries.GetLeadCollectionLeads;
using OneClickEcho.Application.LeadCollection.Queries.GetLeadCollections;
using OneClickEcho.Application.LeadCollection.Queries.SearchLeadCollections;
using OneClickEcho.Domain.Common.Shared;
using OpenIddict.Validation.AspNetCore;

namespace OneClickEcho.App.Controllers
{
    [Route("api/[controller]")]
    public class LeadCollectionController(IMediator mediator) : ApiController(mediator)
    {
        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [HttpGet]
        public async Task<IActionResult> GetLeadsCollections([FromQuery] PagedQueryParams pagedQueryParams, CancellationToken cancellationToken)
        {
            GetLeadCollectionsQuery query = pagedQueryParams.ConvertToBasePagedQuery<GetLeadCollectionsQuery>();

            Result<GetLeadCollectionsResponse> response = await Mediator.Send(query, cancellationToken);

            return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
        }

        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [HttpGet("{id}")]
        public async Task<ActionResult> GetLeadCollection(Guid id, CancellationToken cancellationToken)
        {
            GetLeadCollectionByIdQuery query = new(id);

            Result<GetLeadCollectionByIdResponse> response = await Mediator.Send(query, cancellationToken);

            return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
        }

        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [HttpGet("{id}/Leads")]
        public async Task<IActionResult> GetLeadCollectionsLeads([FromRoute] Guid id,
            [FromQuery] PagedQueryParams pagedQueryParams,
            CancellationToken cancellationToken)
        {
            GetLeadCollectionLeadsQuery query = pagedQueryParams.ConvertToBasePagedQuery<GetLeadCollectionLeadsQuery>();
            
            if ((!User.IsInRole("Administrator") && !string.IsNullOrEmpty(query.Filter) && !query.Filter.Contains("CompanyId")) ||
                (!User.IsInRole("Administrator") && string.IsNullOrEmpty(query.Filter)))
            {
                return BadRequest("You are not authorized to filter by CompanyId.");
            }

            pagedQueryParams.Filter = UpdateFilter.WithCompanyId(User, query.Filter);
            query.LeadCollectionId = id;

            Result<GetLeadCollectionLeadsResponse> response = await Mediator.Send(query, cancellationToken);

            return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
        }

        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [HttpGet("Search")]
        public async Task<IActionResult> SearchLeadCollections([FromQuery] SearchLeadCollectionsQuery query, CancellationToken cancellationToken)
        {
            Result<List<GetLeadCollectionDto>> response = await Mediator.Send(query, cancellationToken);

            return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
        }

        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [HttpPost]
        public async Task<ActionResult> CreateLeadCollection([FromBody] CreateLeadCollectionCommand command, CancellationToken cancellationToken)
        {
            Result<CreateLeadCollectionResponse> response = await Mediator.Send(command, cancellationToken);

            return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
        }

        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [HttpPut]
        public async Task<IActionResult> UpdateLeadCollection([FromBody] EditLeadCollectionCommand command, CancellationToken cancellationToken)
        {
            Result<EditLeadCollectionResponse> response = await Mediator.Send(command, cancellationToken);

            return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
        }

        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLeadCollection([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            DeleteLeadCollectionCommand command = new(id);

            Result<DeleteLeadCollectionResponse> response = await Mediator.Send(command, cancellationToken);

            return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
        }

        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [HttpPost("AssignLeads")]
        public async Task<IActionResult> AssignLeads([FromBody] AssignLeadsToCollectionCommand command, CancellationToken cancellationToken)
        {
            Result<AssignLeadsToCollectionResponse> response = await Mediator.Send(command, cancellationToken);

            return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
        }

        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [HttpPost("CreateAndAssignLead")]
        public async Task<IActionResult> CreateAndAssignLead([FromBody] CreateAndAssignLeadToCollectionCommand command,
            CancellationToken cancellationToken)
        {
            Result<CreateAndAssignLeadToCollectionResponse> response = await Mediator.Send(command, cancellationToken);

            return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
        }

        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [HttpPost("UploadAndAssignLeads")]
        public async Task<IActionResult> AssignLeads(
            [FromForm] IFormFile file,
            [FromForm(Name = "LeadCollectionId")] Guid leadCollectionId,
            CancellationToken cancellationToken)
        {
            if (file is null || file.Length == 0)
            {
                return BadRequest("No file was uploaded (multipart field name must be \"file\").");
            }

            Result<UploadAndAssignLeadsToCollectionResponse> response = await Mediator.Send(
                new UploadAndAssignLeadsToCollectionCommand(file, leadCollectionId), cancellationToken);

            return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
        }
        
        
        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [HttpPost("Count")]
        public async Task<IActionResult> GetLeadCollectionCount([FromBody] GetLeadCollectionCountQuery queryBody, CancellationToken cancellationToken)
        {
            Result<GetLeadCollectionCountResponse> response = await Mediator.Send(queryBody, cancellationToken);
            return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
        }
    }
}