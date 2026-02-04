using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.Common.Repositories;
using OneClickEcho.Domain.Common.Shared;
using OneClickEcho.Domain.CompanyAggregate.Repositories;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;

namespace OneClickEcho.Application.Sender.Commands.DeleteSender
{
    public class DeleteSenderHandler(ISenderRepository senderRepository, IUnitOfWork unitOfWork)
        : ICommandHandler<DeleteSenderCommand, DeleteSenderResponse>
    {
        private readonly ISenderRepository _senderRepository = senderRepository;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<Result<DeleteSenderResponse>> Handle(DeleteSenderCommand command, CancellationToken cancellationToken)
        {
            // get sender
            Domain.CompanyAggregate.Entities.Sender? sender = await _senderRepository
                .GetByIdAsync(SenderId.Create(command.Id), cancellationToken);

            if (sender is null)
            {
                return Result.Failure<DeleteSenderResponse>(new Error("NotFound", "Sender not found"));
            }

            // delete sender
            _senderRepository.Delete(sender);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(new DeleteSenderResponse());
        }
    }
}
