using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OneClickEcho.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AdditionalViberProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "viber_file_size",
                table: "campaigns",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "viber_video_duration",
                table: "campaigns",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "viber_message_id",
                table: "campaign_leads",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "viber_file_size",
                table: "campaigns");

            migrationBuilder.DropColumn(
                name: "viber_video_duration",
                table: "campaigns");

            migrationBuilder.DropColumn(
                name: "viber_message_id",
                table: "campaign_leads");
        }
    }
}
