using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OneClickEcho.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCampaignLeadCollections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "campaign_lead_collections",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    campaign_id = table.Column<Guid>(type: "uuid", nullable: false),
                    lead_collection_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_campaign_lead_collections", x => x.id);
                    table.ForeignKey(
                        name: "fk_campaign_lead_collections_campaigns_campaign_id",
                        column: x => x.campaign_id,
                        principalTable: "campaigns",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_campaign_lead_collections_lead_collections_lead_collection_",
                        column: x => x.lead_collection_id,
                        principalTable: "lead_collections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_campaign_lead_collections_campaign_id",
                table: "campaign_lead_collections",
                column: "campaign_id");

            migrationBuilder.CreateIndex(
                name: "ix_campaign_lead_collections_lead_collection_id",
                table: "campaign_lead_collections",
                column: "lead_collection_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "campaign_lead_collections");
        }
    }
}
