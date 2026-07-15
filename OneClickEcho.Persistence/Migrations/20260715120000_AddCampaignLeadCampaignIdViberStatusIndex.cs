using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OneClickEcho.Persistence.Migrations
{
    /// <inheritdoc />
    [Migration("20260715120000_AddCampaignLeadCampaignIdViberStatusIndex")]
    public partial class AddCampaignLeadCampaignIdViberStatusIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_campaign_leads_campaign_id_viber_status",
                table: "campaign_leads",
                columns: new[] { "campaign_id", "viber_status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_campaign_leads_campaign_id_viber_status",
                table: "campaign_leads");
        }
    }
}
