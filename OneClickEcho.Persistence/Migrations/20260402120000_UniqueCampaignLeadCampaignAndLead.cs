using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OneClickEcho.Persistence.Migrations
{
    /// <inheritdoc />
    [Migration("20260402120000_UniqueCampaignLeadCampaignAndLead")]
    public partial class UniqueCampaignLeadCampaignAndLead : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_campaign_leads_campaign_id_lead_id_unique",
                table: "campaign_leads",
                columns: new[] { "campaign_id", "lead_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_campaign_leads_campaign_id_lead_id_unique",
                table: "campaign_leads");
        }
    }
}
