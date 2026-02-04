using System.Security.Claims;

namespace OneClickEcho.App.Infrastructure.Utils;

public class UpdateFilter
{
    public static string? WithCompanyId(ClaimsPrincipal user, string? filter)
    {
        var newFilter = filter;
        var companyIdString = user.Claims.FirstOrDefault(c => c.Type == "CompanyIds")?.Value;

        if (!string.IsNullOrEmpty(companyIdString))
        {
            var companyIds = System.Text.Json.JsonSerializer.Deserialize<List<string>>(companyIdString);
            var companyGuids = companyIds?.Select(id => Guid.Parse(id)).ToList();
            if (companyGuids != null && companyGuids.Any())
            {
                var firstCompanyId = companyGuids.First();
                if (newFilter is null)
                {
                    newFilter = $"CompanyId eq {firstCompanyId}";
                }
                else if (!newFilter.Contains("CompanyId"))
                {
                    newFilter += $" and CompanyId eq {firstCompanyId}";
                }
            }
        }
        
        return newFilter;
    }
}