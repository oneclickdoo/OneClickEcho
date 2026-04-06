using OneClickEcho.Domain.Common.Queries;
using OneClickEcho.Domain.Common.Repositories;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;

namespace OneClickEcho.Domain.CompanyAggregate.Repositories;

public interface ICompanyRepository : IRepository<Company, CompanyId>
{
    Task<Company?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    Task<Company?> GetByIdAsync(CompanyId id, CancellationToken cancellationToken = default);

    Task<List<Company>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IPagedList<Company>> GetPagedAsync(IPagedQuery query, CancellationToken cancellationToken = default);

    Task<int> GetLeadsCountAsync(CompanyId id, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default);
    
    Task<List<string>> GetCompanyImagesAsync(CompanyId id, CancellationToken cancellationToken = default);
    
    Task<AnalyticsResults> GetAnalyticsResultsAsync(CompanyId id, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default);
    
    Task<bool> CompanyApiValidation(CompanyId id, string apiPassword, CancellationToken cancellationToken = default);

    void Add(Company company);

    void Delete(Company company);
}

/// <summary>
/// Property order must match the column order in the analytics SQL query in persistence (CompanyRepository);
/// EF Core <c>SqlQuery&lt;T&gt;</c> maps result columns to properties by ordinal.
/// </summary>
public class AnalyticsResults
{
    public int ViberTotalSent { get; set; } = 0;
    public int ViberDelivered { get; set; } = 0;
    public int ViberUndelivered { get; set; } = 0;
    public int ViberExpired { get; set; } = 0;
    public int ViberSeen { get; set; } = 0;
    public int ViberClicked { get; set; } = 0;
    public int SmsTotalSent { get; set; } = 0;
    public int SmsDelivered { get; set; } = 0;
    public int SmsFailed { get; set; } = 0;
    public int UniquePhoneNumbers { get; set; } = 0;
    public int TotalUnsubscribed { get; set; } = 0;
    public int NumberOfTestsViber { get; set; } = 0;
    public int NumberOfTestsViberClicked { get; set; } = 0;
    public int NumberOfTestsSms { get; set; } = 0;
    public int NumberOfApiViber { get; set; } = 0;
    public int NumberOfApiViberClicked { get; set; } = 0;
    public int NumberOfApiSms { get; set; } = 0;
    public int NumberOfCampaigns { get; set; } = 0;

    /// <summary>All campaign leads in scope (every row), including status None (not yet sent to Viber).</summary>
    public int ViberTotalLeads { get; set; } = 0;

    /// <summary>Viber status None (0) only — not handed to Comtrade / outbound gateway yet.</summary>
    public int ViberNotSent { get; set; } = 0;

    /// <summary>Viber status Received (1) — primljeno od provajdera, još nije isporučeno na uređaj.</summary>
    public int ViberReceived { get; set; } = 0;

    /// <summary>Viber status Delivered (3) only — isporučeno na uređaj (bez Seen/Clicked u istom broju).</summary>
    public int ViberDeliveredOnly { get; set; } = 0;
}
