using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using OneClickEcho.Persistence;

#nullable disable

namespace OneClickEcho.Persistence.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261029160000_AddSenderIsTwoWay")]
    /// <inheritdoc />
    public partial class AddSenderIsTwoWay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_two_way",
                table: "senders",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_two_way",
                table: "senders");
        }
    }
}
