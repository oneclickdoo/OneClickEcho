using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.Common.Shared;
using OneClickEcho.Domain.CompanyAggregate.Repositories;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;

namespace OneClickEcho.Application.Sender.Queries.GetSenders
{
    public class GetSendersHandler(ICompanyRepository companyRepository, ISenderRepository senderRepository)
        : IQueryHandler<GetSendersQuery, List<GetSendersResponse>>
    {
        private readonly ICompanyRepository _companyRepository = companyRepository;
        private readonly ISenderRepository _senderRepository = senderRepository;

        public async Task<Result<List<GetSendersResponse>>> Handle(GetSendersQuery query, CancellationToken cancellationToken)
        {
            // get company
            Domain.CompanyAggregate.Company? company = await _companyRepository
                .GetByIdAsync(CompanyId.Create(query.CompanyId), cancellationToken);

            if (company is null)
            {
                return Result.Failure<List<GetSendersResponse>>(new Error(
                    "Company.NotFound",
                    $"The Company with Id:\"{query.CompanyId}\" does not exist."
                    ));
            }

            // get senders
            List<Domain.CompanyAggregate.Entities.Sender> senders = await _senderRepository
                .GetAllByCompanyIdAsync(CompanyId.Create(query.CompanyId), cancellationToken);

            List<GetSendersResponse> result = [];

            foreach (Domain.CompanyAggregate.Entities.Sender sender in senders)
            {
                result.Add(new GetSendersResponse(
                    sender.Id.Value,
                    sender.Name,
                    sender.Type));
            }

            return result;
        }
    }
}
