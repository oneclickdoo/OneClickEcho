using OneClickEcho.Application.Common.Helpers;
using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.CampaignAggregate.Repositories;
using OneClickEcho.Domain.CampaignAggregate.ValueObjects;
using OneClickEcho.Domain.CampaignLeadAggregate.Repositories;
using OneClickEcho.Domain.Common.Repositories;
using OneClickEcho.Domain.Common.Shared;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;
using OneClickEcho.Domain.LeadAggregate.Repositories;

namespace OneClickEcho.Application.Lead.Commands.CreateLead;

public class CreateLeadHandler(ICampaignRepository campaignRepository, ICampaignLeadRepository campaignLeadRepository,
    ILeadRepository leadRepository, IUnitOfWork unitOfWork) : ICommandHandler<CreateLeadCommand, CreateLeadResponse>
{
    private readonly ICampaignRepository _campaignRepository = campaignRepository;
    private readonly ICampaignLeadRepository _campaignLeadRepository = campaignLeadRepository;
    private readonly ILeadRepository _leadRepository = leadRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result<CreateLeadResponse>> Handle(CreateLeadCommand request, CancellationToken cancellationToken)
    {
        Domain.LeadAggregate.Lead lead = await LeadHelper.CreateOrUpdate(request, CompanyId.Create(request.CompanyId), _leadRepository);

        // if campaign id provided, get campaign
        if (request.CampaignId != null)
        {
            Domain.CampaignAggregate.Campaign? campaign = await _campaignRepository
                .GetByIdAsync(CampaignId.Create((Guid)request.CampaignId), cancellationToken);

            if (campaign is null)
            {
                return Result.Failure<CreateLeadResponse>(new Error(
                    "Campaign.NotFound",
                    $"The Campaign with Id:\"{request.CampaignId}\" does not exist."
                ));
            }

            Domain.CampaignLeadAggregate.CampaignLead? existingLink =
                await _campaignLeadRepository.GetByCampaignAndLeadId(campaign.Id, lead.Id, cancellationToken);

            if (existingLink is null)
            {
                _campaignLeadRepository.Add(new Domain.CampaignLeadAggregate.CampaignLead(lead.Id, campaign.Id));
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateLeadResponse(lead.Id.Value);
    }
}