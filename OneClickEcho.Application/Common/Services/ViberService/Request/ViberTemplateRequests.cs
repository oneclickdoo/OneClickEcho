using OneClickEcho.Application.Common.Services.ViberService.Request.Common;

namespace OneClickEcho.Application.Common.Services.ViberService.Request;

public class CreateViberTemplateRequestDto
{
    public required ViberUserCredentials UserCredentials { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Display { get; set; } = string.Empty;
    public string Category { get; set; } = "Transactional";
    public string Language { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? Tag { get; set; }
}

public class ViberTemplateIdRequestDto
{
    public required ViberUserCredentials UserCredentials { get; set; }
    public int Id { get; set; }
}

public class ViberTemplatesListRequestDto
{
    public required ViberUserCredentials UserCredentials { get; set; }
}
