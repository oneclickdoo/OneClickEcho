using OneClickEcho.Domain.CampaignLeadAggregate;
using OneClickEcho.Domain.LeadAggregate;

namespace OneClickEcho.Domain.Common.Models
{
    public class LeadWithCampaignLeadDto
    {
        public Lead Lead { get; set; } = default!;

        public CampaignLead CampaignLead { get; set; } = default!;
    }
}
