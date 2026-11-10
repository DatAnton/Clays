using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Clays.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMachineryLibrary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Actions_Machines_MachineId",
                table: "Actions");

            migrationBuilder.DropForeignKey(
                name: "FK_Machines_Companies_CompanyId",
                table: "Machines");

            migrationBuilder.DropForeignKey(
                name: "FK_Machines_Manufacturers_ManufacturerId",
                table: "Machines");

            migrationBuilder.DropForeignKey(
                name: "FK_Machines_Users_CreatedById",
                table: "Machines");

            migrationBuilder.DropForeignKey(
                name: "FK_OwnerHistories_Machines_MachineId",
                table: "OwnerHistories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Machines",
                table: "Machines");

            migrationBuilder.RenameTable(
                name: "Machines",
                newName: "Machinery");

            migrationBuilder.RenameColumn(
                name: "ManufacturerId",
                table: "Machinery",
                newName: "MachineryLibraryId");

            migrationBuilder.RenameIndex(
                name: "IX_Machines_ManufacturerId",
                table: "Machinery",
                newName: "IX_Machinery_MachineryLibraryId");

            migrationBuilder.RenameIndex(
                name: "IX_Machines_CreatedById",
                table: "Machinery",
                newName: "IX_Machinery_CreatedById");

            migrationBuilder.RenameIndex(
                name: "IX_Machines_CompanyId",
                table: "Machinery",
                newName: "IX_Machinery_CompanyId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Machinery",
                table: "Machinery",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "MachineryLibrary",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ManufacturerId = table.Column<int>(type: "integer", nullable: false),
                    Model = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachineryLibrary", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MachineryLibrary_Manufacturers_ManufacturerId",
                        column: x => x.ManufacturerId,
                        principalTable: "Manufacturers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MachineryLibrary_ManufacturerId_Model",
                table: "MachineryLibrary",
                columns: new[] { "ManufacturerId", "Model" },
                unique: true);

            // The renamed column still contains the original manufacturer IDs here.
            migrationBuilder.Sql("""
                INSERT INTO "MachineryLibrary" ("ManufacturerId", "Model")
                SELECT DISTINCT "MachineryLibraryId", "Model" FROM "Machinery";

                UPDATE "Machinery" AS machinery
                SET "MachineryLibraryId" = library."Id"
                FROM "MachineryLibrary" AS library
                WHERE machinery."MachineryLibraryId" = library."ManufacturerId"
                    AND machinery."Model" = library."Model";
                """);

            migrationBuilder.DropColumn(
                name: "Model",
                table: "Machinery");

            migrationBuilder.AddForeignKey(
                name: "FK_Actions_Machinery_MachineId",
                table: "Actions",
                column: "MachineId",
                principalTable: "Machinery",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Machinery_Companies_CompanyId",
                table: "Machinery",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Machinery_MachineryLibrary_MachineryLibraryId",
                table: "Machinery",
                column: "MachineryLibraryId",
                principalTable: "MachineryLibrary",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Machinery_Users_CreatedById",
                table: "Machinery",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_OwnerHistories_Machinery_MachineId",
                table: "OwnerHistories",
                column: "MachineId",
                principalTable: "Machinery",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Actions_Machinery_MachineId",
                table: "Actions");

            migrationBuilder.DropForeignKey(
                name: "FK_Machinery_Companies_CompanyId",
                table: "Machinery");

            migrationBuilder.DropForeignKey(
                name: "FK_Machinery_MachineryLibrary_MachineryLibraryId",
                table: "Machinery");

            migrationBuilder.DropForeignKey(
                name: "FK_Machinery_Users_CreatedById",
                table: "Machinery");

            migrationBuilder.DropForeignKey(
                name: "FK_OwnerHistories_Machinery_MachineId",
                table: "OwnerHistories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Machinery",
                table: "Machinery");

            migrationBuilder.RenameTable(
                name: "Machinery",
                newName: "Machines");

            migrationBuilder.RenameColumn(
                name: "MachineryLibraryId",
                table: "Machines",
                newName: "ManufacturerId");

            migrationBuilder.RenameIndex(
                name: "IX_Machinery_MachineryLibraryId",
                table: "Machines",
                newName: "IX_Machines_ManufacturerId");

            migrationBuilder.RenameIndex(
                name: "IX_Machinery_CreatedById",
                table: "Machines",
                newName: "IX_Machines_CreatedById");

            migrationBuilder.RenameIndex(
                name: "IX_Machinery_CompanyId",
                table: "Machines",
                newName: "IX_Machines_CompanyId");

            migrationBuilder.AddColumn<string>(
                name: "Model",
                table: "Machines",
                type: "text",
                nullable: false,
                defaultValue: "");

            // Restore the denormalized values before removing the library.
            migrationBuilder.Sql("""
                UPDATE "Machines" AS machine
                SET "ManufacturerId" = library."ManufacturerId", "Model" = library."Model"
                FROM "MachineryLibrary" AS library
                WHERE machine."ManufacturerId" = library."Id";
                """);

            migrationBuilder.DropTable(
                name: "MachineryLibrary");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Machines",
                table: "Machines",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Actions_Machines_MachineId",
                table: "Actions",
                column: "MachineId",
                principalTable: "Machines",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Machines_Companies_CompanyId",
                table: "Machines",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Machines_Manufacturers_ManufacturerId",
                table: "Machines",
                column: "ManufacturerId",
                principalTable: "Manufacturers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Machines_Users_CreatedById",
                table: "Machines",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_OwnerHistories_Machines_MachineId",
                table: "OwnerHistories",
                column: "MachineId",
                principalTable: "Machines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
