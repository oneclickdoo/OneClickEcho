using FluentValidation;

namespace OneClickEcho.Application.Scheduling.Commands.ExpirePendingMessages;

public class ExpirePendingMessagesValidator : AbstractValidator<ExpirePendingMessagesCommand>
{
    public ExpirePendingMessagesValidator()
    {
        
    }
}