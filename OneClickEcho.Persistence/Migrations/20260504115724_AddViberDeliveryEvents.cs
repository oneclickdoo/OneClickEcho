using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OneClickEcho.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddViberDeliveryEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "viber_delivery_events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    campaign_lead_id = table.Column<Guid>(type: "uuid", nullable: false),
                    viber_message_id = table.Column<long>(type: "bigint", nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    sub_status = table.Column<int>(type: "integer", nullable: false),
                    click_count = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_viber_delivery_events", x => x.id);
                    table.ForeignKey(
                        name: "fk_viber_delivery_events_campaign_leads_campaign_lead_id",
                        column: x => x.campaign_lead_id,
                        principalTable: "campaign_leads",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_viber_delivery_events_campaign_lead_id",
                table: "viber_delivery_events",
                column: "campaign_lead_id");

            migrationBuilder.CreateIndex(
                name: "ix_viber_delivery_events_viber_message_id",
                table: "viber_delivery_events",
                column: "viber_message_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "viber_delivery_events");
        }
    }
}
