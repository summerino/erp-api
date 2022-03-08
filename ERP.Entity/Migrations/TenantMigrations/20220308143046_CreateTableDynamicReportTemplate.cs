using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class CreateTableDynamicReportTemplate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DynamicReportTemplate",
                schema: "General",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    DataTypeParameter1 = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    SourceParameter1 = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    DropdownValueParameter1 = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    DropdownTextParameter1 = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    DataTypeParameter2 = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    SourceParameter2 = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    DropdownValueParameter2 = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    DropdownTextParameter2 = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    DataTypeParameter3 = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    SourceParameter3 = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    DropdownValueParameter3 = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    DropdownTextParameter3 = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    DataTypeParameter4 = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    SourceParameter4 = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    DropdownValueParameter4 = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    DropdownTextParameter4 = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    DataTypeParameter5 = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    SourceParameter5 = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    DropdownValueParameter5 = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    DropdownTextParameter5 = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    Query = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DynamicReportTemplate", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DynamicReportTemplate",
                schema: "General");
        }
    }
}
