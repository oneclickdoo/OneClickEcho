using System.Security.Claims;
using System.Text.Json;

namespace OneClickEcho.App.Infrastructure.Utils;

/// <summary>
/// Builds OData-style filters for campaign list queries and validates tenant access.
/// Administrator: must pass <paramref name="companyIdFromQuery"/> — only that company's campaigns (client Filter CompanyId is ignored).
/// Other users: tenant from JWT; optional <paramref name="companyIdFromQuery"/> narrows to one allowed company.
/// </summary>
public static class CampaignTenantFilter
{
    public static IReadOnlyList<Guid> GetUserCompanyIds(ClaimsPrincipal user)
    {
        string? raw = user.Claims.FirstOrDefault(c => c.Type == "CompanyIds")?.Value;
        if (string.IsNullOrEmpty(raw))
        {
            return Array.Empty<Guid>();
        }

        try
        {
            List<string>? ids = JsonSerializer.Deserialize<List<string>>(raw);
            if (ids is null || ids.Count == 0)
            {
                return Array.Empty<Guid>();
            }

            List<Guid> result = [];
            foreach (string id in ids)
            {
                if (Guid.TryParse(id, out Guid g))
                {
                    result.Add(g);
                }
            }

            return result;
        }
        catch
        {
            return Array.Empty<Guid>();
        }
    }

    public static bool UserMayAccessCampaignCompany(ClaimsPrincipal user, Guid campaignCompanyId)
    {
        if (user.IsInRole("Administrator"))
        {
            return true;
        }

        return GetUserCompanyIds(user).Contains(campaignCompanyId);
    }

    /// <param name="companyIdFromQuery">Scoped company from <c>?CompanyId=</c> (dashboard). Required for admins (validated in controller).</param>
    public static string? BuildCampaignsListFilter(ClaimsPrincipal user, string? clientFilter, Guid? companyIdFromQuery)
    {
        if (user.IsInRole("Administrator"))
        {
            Guid scope = companyIdFromQuery ?? Guid.Empty;
            if (scope == Guid.Empty)
            {
                return $"CompanyId eq {Guid.Empty}";
            }

            string? rest = StripCompanyIdClauses(clientFilter);
            string tenant = $"CompanyId eq {scope}";
            return string.IsNullOrWhiteSpace(rest) ? tenant : $"({tenant}) and ({rest})";
        }

        IReadOnlyList<Guid> allowed = GetUserCompanyIds(user);
        if (allowed.Count == 0)
        {
            return $"CompanyId eq {Guid.Empty}";
        }

        string tenantClause;
        if (companyIdFromQuery is { } picked && picked != Guid.Empty)
        {
            // Caller must ensure picked is in allowed for non-administrators (see CampaignController).
            tenantClause = $"CompanyId eq {picked}";
        }
        else
        {
            tenantClause = allowed.Count == 1
                ? $"CompanyId eq {allowed[0]}"
                : "(" + string.Join(" or ", allowed.Select(g => $"CompanyId eq {g}")) + ")";
        }

        string? restNonAdmin = StripCompanyIdClauses(clientFilter);
        if (string.IsNullOrWhiteSpace(restNonAdmin))
        {
            return tenantClause;
        }

        return $"({tenantClause}) and ({restNonAdmin})";
    }

    private static string? StripCompanyIdClauses(string? filter)
    {
        if (string.IsNullOrWhiteSpace(filter))
        {
            return null;
        }

        string[] parts = filter.Split(" and ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        IEnumerable<string> kept = parts.Where(p =>
            !p.StartsWith("CompanyId ", StringComparison.OrdinalIgnoreCase));

        List<string> list = kept.ToList();
        return list.Count == 0 ? null : string.Join(" and ", list);
    }
}
