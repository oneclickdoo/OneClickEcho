using OneClickEcho.Application.Common.Messaging;

namespace OneClickEcho.Application.Scheduling.Commands.CompleteCampaign;

public sealed record CompleteCampaignCommand(Guid CampaignId) : ICommand<CompleteCampaignResponse>;