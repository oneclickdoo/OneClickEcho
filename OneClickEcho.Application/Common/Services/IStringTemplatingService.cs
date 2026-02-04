namespace OneClickEcho.Application.Common.Services;

public interface IStringTemplatingService
{
    public string SubstituteLeadInfo(string campaignMessage, Domain.LeadAggregate.Lead lead);
}