using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OneClickEcho.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CampaignSmsSender : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "viber_title",
                table: "campaigns",
                newName: "viber_sender");

            migrationBuilder.AddColumn<string>(
                name: "sms_sender",
                table: "campaigns",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "sms_sender",
                table: "campaigns");

            migrationBuilder.RenameColumn(
                name: "viber_sender",
                table: "campaigns",
                newName: "viber_title");
        }
    }
}
