using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.Common.Repositories;
using OneClickEcho.Domain.Common.Shared;
using OneClickEcho.Domain.CompanyAggregate.Enums;
using OneClickEcho.Domain.CompanyAggregate.Repositories;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;

namespace OneClickEcho.Application.Sender.Commands.SetSenderTwoWay
{
    public class SetSenderTwoWayHandler(ISenderRepository senderRepository, IUnitOfWork unitOfWork)
        : ICommandHandler<SetSenderTwoWayCommand, SetSenderTwoWayResponse>
    {
        public async Task<Result<SetSenderTwoWayResponse>> Handle(
            SetSenderTwoWayCommand command,
            CancellationToken cancellationToken)
        {
            Domain.CompanyAggregate.Entities.Sender? sender = await senderRepository
                .GetByIdAsync(SenderId.Create(command.Id), cancellationToken);

            if (sender is null)
            {
                return Result.Failure<SetSenderTwoWayResponse>(new Error("NotFound", "Sender not found"));
            }

            if (sender.Type != SenderType.Viber)
            {
                return Result.Failure<SetSenderTwoWayResponse>(
                    new Error("Sender.NotViber", "Only a Viber sender can be opened for two-way replies."));
            }

            sender.IsTwoWay = command.IsTwoWay;
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(new SetSenderTwoWayResponse(sender.Id.Value, sender.IsTwoWay));
        }
    }
}
