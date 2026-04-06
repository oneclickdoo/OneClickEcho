using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.Common.Shared;
using OneClickEcho.Domain.CompanyAggregate.Repositories;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;
using OneClickEcho.Domain.LeadAggregate.Repositories;
using System.Text;
using OneClickEcho.Domain.LeadCollectionAggregate.ValueObjects;

namespace OneClickEcho.Application.Company.Queries.ExportCompanyLeads
{
    public class ExportCompanyLeadsHandler(ICompanyRepository companyRepository, ILeadRepository leadRepository)
        : IQueryHandler<ExportCompanyLeadsQuery, ExportCompanyLeadsResponse>
    {
        private readonly ICompanyRepository _companyRepository = companyRepository;
        private readonly ILeadRepository _leadRepository = leadRepository;

        public async Task<Result<ExportCompanyLeadsResponse>> Handle(ExportCompanyLeadsQuery query, CancellationToken cancellationToken)
        {
            // get company
            Domain.CompanyAggregate.Company? company = await _companyRepository
                .GetByIdAsync(CompanyId.Create(query.CompanyId), cancellationToken);

            if (company is null)
            {
                return Result.Failure<ExportCompanyLeadsResponse>(new Error(
                    "Company.NotFound",
                    $"The Company with Id:\"{query.CompanyId}\" does not exist."
                    ));
            }

            List<Domain.LeadAggregate.Lead> leads;
            // get company leads
            if (query.CollectionId is not null)
            {
                leads = await _leadRepository.GetAllByCollectionIdAsync(LeadCollectionId.Create(query.CollectionId.Value), cancellationToken);
            }
            else
            {
                leads = await _leadRepository.GetAllByCompanyId(company.Id, cancellationToken);
            }

            if (query.BlacklistedOnly)
            {
                leads = leads.Where(l => l.IsBlacklisted).ToList();
            }

            if (leads.Count == 0 && !query.BlacklistedOnly)
            {
                return Result.Failure<ExportCompanyLeadsResponse>(new Error(
                    "Leads.NotFound",
                    $"The Company with Id:\"{query.CompanyId}\" does not have any leads."
                    ));
            }

            // create CSV content
            StringBuilder csvContent = new();

            // append header row
            csvContent.AppendLine("PhoneNumber,FirstName,LastName,Gender,Email,DateOfBirth,City,State,Country");

            // append data rows
            foreach (Domain.LeadAggregate.Lead lead in leads)
            {
                csvContent.AppendLine($"{lead.PhoneNumber},{lead.FirstName},{lead.LastName},{lead.Gender}," +
                    $"{lead.Email},{lead.DateOfBirth},{lead.City},{lead.State},{lead.Country}");
            }

            // encode CSV content
            byte[] fileBytes = Encoding.UTF8.GetBytes(csvContent.ToString());

            return new ExportCompanyLeadsResponse(fileBytes);
        }
    }
}
