using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OneClickEcho.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddComtradeViberGifCarouselTemplateSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "viber_carousel_json",
                table: "campaigns",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "viber_image_ratio",
                table: "campaigns",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "viber_survey_ui_type",
                table: "campaigns",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "viber_template_id",
                table: "campaigns",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "viber_template_params_json",
                table: "campaigns",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "viber_carousel_json",
                table: "api_messages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "viber_image_format",
                table: "api_messages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "viber_image_ratio",
                table: "api_messages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "viber_survey_options_json",
                table: "api_messages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "viber_survey_ui_type",
                table: "api_messages",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "viber_template_id",
                table: "api_messages",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "viber_template_params_json",
                table: "api_messages",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "viber_templates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    display = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    language = table.Column<string>(type: "text", nullable: false),
                    body = table.Column<string>(type: "text", nullable: false),
                    tag = table.Column<string>(type: "text", nullable: true),
                    category = table.Column<string>(type: "text", nullable: false),
                    comtrade_id = table.Column<int>(type: "integer", nullable: false),
                    provider_template_id = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_viber_templates", x => x.id);
                    table.ForeignKey(
                        name: "fk_viber_templates_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "viber_sessions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sender = table.Column<string>(type: "text", nullable: false),
                    phone_number = table.Column<string>(type: "text", nullable: false),
                    comtrade_session_id = table.Column<string>(type: "text", nullable: true),
                    started_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    last_inbound_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    total_outbound = table.Column<int>(type: "integer", nullable: false),
                    consecutive_outbound = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_viber_sessions", x => x.id);
                    table.ForeignKey(
                        name: "fk_viber_sessions_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "viber_session_messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    direction = table.Column<int>(type: "integer", nullable: false),
                    comtrade_message_type = table.Column<int>(type: "integer", nullable: false),
                    body = table.Column<string>(type: "text", nullable: true),
                    media = table.Column<string>(type: "text", nullable: true),
                    survey_options_json = table.Column<string>(type: "text", nullable: true),
                    viber_message_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'8000000000', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    message_token = table.Column<string>(type: "text", nullable: true),
                    is_sent = table.Column<bool>(type: "boolean", nullable: false),
                    billed_as_promotion = table.Column<bool>(type: "boolean", nullable: false),
                    send_error = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_viber_session_messages", x => x.id);
                    table.ForeignKey(
                        name: "fk_viber_session_messages_viber_sessions_session_id",
                        column: x => x.session_id,
                        principalTable: "viber_sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_viber_templates_company_id_comtrade_id",
                table: "viber_templates",
                columns: new[] { "company_id", "comtrade_id" });

            migrationBuilder.CreateIndex(
                name: "ix_viber_sessions_company_id_sender_phone_number",
                table: "viber_sessions",
                columns: new[] { "company_id", "sender", "phone_number" });

            migrationBuilder.CreateIndex(
                name: "ix_viber_session_messages_session_id",
                table: "viber_session_messages",
                column: "session_id");

            migrationBuilder.CreateIndex(
                name: "ix_viber_session_messages_viber_message_id",
                table: "viber_session_messages",
                column: "viber_message_id");

            migrationBuilder.CreateIndex(
                name: "ix_viber_session_messages_message_token",
                table: "viber_session_messages",
                column: "message_token");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "viber_session_messages");
            migrationBuilder.DropTable(name: "viber_sessions");
            migrationBuilder.DropTable(name: "viber_templates");

            migrationBuilder.DropColumn(name: "viber_carousel_json", table: "campaigns");
            migrationBuilder.DropColumn(name: "viber_image_ratio", table: "campaigns");
            migrationBuilder.DropColumn(name: "viber_survey_ui_type", table: "campaigns");
            migrationBuilder.DropColumn(name: "viber_template_id", table: "campaigns");
            migrationBuilder.DropColumn(name: "viber_template_params_json", table: "campaigns");

            migrationBuilder.DropColumn(name: "viber_carousel_json", table: "api_messages");
            migrationBuilder.DropColumn(name: "viber_image_format", table: "api_messages");
            migrationBuilder.DropColumn(name: "viber_image_ratio", table: "api_messages");
            migrationBuilder.DropColumn(name: "viber_survey_options_json", table: "api_messages");
            migrationBuilder.DropColumn(name: "viber_survey_ui_type", table: "api_messages");
            migrationBuilder.DropColumn(name: "viber_template_id", table: "api_messages");
            migrationBuilder.DropColumn(name: "viber_template_params_json", table: "api_messages");
        }
    }
}
