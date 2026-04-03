namespace OneClickEcho.Infrastructure.Services.GptService;

public class ChatCompletionMessageDto
{
    public string Content { get; set; } = default!;
}

public class ChatCompletionChoiceDto
{
    public ChatCompletionMessageDto Message { get; set; } = default!;
}

public class ChatCompletionResponseDto
{
    public List<ChatCompletionChoiceDto> Choices { get; set; } = default!;
}