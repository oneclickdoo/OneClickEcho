using OneClickEcho.Application.Common.Abstractions;
using OneClickEcho.Application.Lead.Queries.GetLeads;
using OneClickEcho.Domain.Common.Queries;

namespace OneClickEcho.Application.Campaign.Queries.GetCampaignLeads
{
    public class GetCampaignLeadsResponse(IPagedList<Domain.LeadAggregate.Lead> items)
        : PagedListDto<Domain.LeadAggregate.Lead, GetLeadDto>(items)
    {
        public override List<GetLeadDto> ConvertTToTDto(List<Domain.LeadAggregate.Lead> items)
        {
            List<GetLeadDto> result = [];

            foreach (Domain.LeadAggregate.Lead lead in items)
            {
                result.Add(new GetLeadDto(
                    LeadId: lead.Id.Value,
                    FirstName: lead.FirstName,
                    LastName: lead.LastName,
                    Email: lead.Email,
                    PhoneNumber: lead.PhoneNumber,
                    Gender: lead.Gender,
                    DateOfBirth: lead.DateOfBirth,
                    City: lead.City,
                    State: lead.State,
                    Country: lead.Country,
                    IsBlacklisted: lead.IsBlacklisted
                ));
            }

            return result;
        }
    }
}
