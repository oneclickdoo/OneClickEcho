using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OneClickEcho.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePhoneNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_leads_phone_number",
                table: "leads");

            migrationBuilder.CreateIndex(
                name: "ix_leads_phone_number",
                table: "leads",
                column: "phone_number");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_leads_phone_number",
                table: "leads");

            migrationBuilder.CreateIndex(
                name: "ix_leads_phone_number",
                table: "leads",
                column: "phone_number",
                unique: true);
        }
    }
}
