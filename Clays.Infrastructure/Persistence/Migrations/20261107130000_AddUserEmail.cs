using Clays.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clays.Infrastructure.Persistence.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261107130000_AddUserEmail")]
public sealed class AddUserEmail : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Email",
            table: "Users",
            type: "character varying(320)",
            maxLength: 320,
            nullable: true);

        migrationBuilder.AddCheckConstraint(
            name: "CK_Users_Email_Normalized",
            table: "Users",
            sql: "\"Email\" = lower(btrim(\"Email\"))");

        migrationBuilder.CreateIndex(
            name: "IX_Users_Email",
            table: "Users",
            column: "Email",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_Users_Email", table: "Users");
        migrationBuilder.DropCheckConstraint(name: "CK_Users_Email_Normalized", table: "Users");
        migrationBuilder.DropColumn(name: "Email", table: "Users");
    }
}
