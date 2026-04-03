using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OneClickEcho.Persistence.Db.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTestPhoneNumbersAndAddPhoneNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "test_phone_numbers",
                table: "campaigns");

            migrationBuilder.AddColumn<string>(
                name: "test_phone_number",
                table: "campaigns",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "test_phone_number",
                table: "campaigns");

            migrationBuilder.AddColumn<string>(
                name: "test_phone_numbers",
                table: "campaigns",
                type: "jsonb",
                nullable: true);
        }
    }
}
