using OneClickEcho.Application.Common.Services.ViberService.Response.Common;

namespace OneClickEcho.Application.Common.Services.ViberService.Response;

public class AnswerViberMessageResponseDto
{
    public required List<ViberAnswer> ViberAnswers { get; set; }
}