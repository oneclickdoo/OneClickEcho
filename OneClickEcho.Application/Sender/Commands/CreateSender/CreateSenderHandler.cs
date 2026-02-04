using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.Common.Repositories;
using OneClickEcho.Domain.Common.Shared;
using OneClickEcho.Domain.CompanyAggregate.Repositories;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;

namespace OneClickEcho.Application.Sender.Commands.CreateSender
{
    public class CreateSenderHandler(ICompanyRepository companyRepository, ISenderRepository senderRepository,
        IUnitOfWork unitOfWork) : ICommandHandler<CreateSenderCommand, CreateSenderResponse>
    {
        private readonly ICompanyRepository _companyRepository = companyRepository;
        private readonly ISenderRepository _senderRepository = senderRepository;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<Result<CreateSenderResponse>> Handle(CreateSenderCommand command, CancellationToken cancellationToken)
        {
            // get company
            Domain.CompanyAggregate.Company? company = await _companyRepository
                .GetByIdAsync(CompanyId.Create(command.CompanyId), cancellationToken);

            if (company is null)
            {
                return Result.Failure<CreateSenderResponse>(Error.NullValue);
            }

            // create sender
            Domain.CompanyAggregate.Entities.Sender sender = new(company.Id, command.Name, command.Type);

            _senderRepository.Add(sender);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new CreateSenderResponse(sender.Id.Value);
        }
    }
}
