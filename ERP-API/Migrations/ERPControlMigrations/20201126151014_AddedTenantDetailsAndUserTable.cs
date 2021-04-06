using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP_API.Migrations.ERPControlMigrations
{
    public partial class AddedTenantDetailsAndUserTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TenantDetails",
                columns: table => new
                {
                    TenantShard = table.Column<Guid>(nullable: false),
                    TenantName = table.Column<string>(nullable: true),
                    TenantSlug = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantDetails", x => x.TenantShard);
                });

            migrationBuilder.CreateTable(
                name: "UserTable",
                columns: table => new
                {
                    UserID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserName = table.Column<string>(nullable: false),
                    Password = table.Column<string>(nullable: false),
                    Role = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTable", x => x.UserID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TenantDetails");

            migrationBuilder.DropTable(
                name: "UserTable");
        }
    }
}
