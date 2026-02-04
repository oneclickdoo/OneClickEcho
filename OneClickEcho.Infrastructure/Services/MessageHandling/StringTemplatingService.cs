using MediatR;
using OneClickEcho.Application.Common.Services;
using OneClickEcho.Application.GptRequest.Commands.ConvertNounCases;
using OneClickEcho.Domain.LeadAggregate;

namespace OneClickEcho.Infrastructure.Services.MessageHandling;

public class StringTemplatingService(IMediator mediator) : IStringTemplatingService
{
    private readonly IMediator _mediator = mediator;

    public string SubstituteLeadInfo(string campaignMessage, Lead lead)
    {
        // replace nominative
        campaignMessage = campaignMessage.Replace("{firstName}", lead.FirstName ?? "");
        campaignMessage = campaignMessage.Replace("{lastName}", lead.LastName ?? "");

        campaignMessage = campaignMessage.Replace("{firstName:nominative}", lead.FirstName ?? "");
        campaignMessage = campaignMessage.Replace("{lastName:nominative}", lead.LastName ?? "");

        // replace vocative
        if (campaignMessage.Contains("{firstName:vocative}"))
        {
            if (lead.FirstName is not null)
            {
                Task<Domain.Common.Shared.Result<ConvertNounCasesResponse>> response = _mediator
                    .Send(new ConvertNounCasesCommand(lead.FirstName));

                campaignMessage = campaignMessage.Replace("{firstName:vocative}", response.Result.Value.Name);
            }
            else
            {
                campaignMessage = campaignMessage.Replace("{firstName:vocative}", "");
            }
        }

        if (campaignMessage.Contains("{lastName:vocative}"))
        {
            if (lead.LastName is not null)
            {
                Task<Domain.Common.Shared.Result<ConvertNounCasesResponse>> response = _mediator
                    .Send(new ConvertNounCasesCommand(lead.LastName));

                campaignMessage = campaignMessage.Replace("{lastName:vocative}", response.Result.Value.Name);
            }
            else
            {
                campaignMessage = campaignMessage.Replace("{lastName:vocative}", "");
            }
        }

        return campaignMessage;
    }
}