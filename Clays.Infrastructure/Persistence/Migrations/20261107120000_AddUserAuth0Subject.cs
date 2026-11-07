using Clays.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clays.Infrastructure.Persistence.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261107120000_AddUserAuth0Subject")]
public sealed class AddUserAuth0Subject : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Auth0Subject",
            table: "Users",
            type: "text",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Users_Auth0Subject",
            table: "Users",
            column: "Auth0Subject",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Users_Auth0Subject",
            table: "Users");

        migrationBuilder.DropColumn(name: "Auth0Subject", table: "Users");
    }
}
