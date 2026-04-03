using FluentValidation;

namespace OneClickEcho.Application.Scheduling.Commands.EnqueueApiMessages;

public class EnqueueApiMessagesValidator: AbstractValidator<EnqueueApiMessagesCommand>
{
    public EnqueueApiMessagesValidator() {}
}