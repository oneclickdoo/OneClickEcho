using OneClickEcho.Application.Common.Messaging;

namespace OneClickEcho.Application.Campaign.Commands.PrepareCampaignLaunch;

public sealed record PrepareCampaignLaunchCommand(Guid CampaignId) : ICommand;
