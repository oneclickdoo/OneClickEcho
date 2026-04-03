using Microsoft.AspNetCore.Identity;
using OneClickEcho.Domain.ApplicationUserAggregate.Entities;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;

namespace OneClickEcho.Domain.ApplicationUserAggregate;

public class ApplicationUser : IdentityUser<Guid>
{
    public ICollection<ApplicationUserCompany> CompanyIds { get; set; } = [];

    public bool CompanyBelongsToUser(CompanyId companyId)
    {
        return CompanyIds.Any(c => c.CompanyId == companyId);
    }

    public void AddCompany(CompanyId companyId)
    {
        CompanyIds.Add(new ApplicationUserCompany(companyId));
    }

    public ApplicationUser() { }
}