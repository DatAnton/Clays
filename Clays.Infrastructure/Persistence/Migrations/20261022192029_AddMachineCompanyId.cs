using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clays.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMachineCompanyId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "Machines",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Machines_CompanyId",
                table: "Machines",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Machines_Companies_CompanyId",
                table: "Machines",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Machines_Companies_CompanyId",
                table: "Machines");

            migrationBuilder.DropIndex(
                name: "IX_Machines_CompanyId",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Machines");
        }
    }
}
