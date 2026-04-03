using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OneClickEcho.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CampaignViber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "viber_url_title",
                table: "campaigns",
                newName: "viber_button_url_title");

            migrationBuilder.RenameColumn(
                name: "viber_url",
                table: "campaigns",
                newName: "viber_button_url");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "viber_button_url_title",
                table: "campaigns",
                newName: "viber_url_title");

            migrationBuilder.RenameColumn(
                name: "viber_button_url",
                table: "campaigns",
                newName: "viber_url");
        }
    }
}
