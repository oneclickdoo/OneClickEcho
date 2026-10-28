using OneClickEcho.Application.Common.Services.ViberService.Request;
using OneClickEcho.Application.Common.Services.ViberService.Response;

namespace OneClickEcho.Application.Common.Services.ViberService
{
    public interface IViberService
    {
        public Task<SendViberMessageResponseDto?> Send(SendViberMessageDto request, int retryCountParam = 6);

        public Task<DeliveryViberMessageResponseDto?> DeliveryById(DeliveryViberMessageDto request);

        public Task<AnswerViberMessageResponseDto?> AnswersById(AnswerViberMessageDto request);

        public Task<CreateViberTemplateResponseDto?> CreateTemplate(CreateViberTemplateRequestDto request);

        public Task<DeleteViberTemplateResponseDto?> DeleteTemplate(ViberTemplateIdRequestDto request);

        public Task<ViberTemplateListResponseDto?> GetTemplates(ViberTemplatesListRequestDto request);

        public Task<ViberTemplateListResponseDto?> GetTemplateById(ViberTemplateIdRequestDto request);

        public Task<ViberTemplateListResponseDto?> RefreshTemplate(ViberTemplateIdRequestDto request);
    }
}
