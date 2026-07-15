using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OneClickEcho.Persistence.Migrations
{
    /// <inheritdoc />
    [Migration("20260715140000_CampaignLeadReportStatusMessageIdIndex")]
    public partial class CampaignLeadReportStatusMessageIdIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // May be missing if the earlier 2-column index was never applied.
            migrationBuilder.Sql("DROP INDEX IF EXISTS ix_campaign_leads_campaign_id_viber_status;");

            migrationBuilder.CreateIndex(
                name: "ix_campaign_leads_campaign_id_viber_status_message_id",
                table: "campaign_leads",
                columns: new[] { "campaign_id", "viber_status", "viber_message_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_campaign_leads_campaign_id_viber_status_message_id",
                table: "campaign_leads");

            migrationBuilder.CreateIndex(
                name: "ix_campaign_leads_campaign_id_viber_status",
                table: "campaign_leads",
                columns: new[] { "campaign_id", "viber_status" });
        }
    }
}
