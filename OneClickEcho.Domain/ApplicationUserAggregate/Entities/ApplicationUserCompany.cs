using OneClickEcho.Domain.ApplicationUserAggregate.ValueObjects;
using OneClickEcho.Domain.Common.Primitives;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;

namespace OneClickEcho.Domain.ApplicationUserAggregate.Entities;

public sealed class ApplicationUserCompany : Entity<ApplicationUserCompanyId>
{
    public ApplicationUserCompany(ApplicationUserCompanyId applicationUserCompanyId, CompanyId companyId) : base(applicationUserCompanyId)
    {
        CompanyId = companyId;
    }

    public ApplicationUserCompany(CompanyId companyId) : base(ApplicationUserCompanyId.CreateUnique())
    {
        CompanyId = companyId;
    }

    public CompanyId CompanyId { get; set; } = default!;

    // Used for EFCore
    public ApplicationUserCompany() : base(ApplicationUserCompanyId.CreateUnique()) { }
}