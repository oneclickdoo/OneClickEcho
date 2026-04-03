using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OneClickEcho.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddApiMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "api_messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    phone_number = table.Column<string>(type: "text", nullable: false),
                    message_content = table.Column<string>(type: "text", nullable: false),
                    viber_sender = table.Column<string>(type: "text", nullable: true),
                    viber_message = table.Column<string>(type: "text", nullable: true),
                    viber_media = table.Column<string>(type: "text", nullable: true),
                    viber_button_url = table.Column<string>(type: "text", nullable: true),
                    viber_button_url_title = table.Column<string>(type: "text", nullable: true),
                    viber_message_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'5000000000', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    viber_status = table.Column<short>(type: "smallint", nullable: false),
                    viber_status_description = table.Column<string>(type: "text", nullable: true),
                    sms_status = table.Column<short>(type: "smallint", nullable: false),
                    sms_status_description = table.Column<string>(type: "text", nullable: true),
                    sms_reference_id = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_api_messages", x => x.id);
                    table.ForeignKey(
                        name: "fk_api_messages_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_api_messages_company_id",
                table: "api_messages",
                column: "company_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "api_messages");
        }
    }
}
