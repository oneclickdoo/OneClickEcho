using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OneClickEcho.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddingIsViberToTestMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_viber",
                table: "test_messages",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_viber",
                table: "test_messages");
        }
    }
}
