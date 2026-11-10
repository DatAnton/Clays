using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clays.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddContactMachineryAndComment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Comment",
                table: "Contacts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MachineryId",
                table: "Contacts",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Comment",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "MachineryId",
                table: "Contacts");
        }
    }
}
