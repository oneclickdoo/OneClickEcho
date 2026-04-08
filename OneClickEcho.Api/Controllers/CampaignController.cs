using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OneClickEcho.App.Abstractions;
using OneClickEcho.App.Abstractions.Queries;
using OneClickEcho.App.Infrastructure.Utils;
using OneClickEcho.Application.Campaign.Commands.AddLeadsToCollectionFromStatus;
using OneClickEcho.Application.Campaign.Commands.AssignLeadCollection;
using OneClickEcho.Application.Campaign.Commands.CloneCampaign;
using OneClickEcho.Application.Campaign.Commands.CreateCampaign;
using OneClickEcho.Application.Campaign.Commands.CreateLeadCollectionFromStatus;
using OneClickEcho.Application.Campaign.Commands.DeleteCampaign;
using OneClickEcho.Application.Campaign.Commands.EditCampaign;
using OneClickEcho.Application.Campaign.Commands.EndCampaign;
using OneClickEcho.Application.Campaign.Commands.ExportLeadsFromStatus;
using OneClickEcho.Application.Campaign.Commands.LaunchCampaign;
using OneClickEcho.Application.Campaign.Commands.PauseCampaign;
using OneClickEcho.Application.Campaign.Commands.TestCampaign;
using OneClickEcho.Application.Campaign.Commands.UnassignLeadCollection;
using OneClickEcho.Application.Campaign.Commands.UploadCampaignViberMedia;
using OneClickEcho.Application.Campaign.Queries.ExportCampaignLeads;
using OneClickEcho.Application.Campaign.Queries.GetCampaignAnalytics;
using OneClickEcho.Application.Campaign.Queries.GetCampaignById;
using OneClickEcho.Application.Campaign.Queries.GetCampaignLeadCollections;
using OneClickEcho.Application.Campaign.Queries.GetCampaignLeads;
using OneClickEcho.Application.Campaign.Queries.GetCampaigns;
using OneClickEcho.Domain.Common.Shared;
using OpenIddict.Validation.AspNetCore;

namespace OneClickEcho.App.Controllers;

