namespace OneClickEcho.Application.Scheduling.Commands.QueueImmediateCampaigns
{
    public record QueueImmediateCampaignsResponse(List<Domain.CampaignAggregate.Campaign> ImmediateCampaigns);
}
