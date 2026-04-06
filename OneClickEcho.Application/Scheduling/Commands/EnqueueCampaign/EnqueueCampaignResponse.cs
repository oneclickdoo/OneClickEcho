namespace OneClickEcho.Application.Scheduling.Commands.EnqueueCampaign;

/// <param name="ClaimedFromQueued"><see langword="true"/> if this call moved the campaign from Queued to InProgress (single winner across duplicate jobs).</param>
public record EnqueueCampaignResponse(bool ClaimedFromQueued);