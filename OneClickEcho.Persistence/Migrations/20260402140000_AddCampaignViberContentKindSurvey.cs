using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OneClickEcho.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCampaignViberContentKindSurvey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "viber_content_kind",
                table: "campaigns",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<string>(
                name: "viber_survey_options_json",
                table: "campaigns",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "viber_content_kind",
                table: "campaigns");

            migrationBuilder.DropColumn(
                name: "viber_survey_options_json",
                table: "campaigns");
        }
    }
}
