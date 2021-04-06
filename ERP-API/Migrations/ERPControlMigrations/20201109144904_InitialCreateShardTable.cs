using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP_API.Migrations.ERPControlMigrations
{
    public partial class InitialCreateShardTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ShardTable",
                columns: table => new
                {
                    ShardID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Shard = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShardTable", x => x.ShardID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShardTable");
        }
    }
}
