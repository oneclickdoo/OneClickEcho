using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OneClickEcho.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSmsMessageSenderAndViberValidity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "sms_message",
                table: "api_messages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "sms_sender",
                table: "api_messages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "viber_validity",
                table: "api_messages",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "sms_message",
                table: "api_messages");

            migrationBuilder.DropColumn(
                name: "sms_sender",
                table: "api_messages");

            migrationBuilder.DropColumn(
                name: "viber_validity",
                table: "api_messages");
        }
    }
}
