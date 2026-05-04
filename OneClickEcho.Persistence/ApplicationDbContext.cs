using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OneClickEcho.Domain.ApiMessageAggregate;
using OneClickEcho.Domain.ApplicationUserAggregate;
using OneClickEcho.Domain.ApplicationUserAggregate.Entities;
using OneClickEcho.Domain.CampaignAggregate;
using OneClickEcho.Domain.CampaignAggregate.Entities;
using OneClickEcho.Domain.CampaignLeadAggregate;
using OneClickEcho.Domain.CampaignLeadAggregate.Entities;
using OneClickEcho.Domain.CompanyAggregate;
using OneClickEcho.Domain.CompanyAggregate.Entities;
using OneClickEcho.Domain.GptRequestAggregate;
using OneClickEcho.Domain.LeadAggregate;
using OneClickEcho.Domain.LeadCollectionAggregate;
using OneClickEcho.Domain.LeadCollectionAggregate.Entities;
using OneClickEcho.Domain.NounCaseAggregate;
using OneClickEcho.Domain.TestMessageAggregate;
using OneClickEcho.Persistence.Configurations;

namespace OneClickEcho.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<ApplicationUser> ApplicationUsers { get; set; }

    public DbSet<ApplicationUserCompany> ApplicationUserCompanies { get; set; }

    public DbSet<Campaign> Campaigns { get; set; }

    public DbSet<CampaignLead> CampaignLeads { get; set; }

    public DbSet<Company> Companies { get; set; }

    public DbSet<GptRequest> GptRequests { get; set; }

    public DbSet<Lead> Leads { get; set; }

    public DbSet<LeadCollection> LeadCollections { get; set; }

    public DbSet<LeadAssignment> LeadAssignments { get; set; }

    public DbSet<CampaignLeadCollection> CampaignLeadCollections { get; set; }

    public DbSet<NounCase> NounCases { get; set; }

    public DbSet<ReceivedMessage> ReceivedMessages { get; set; }
    public DbSet<ViberDeliveryEvent> ViberDeliveryEvents { get; set; }

    public DbSet<Sender> Senders { get; set; }
    
    public DbSet<TestMessage> TestMessages { get; set; }
    
    public DbSet<ApiMessage> ApiMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseOpenIddict();
        base.OnModelCreating(modelBuilder);

        new ApplicationUserConfiguration().Configure(modelBuilder.Entity<ApplicationUser>());
        new ApplicationUserCompanyConfiguration().Configure(modelBuilder.Entity<ApplicationUserCompany>());
        new CampaignConfiguration().Configure(modelBuilder.Entity<Campaign>());
        new CampaignLeadConfiguration().Configure(modelBuilder.Entity<CampaignLead>());
        new CompanyConfiguration().Configure(modelBuilder.Entity<Company>());
        new GptRequestConfiguration().Configure(modelBuilder.Entity<GptRequest>());
        new LeadAssignmentConfiguration().Configure(modelBuilder.Entity<LeadAssignment>());
        new CampaignLeadCollectionConfiguration().Configure(modelBuilder.Entity<CampaignLeadCollection>());
        new LeadCollectionConfiguration().Configure(modelBuilder.Entity<LeadCollection>());
        new LeadConfiguration().Configure(modelBuilder.Entity<Lead>());
        new NounCaseConfiguration().Configure(modelBuilder.Entity<NounCase>());
        new ReceivedMessageConfiguration().Configure(modelBuilder.Entity<ReceivedMessage>());
        new ViberDeliveryEventConfiguration().Configure(modelBuilder.Entity<ViberDeliveryEvent>());
        new SenderConfiguration().Configure(modelBuilder.Entity<Sender>());
        new TestMessageConfiguration().Configure(modelBuilder.Entity<TestMessage>());
        new ApiMessageConfiguration().Configure(modelBuilder.Entity<ApiMessage>());
    }
}