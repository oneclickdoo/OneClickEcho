using Microsoft.Extensions.DependencyInjection;
using OneClickEcho.Application.Common.Services.GptService;
using OneClickEcho.Domain.GptRequestAggregate.Enums;

namespace OneClickEcho.Infrastructure.Services.GptService;

public class GptRequestStrategyFactory(IServiceProvider serviceProvider)
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

#pragma warning disable CS8603 // Possible null reference return.
    public IGptRequestStrategy GetGptRequestStrategy(GptRequestType requestType)
    {
        return requestType switch
        {
            GptRequestType.GenerateNewCampaignMessage => _serviceProvider.GetService<GenerateNewCampaignMessageStrategy>(),
            GptRequestType.EnhanceCampaignMessage => _serviceProvider.GetService<EnhanceCampaignMessageStrategy>(),
            GptRequestType.ConvertNounCases => _serviceProvider.GetService<ConvertNounCasesStrategy>(),
            _ => throw new ArgumentOutOfRangeException(nameof(requestType), $"Invalid GPT request type provided: {requestType}")
        };
    }
#pragma warning restore CS8603 // Possible null reference return.
}