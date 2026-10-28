namespace OneClickEcho.Application.Common.Services.ViberService.Response;

public class ComtradeViberTemplateDto
{
    public string? Body { get; set; }
    public string? Category { get; set; }
    public string? Display { get; set; }
    public int Id { get; set; }
    public string? Language { get; set; }
    public string? Name { get; set; }
    public string? Status { get; set; }
    public string? Tag { get; set; }
    public string? ViberTemplateId { get; set; }
}

public class CreateViberTemplateResponseDto
{
    public bool Success { get; set; }
    public ComtradeViberTemplateDto? Template { get; set; }
}

public class ViberTemplateListResponseDto
{
    public bool Success { get; set; }
    public List<ComtradeViberTemplateDto>? Templates { get; set; }
}

public class DeleteViberTemplateResponseDto
{
    public bool Success { get; set; }
}
