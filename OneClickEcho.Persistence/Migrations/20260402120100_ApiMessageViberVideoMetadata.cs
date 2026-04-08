using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OneClickEcho.Persistence.Migrations
{
    /// <inheritdoc />
    [Migration("20260402120100_ApiMessageViberVideoMetadata")]
    public partial class ApiMessageViberVideoMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "viber_file_size",
                table: "api_messages",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "viber_video_thumbnail",
                table: "api_messages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "viber_video_duration",
                table: "api_messages",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "viber_file_size",
                table: "api_messages");

            migrationBuilder.DropColumn(
                name: "viber_video_thumbnail",
                table: "api_messages");

            migrationBuilder.DropColumn(
                name: "viber_video_duration",
                table: "api_messages");
        }
    }
}
