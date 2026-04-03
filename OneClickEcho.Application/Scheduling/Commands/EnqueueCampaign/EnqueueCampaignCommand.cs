using OneClickEcho.Application.Common.Messaging;

namespace OneClickEcho.Application.Scheduling.Commands.EnqueueCampaign;

public sealed record EnqueueCampaignCommand(Guid CampaignId) : ICommand<EnqueueCampaignResponse>;