using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clays.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOtherManufacturer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Manufacturers",
                columns: new[] { "Id", "Name" },
                values: new object[] { 7, "Other" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Manufacturers",
                keyColumn: "Id",
                keyValue: 7);
        }
    }
}
