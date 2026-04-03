using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OneClickEcho.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyIdToTestMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "company_id",
                table: "test_messages",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "ix_test_messages_company_id",
                table: "test_messages",
                column: "company_id");

            migrationBuilder.AddForeignKey(
                name: "fk_test_messages_companies_company_id",
                table: "test_messages",
                column: "company_id",
                principalTable: "companies",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_test_messages_companies_company_id",
                table: "test_messages");

            migrationBuilder.DropIndex(
                name: "ix_test_messages_company_id",
                table: "test_messages");

            migrationBuilder.DropColumn(
                name: "company_id",
                table: "test_messages");
        }
    }
}
