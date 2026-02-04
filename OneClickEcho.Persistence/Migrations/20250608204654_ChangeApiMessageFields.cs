using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OneClickEcho.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChangeApiMessageFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "viber_sender",
                table: "api_messages",
                newName: "sender");

            migrationBuilder.AddColumn<bool>(
                name: "is_sent",
                table: "api_messages",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_sent",
                table: "api_messages");

            migrationBuilder.RenameColumn(
                name: "sender",
                table: "api_messages",
                newName: "viber_sender");
        }
    }
}
