using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clays.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameMachineryReferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Actions_Machinery_MachineId",
                table: "Actions");

            migrationBuilder.DropForeignKey(
                name: "FK_OwnerHistories_Machinery_MachineId",
                table: "OwnerHistories");

            migrationBuilder.RenameColumn(
                name: "MachineId",
                table: "OwnerHistories",
                newName: "MachineryId");

            migrationBuilder.RenameIndex(
                name: "IX_OwnerHistories_MachineId",
                table: "OwnerHistories",
                newName: "IX_OwnerHistories_MachineryId");

            migrationBuilder.RenameColumn(
                name: "MachineId",
                table: "Actions",
                newName: "MachineryId");

            migrationBuilder.RenameIndex(
                name: "IX_Actions_MachineId",
                table: "Actions",
                newName: "IX_Actions_MachineryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Actions_Machinery_MachineryId",
                table: "Actions",
                column: "MachineryId",
                principalTable: "Machinery",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_OwnerHistories_Machinery_MachineryId",
                table: "OwnerHistories",
                column: "MachineryId",
                principalTable: "Machinery",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Actions_Machinery_MachineryId",
                table: "Actions");

            migrationBuilder.DropForeignKey(
                name: "FK_OwnerHistories_Machinery_MachineryId",
                table: "OwnerHistories");

            migrationBuilder.RenameColumn(
                name: "MachineryId",
                table: "OwnerHistories",
                newName: "MachineId");

            migrationBuilder.RenameIndex(
                name: "IX_OwnerHistories_MachineryId",
                table: "OwnerHistories",
                newName: "IX_OwnerHistories_MachineId");

            migrationBuilder.RenameColumn(
                name: "MachineryId",
                table: "Actions",
                newName: "MachineId");

            migrationBuilder.RenameIndex(
                name: "IX_Actions_MachineryId",
                table: "Actions",
                newName: "IX_Actions_MachineId");

            migrationBuilder.AddForeignKey(
                name: "FK_Actions_Machinery_MachineId",
                table: "Actions",
                column: "MachineId",
                principalTable: "Machinery",
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
    }
}
