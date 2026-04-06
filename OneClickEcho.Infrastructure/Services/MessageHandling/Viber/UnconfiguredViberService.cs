using OneClickEcho.Application.Common.Services.ViberService;
using OneClickEcho.Application.Common.Services.ViberService.Request;
using OneClickEcho.Application.Common.Services.ViberService.Response;

namespace OneClickEcho.Infrastructure.Services.MessageHandling.Viber;

/// <summary>
/// Used when Viber:Username / Viber:Password are missing so the host can start; Viber calls effectively no-op (null responses).
/// </summary>
public sealed class UnconfiguredViberService : IViberService
{
    public Task<SendViberMessageResponseDto?> Send(SendViberMessageDto request, int retryCountParam = 6) =>
        Task.FromResult<SendViberMessageResponseDto?>(null);

    public Task<DeliveryViberMessageResponseDto?> DeliveryById(DeliveryViberMessageDto request) =>
        Task.FromResult<DeliveryViberMessageResponseDto?>(null);

    public Task<AnswerViberMessageResponseDto?> AnswersById(AnswerViberMessageDto request) =>
        Task.FromResult<AnswerViberMessageResponseDto?>(null);
}