[Route("api/Campaign")]
public class CampaignController(IMediator mediator) : ApiController(mediator)
{
    private async Task<IActionResult?> RequireCampaignAccessAsync(Guid campaignId, CancellationToken cancellationToken)
    {
        Result<GetCampaignByIdResponse> r = await Mediator.Send(new GetCampaignByIdQuery(campaignId), cancellationToken);
        if (!r.IsSuccess)
        {
            return NotFound(r.Error);
        }

        if (!CampaignTenantFilter.UserMayAccessCampaignCompany(User, r.Value.CompanyId))
        {
            return Forbid();
        }

        return null;
    }

    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [HttpPost]
    public async Task<IActionResult> CreateCampaign([FromBody] CreateCampaignCommand request, CancellationToken cancellationToken)
    {
        if (!User.IsInRole("Administrator") &&
            !CampaignTenantFilter.GetUserCompanyIds(User).Contains(request.CompanyId))
        {
            return Forbid();
        }

        Result<CreateCampaignResponse> response = await Mediator.Send(request, cancellationToken);

        return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
    }

    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [HttpPut]
    public async Task<IActionResult> EditCampaign([FromBody] EditCampaignCommand request, CancellationToken cancellationToken)
    {
        IActionResult? denied = await RequireCampaignAccessAsync(request.CampaignId, cancellationToken);
        if (denied != null)
        {
            return denied;
        }

        Result<EditCampaignResponse> response = await Mediator.Send(request, cancellationToken);

        return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
    }

    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [HttpPut("{campaignId}/End")]
    public async Task<IActionResult> EndCampaign([FromRoute] Guid campaignId, CancellationToken cancellationToken)
    {
        IActionResult? denied = await RequireCampaignAccessAsync(campaignId, cancellationToken);
        if (denied != null)
        {
            return denied;
        }

        EndCampaignCommand command = new(campaignId);

        Result<EndCampaignResponse> response = await Mediator.Send(command, cancellationToken);

        return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
    }

    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [HttpPut("{campaignId}/Launch")]
    public async Task<IActionResult> LaunchCampaign([FromRoute] Guid campaignId, CancellationToken cancellationToken)
    {
        IActionResult? denied = await RequireCampaignAccessAsync(campaignId, cancellationToken);
        if (denied != null)
        {
            return denied;
        }

        LaunchCampaignCommand command = new(campaignId);

        Result<LaunchCampaignResponse> response = await Mediator.Send(command, cancellationToken);

        return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
    }

    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [HttpPut("{campaignId}/Pause")]
    public async Task<IActionResult> PauseCampaign([FromRoute] Guid campaignId, CancellationToken cancellationToken)
    {
        IActionResult? denied = await RequireCampaignAccessAsync(campaignId, cancellationToken);
        if (denied != null)
        {
            return denied;
        }

        PauseCampaignCommand command = new(campaignId);

        Result<PauseCampaignResponse> response = await Mediator.Send(command, cancellationToken);

        return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
    }

    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [HttpPost("{campaignId}/Upload")]
    [HttpPut("{campaignId}/Upload")]
    public async Task<IActionResult> UploadCampaignViberImage(
        [FromRoute] Guid campaignId,
        [FromQuery] bool isThumbnail,
        [FromQuery] int? duration,
        [FromForm] IFormFile file,
        CancellationToken cancellationToken)
    {
        IActionResult? denied = await RequireCampaignAccessAsync(campaignId, cancellationToken);
        if (denied != null)
        {
            return denied;
        }

        if (file is null || file.Length == 0)
        {
            return BadRequest("No file was uploaded (multipart field name must be \"file\").");
        }

        UploadCampaignViberMediaCommand command = new(campaignId, file, duration, isThumbnail);

        Result<UploadCampaignViberMediaResponse> response = await Mediator.Send(command, cancellationToken);

        return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
    }

    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [HttpPost("{campaignId}/Test")]
    public async Task<IActionResult> TestCampaign([FromRoute] Guid campaignId, [FromBody] TestCampaignObject testCampaignObject, CancellationToken cancellationToken)
    {
        IActionResult? denied = await RequireCampaignAccessAsync(campaignId, cancellationToken);
        if (denied != null)
        {
            return denied;
        }

        TestCampaignCommand command = new(campaignId, testCampaignObject.PhoneNumber);

        Result<TestCampaignResponse> response = await Mediator.Send(command, cancellationToken);

        return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
    }

    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [HttpPost("{campaignId}/Clone")]
    public async Task<IActionResult> CloneCampaign([FromRoute] Guid campaignId, CancellationToken cancellationToken)
    {
        IActionResult? denied = await RequireCampaignAccessAsync(campaignId, cancellationToken);
        if (denied != null)
        {
            return denied;
        }

        CloneCampaignCommand command = new(campaignId);

        Result<CloneCampaignResponse> response = await Mediator.Send(command, cancellationToken);

        return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
    }

    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [HttpGet("{campaignId}")]
    public async Task<IActionResult> GetCampaignById([FromRoute] Guid campaignId, CancellationToken cancellationToken)
    {
        GetCampaignByIdQuery query = new(campaignId);

        Result<GetCampaignByIdResponse> response = await Mediator.Send(query, cancellationToken);

        if (!response.IsSuccess)
        {
            return NotFound(response.Error);
        }

        if (!CampaignTenantFilter.UserMayAccessCampaignCompany(User, response.Value.CompanyId))
        {
            return Forbid();
        }

        return Ok(response.Value);
    }

    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [HttpGet("{campaignId}/Analytics")]
    public async Task<IActionResult> GetCampaignAnalytics([FromRoute] Guid campaignId, CancellationToken cancellationToken)
    {
        IActionResult? denied = await RequireCampaignAccessAsync(campaignId, cancellationToken);
        if (denied != null)
        {
            return denied;
        }

        GetCampaignAnalyticsQuery query = new(campaignId);

        Result<GetCampaignAnalyticsResponse> response = await Mediator.Send(query, cancellationToken);

        return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
    }

    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [HttpGet("{campaignId}/Leads")]
    public async Task<IActionResult> GetCampaignLeads([FromRoute] Guid campaignId, [FromQuery] PagedQueryParams pagedQueryParams,
        CancellationToken cancellationToken)
    {
        IActionResult? denied = await RequireCampaignAccessAsync(campaignId, cancellationToken);
        if (denied != null)
        {
            return denied;
        }

        GetCampaignLeadsQuery query = pagedQueryParams.ConvertToBasePagedQuery<GetCampaignLeadsQuery>();

        query.CampaignId = campaignId;

        Result<GetCampaignLeadsResponse> response = await Mediator.Send(query, cancellationToken);

        return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
    }

    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [HttpGet("{campaignId}/LeadCollections")]
    public async Task<IActionResult> GetCampaignLeadCollections([FromRoute] Guid campaignId,
        [FromQuery] PagedQueryParams pagedQueryParams,
        CancellationToken cancellationToken)
    {
        IActionResult? denied = await RequireCampaignAccessAsync(campaignId, cancellationToken);
        if (denied != null)
        {
            return denied;
        }

        GetCampaignLeadCollectionsQuery query = pagedQueryParams.ConvertToBasePagedQuery<GetCampaignLeadCollectionsQuery>();

        query.CampaignId = campaignId;

        Result<GetCampaignLeadCollectionsResponse> response = await Mediator.Send(query, cancellationToken);

        return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
    }

    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [HttpPost("{campaignId}/LeadCollections")]
    public async Task<IActionResult> AssignCampaignLeadCollections([FromRoute] Guid campaignId,
        [FromBody] AssignLeadCollectionDto assignLeadCollectionDto,
        CancellationToken cancellationToken)
    {
        IActionResult? denied = await RequireCampaignAccessAsync(campaignId, cancellationToken);
        if (denied != null)
        {
            return denied;
        }

        AssignLeadCollectionCommand command = new(campaignId, assignLeadCollectionDto.LeadCollectionId);

        Result<AssignLeadCollectionResponse> response = await Mediator.Send(command, cancellationToken);

        return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
    }

    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [HttpDelete("{campaignId}/LeadCollections/{leadCollectionId}")]
    public async Task<IActionResult> UnassignCampaignLeadCollections([FromRoute] Guid campaignId, [FromRoute] Guid leadCollectionId,
        CancellationToken cancellationToken)
    {
        IActionResult? denied = await RequireCampaignAccessAsync(campaignId, cancellationToken);
        if (denied != null)
        {
            return denied;
        }

        UnassignLeadCollectionCommand command = new(campaignId, leadCollectionId);

        Result<UnassignLeadCollectionResponse> response = await Mediator.Send(command, cancellationToken);

        return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
    }

    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [HttpGet("{campaignId}/ExportLeads")]
    public async Task<IActionResult> ExportCampaignLeads([FromRoute] Guid campaignId, CancellationToken cancellationToken)
    {
        IActionResult? denied = await RequireCampaignAccessAsync(campaignId, cancellationToken);
        if (denied != null)
        {
            return denied;
        }

        ExportCampaignLeadsQuery query = new(campaignId);

        Result<ExportCampaignLeadsResponse> response = await Mediator.Send(query, cancellationToken);

        return response.IsSuccess ?
            File(response.Value.FileBytes, "text/csv", $"leads-{campaignId}.csv")
            :
            NotFound(response.Error);
    }
        
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [HttpPost("{campaignId}/CreateCollection")]
    public async Task<IActionResult> CreateLeadCollectionFromStatus([FromRoute] Guid campaignId,
        [FromBody] CreateLeadCollectionFromStatusCommandDto createLeadCollectionFromStatusCommandDto,
        CancellationToken cancellationToken)
    {
        IActionResult? denied = await RequireCampaignAccessAsync(campaignId, cancellationToken);
        if (denied != null)
        {
            return denied;
        }

        if (!User.IsInRole("Administrator") &&
            !CampaignTenantFilter.GetUserCompanyIds(User).Contains(createLeadCollectionFromStatusCommandDto.CompanyId))
        {
            return Forbid();
        }

        CreateLeadCollectionFromStatusCommand command = new(
            createLeadCollectionFromStatusCommandDto.CompanyId,
            campaignId,
            createLeadCollectionFromStatusCommandDto.ViberStatus,
            createLeadCollectionFromStatusCommandDto.SmsStatus,
            createLeadCollectionFromStatusCommandDto.CollectionName
        );

        Result<CreateLeadCollectionFromStatusResponse> response = await Mediator.Send(command, cancellationToken);

        return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
    }
        
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [HttpPost("{campaignId}/AddToCollection")]
    public async Task<IActionResult> AddLeadsToCollectionFromStatus([FromRoute] Guid campaignId,
        [FromBody] AddLeadsToCollectionFromStatusCommandDto addLeadsToCollectionFromStatusCommandDto,
        CancellationToken cancellationToken)
    {
        IActionResult? denied = await RequireCampaignAccessAsync(campaignId, cancellationToken);
        if (denied != null)
        {
            return denied;
        }

        AddLeadsToCollectionFromStatusCommand command = new(
            addLeadsToCollectionFromStatusCommandDto.LeadCollectionId,
            campaignId,
            addLeadsToCollectionFromStatusCommandDto.ViberStatus,
            addLeadsToCollectionFromStatusCommandDto.SmsStatus
        );

        Result<AddLeadsToCollectionFromStatusResponse> response = await Mediator.Send(command, cancellationToken);

        return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
    }

    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [HttpPost("{campaignId}/ExportFromStatus")]
    public async Task<IActionResult> ExportLeadsFromStatus([FromRoute] Guid campaignId,
        [FromBody] ExportLeadsFromStatusCommandDto exportLeadsFromStatusCommandDto,
        CancellationToken cancellationToken)
    {
        IActionResult? denied = await RequireCampaignAccessAsync(campaignId, cancellationToken);
        if (denied != null)
        {
            return denied;
        }

        ExportLeadsFromStatusCommand command = new(
            campaignId,
            exportLeadsFromStatusCommandDto.ViberStatus,
            exportLeadsFromStatusCommandDto.SmsStatus
        );

        Result<ExportLeadsFromStatusResponse> response = await Mediator.Send(command, cancellationToken);

        return response.IsSuccess ?
            File(response.Value.FileBytes, "text/csv", $"leads-{campaignId}.csv")
            : NotFound(response.Error);
    }

    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [HttpGet]
    public async Task<IActionResult> GetCampaigns([FromQuery] PagedQueryParams pagedQueryParams, CancellationToken cancellationToken)
    {
        GetCampaignsQuery query = pagedQueryParams.ConvertToBasePagedQuery<GetCampaignsQuery>();

        if (User.IsInRole("Administrator"))
        {
            if (!pagedQueryParams.CompanyId.HasValue || pagedQueryParams.CompanyId == Guid.Empty)
            {
                return BadRequest("CompanyId query parameter is required.");
            }
        }
        else
        {
            if (pagedQueryParams.CompanyId is { } scopedCompany && scopedCompany != Guid.Empty)
            {
                if (!CampaignTenantFilter.GetUserCompanyIds(User).Contains(scopedCompany))
                {
                    return Forbid();
                }
            }
        }

        query.Filter = CampaignTenantFilter.BuildCampaignsListFilter(User, query.Filter, pagedQueryParams.CompanyId);

        Result<GetCampaignsResponse> response = await Mediator.Send(query, cancellationToken);
        
        return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
    }

    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [HttpDelete("{campaignId}")]
    public async Task<IActionResult> DeleteLeadCampaign([FromRoute] Guid campaignId, CancellationToken cancellationToken)
    {
        IActionResult? denied = await RequireCampaignAccessAsync(campaignId, cancellationToken);
        if (denied != null)
        {
            return denied;
        }

        DeleteCampaignCommand command = new(campaignId);

        Result<DeleteCampaignResponse> response = await Mediator.Send(command, cancellationToken);

        return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
    }
}

public record TestCampaignObject(string PhoneNumber);