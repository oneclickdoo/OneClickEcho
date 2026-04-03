using OneClickEcho.Application.Common.Services.ViberService.Request.Common;

namespace OneClickEcho.Application.Common.Services.ViberService.Request;

public class AnswerViberMessageDto
{
    public required ViberUserCredentials UserCredentials { get; set; }

    public List<long> IDList { get; set; } = null!;
}