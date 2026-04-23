using OneClickEcho.Domain.CampaignAggregate;
using OneClickEcho.Persistence.Common.Filtering;

namespace OneClickEcho.Persistence.Tests;

/// <summary>
/// Guards the campaign list OData-style filter path used by <c>PagedList&lt;Campaign&gt;</c>:
/// filters merged with client date clauses must not use leading <c>(</c> before the first property name
/// (see API <c>CampaignTenantFilter.BuildCampaignsListFilter</c>).
/// </summary>
public sealed class CampaignListFilterParsingTests
{
    private static readonly Guid SampleCompanyId = Guid.Parse("075fe381-fda7-4d94-aaf1-5d72ec07a2eb");

    [Fact]
    public void CompanyId_and_CreatedAt_range_without_parentheses_compiles()
    {
        string filter =
            $"CompanyId eq {SampleCompanyId} and CreatedAt ge 2025-12-31T23:00:00.000Z and CreatedAt le 2027-01-01T22:59:59.999Z";

        var lambda = Filtering<Campaign>.ApplyFilter(filter);

        Assert.NotNull(lambda.Compile());
    }

    [Fact]
    public void Multi_company_or_tenant_without_outer_parentheses_compiles()
    {
        Guid g1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
        Guid g2 = Guid.Parse("22222222-2222-2222-2222-222222222222");
        string filter =
            $"CompanyId eq {g1} or CompanyId eq {g2} and CreatedAt ge 2025-12-31T23:00:00.000Z and CreatedAt le 2027-01-01T22:59:59.999Z";

        var lambda = Filtering<Campaign>.ApplyFilter(filter);

        Assert.NotNull(lambda.Compile());
    }

    [Fact]
    public void Parenthesized_tenant_and_rest_throws_so_API_must_not_emit_this_shape()
    {
        string filter =
            $"(CompanyId eq {SampleCompanyId}) and (CreatedAt ge 2025-12-31T23:00:00.000Z and CreatedAt le 2027-01-01T22:59:59.999Z)";

        Assert.ThrowsAny<ArgumentException>(() => Filtering<Campaign>.ApplyFilter(filter));
    }
}
