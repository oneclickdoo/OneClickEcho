using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.Common.Repositories;
using OneClickEcho.Domain.Common.Shared;
using OneClickEcho.Domain.LeadAggregate.Repositories;
using OneClickEcho.Domain.LeadAggregate.ValueObjects;

namespace OneClickEcho.Application.Lead.Commands.DeleteLead
{
    public class DeleteLeadHandler(ILeadRepository leadRepository, IUnitOfWork unitOfWork)
        : ICommandHandler<DeleteLeadCommand, DeleteLeadResponse>
    {
        private readonly ILeadRepository _leadRepository = leadRepository;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<Result<DeleteLeadResponse>> Handle(DeleteLeadCommand request, CancellationToken cancellationToken)
        {
            // get lead
            Domain.LeadAggregate.Lead? lead = await _leadRepository
                .GetByIdAsync(LeadId.Create(request.LeadId), cancellationToken);

            if (lead is null)
            {
                return Result.Failure<DeleteLeadResponse>(new Error(
                    "Lead.NotFound",
                    $"The Lead with Id:\"{request.LeadId}\" does not exist."
                    ));
            }

            // delete lead
            _leadRepository.Delete(lead);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(new DeleteLeadResponse());
        }
    }
}
