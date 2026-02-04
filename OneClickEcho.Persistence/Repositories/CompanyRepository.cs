using Microsoft.EntityFrameworkCore;
using OneClickEcho.Domain.CampaignAggregate.Enums;
using OneClickEcho.Domain.Common.Queries;
using OneClickEcho.Domain.CompanyAggregate;
using OneClickEcho.Domain.CompanyAggregate.Repositories;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;
using OneClickEcho.Persistence.Common;
using Serilog;

namespace OneClickEcho.Persistence.Repositories;

public class CompanyRepository(ApplicationDbContext dbContext) : ICompanyRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task<Company?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<Company>()
            .FirstOrDefaultAsync(g => g.Name == name, cancellationToken);
    }

    public async Task<Company?> GetByIdAsync(CompanyId id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<Company>()
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
    }

    public async Task<List<Company>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Companies.ToListAsync(cancellationToken);
    }

    public async Task<IPagedList<Company>> GetPagedAsync(IPagedQuery query, CancellationToken cancellationToken = default)
    {
        PagedList<Company> pagedList = await PagedList<Company>
            .CreateAsync(_dbContext.Companies, query, cancellationToken);

        return pagedList;
    }

    public async Task<int> GetLeadsCountAsync(CompanyId id, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default)
    {
        return startDate != null && endDate != null
            ? await _dbContext.Leads
                .Where(l => l.CompanyId == id && l.CreatedAt >= startDate && l.CreatedAt <= endDate)
                .CountAsync(cancellationToken)
            : await _dbContext.Leads.CountAsync(l => l.CompanyId == id, cancellationToken);
    }

    public async Task<List<string>> GetCompanyImagesAsync(CompanyId id, CancellationToken cancellationToken = default)
    {
        return (await _dbContext.Campaigns
            .Where(c =>
                c.CompanyId == id &&
                c.ViberMedia != null &&
                c.Status == CampaignStatus.Done)
            .Select(c => c.ViberMedia)
            .Distinct()
            .ToListAsync(cancellationToken))!;
    }

    public async Task<AnalyticsResults> GetAnalyticsResultsAsync(CompanyId id, DateTime? startDate, DateTime? endDate,
        CancellationToken cancellationToken = default)
    {
        if (startDate is null) startDate = DateTime.UtcNow.AddYears(-1000);
        if (endDate is null) endDate = DateTime.UtcNow.AddYears(1000);

        try
        {
            AnalyticsResults? result = await _dbContext.Database
                .SqlQuery<AnalyticsResults>($@"
                SELECT
                    -- If not none or received, then its considered as sent
                    COALESCE(SUM(CASE WHEN l.viber_status NOT IN (0, 1) THEN 1 ELSE 0 END), 0) AS viber_total_sent,
                    COALESCE(SUM(CASE WHEN l.viber_status IN (3, 4, 7) THEN 1 ELSE 0 END), 0) AS viber_delivered,
                    COALESCE(SUM(CASE WHEN l.viber_status = 5 THEN 1 ELSE 0 END), 0) AS viber_undelivered,
                    COALESCE(SUM(CASE WHEN l.viber_status = 6 THEN 1 ELSE 0 END), 0) AS viber_expired,
                    COALESCE(SUM(CASE WHEN l.viber_status IN (4, 7) THEN 1 ELSE 0 END), 0) AS viber_seen,
                    COALESCE(SUM(CASE WHEN l.viber_status = 7 THEN 1 ELSE 0 END), 0) AS viber_clicked,
                    COALESCE(SUM(CASE WHEN l.sms_status != 0 THEN 1 ELSE 0 END), 0) AS sms_total_sent,
                    COALESCE(SUM(CASE WHEN l.sms_status = 1 THEN 1 ELSE 0 END), 0) AS sms_delivered,
                    COALESCE(SUM(CASE WHEN l.sms_status = 2 THEN 1 ELSE 0 END), 0) AS sms_failed,
                    COALESCE(COUNT(DISTINCT le.phone_number), 0) AS unique_phone_numbers,
                    COALESCE((
                        SELECT COUNT(DISTINCT le1.phone_number)
                        FROM campaign_leads l1
                            JOIN leads le1 ON le1.id = l1.lead_id
                            JOIN campaigns c1 ON c1.id = l1.campaign_id
                        WHERE c1.company_id = {id.Value}
                            AND c1.sending_datetime >= {startDate}
                            AND c1.sending_datetime <= {endDate}
                            AND c1.status = 4
                            AND le1.is_unsubscribed = true
                    ), 0) AS total_unsubscribed,
                    COALESCE((
                        SELECT COUNT(*)
                        FROM test_messages t1
                        WHERE t1.company_id = {id.Value}
                            AND t1.created_at >= {startDate}
                            AND t1.created_at <= {endDate}
                            AND t1.is_delivered = true
                            AND t1.is_viber = true
                    ), 0) AS number_of_tests_viber,
                    COALESCE((
                        SELECT COUNT(*)
                        FROM test_messages t1
                        WHERE t1.company_id = {id.Value}
                            AND t1.created_at >= {startDate}
                            AND t1.created_at <= {endDate}
                            AND t1.is_delivered = true
                            AND t1.is_viber = true
                            AND t1.is_clicked = true
                    ), 0) AS number_of_tests_viber_clicked,
                    COALESCE((
                        SELECT COUNT(*)
                        FROM test_messages t1
                        WHERE t1.company_id = {id.Value}
                            AND t1.created_at >= {startDate}
                            AND t1.created_at <= {endDate}
                            AND t1.is_delivered = true
                            AND t1.is_viber = false
                    ), 0) AS number_of_tests_sms,
                    COALESCE((
                        SELECT COUNT(*)
                        FROM api_messages api_1
                        WHERE api_1.company_id = {id.Value}
                            AND api_1.created_at >= {startDate}
                            AND api_1.created_at <= {endDate}
                            AND api_1.message_type = 1
                            AND api_1.viber_status IN (3,4,7)
                    ), 0) AS number_of_api_viber,
                    COALESCE((
                        SELECT COUNT(*)
                        FROM api_messages api_1
                        WHERE api_1.company_id = {id.Value}
                            AND api_1.created_at >= {startDate}
                            AND api_1.created_at <= {endDate}
                            AND api_1.message_type = 1
                            AND api_1.viber_status = 7
                    ), 0) AS number_of_api_viber_clicked,
                    COALESCE((
                        SELECT COUNT(*)
                        FROM api_messages api_1
                        WHERE api_1.company_id = {id.Value}
                            AND api_1.created_at >= {startDate}
                            AND api_1.created_at <= {endDate}
                            AND api_1.message_type = 2
                            AND api_1.sms_status = 1
                    ), 0) AS number_of_api_sms,
                    COALESCE(COUNT(DISTINCT c.id), 0) AS number_of_campaigns
                FROM campaign_leads l
                JOIN leads le ON le.id = l.lead_id
                JOIN campaigns c ON c.id = l.campaign_id
                WHERE c.company_id = {id.Value}
                    AND c.sending_datetime >= {startDate}
                    AND c.sending_datetime <= {endDate}
                    AND c.status = 4
            ")
                .FirstOrDefaultAsync(cancellationToken);

            return result ?? new AnalyticsResults();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while retrieving analytics results for CompanyId: {CompanyId}", id.Value);
            return new AnalyticsResults();
        }
    }

    public async Task<bool> CompanyApiValidation(CompanyId id, string apiPassword, CancellationToken cancellationToken = default)
    {
        var company = await _dbContext.Companies.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        return company?.ApiPassword == apiPassword;
    }

    public void Add(Company company)
    {
        _dbContext.Set<Company>().Add(company);
    }

    public void Delete(Company company)
    {
        _dbContext.Set<Company>().Remove(company);
    }
}