using OneClickEcho.Application.Common.Messaging;

namespace OneClickEcho.Application.Company.Queries.ExportCompanyLeads
{
    public record ExportCompanyLeadsQuery(
        Guid CompanyId,
        Guid? CollectionId = null
    ) : IQuery<ExportCompanyLeadsResponse>;
}
