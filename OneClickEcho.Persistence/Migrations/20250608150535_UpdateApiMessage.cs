using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OneClickEcho.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateApiMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "viber_message",
                table: "api_messages");

            migrationBuilder.RenameColumn(
                name: "message_content",
                table: "api_messages",
                newName: "message");

            migrationBuilder.AddColumn<bool>(
                name: "has_sms_fallback",
                table: "api_messages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<short>(
                name: "message_type",
                table: "api_messages",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "has_sms_fallback",
                table: "api_messages");

            migrationBuilder.DropColumn(
                name: "message_type",
                table: "api_messages");

            migrationBuilder.RenameColumn(
                name: "message",
                table: "api_messages",
                newName: "message_content");

            migrationBuilder.AddColumn<string>(
                name: "viber_message",
                table: "api_messages",
                type: "text",
                nullable: true);
        }
    }
}
