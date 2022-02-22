using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class CreateTableSalesTarget : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SalesTargetHeader",
                schema: "Sales",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    StartDate = table.Column<DateTime>(type: "date", nullable: false),
                    EndDate = table.Column<DateTime>(type: "date", nullable: false),
                    Mark = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesTargetHeader", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "SalesTargetDetail",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    ItemGroupId = table.Column<int>(type: "int", nullable: false),
                    ItemSubGroupId = table.Column<int>(type: "int", nullable: false),
                    SubGroup = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesTargetDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesTargetDetail_ItemGroup_ItemGroupId",
                        column: x => x.ItemGroupId,
                        principalSchema: "Inventory",
                        principalTable: "ItemGroup",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SalesTargetDetail_ItemGroupSubGroup_ItemSubGroupId",
                        column: x => x.ItemSubGroupId,
                        principalSchema: "Inventory",
                        principalTable: "ItemGroupSubGroup",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SalesTargetDetail_SalesTargetHeader_Code",
                        column: x => x.Code,
                        principalSchema: "Sales",
                        principalTable: "SalesTargetHeader",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "SalesTargetSubject",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    SalesmanId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesTargetSubject", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesTargetSubject_Employee_SalesmanId",
                        column: x => x.SalesmanId,
                        principalSchema: "General",
                        principalTable: "Employee",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SalesTargetSubject_SalesTargetHeader_Code",
                        column: x => x.Code,
                        principalSchema: "Sales",
                        principalTable: "SalesTargetHeader",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalesTargetDetail_Code",
                schema: "Sales",
                table: "SalesTargetDetail",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_SalesTargetDetail_ItemGroupId",
                schema: "Sales",
                table: "SalesTargetDetail",
                column: "ItemGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesTargetDetail_ItemSubGroupId",
                schema: "Sales",
                table: "SalesTargetDetail",
                column: "ItemSubGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesTargetDetail_SubGroup",
                schema: "Sales",
                table: "SalesTargetDetail",
                column: "SubGroup");

            migrationBuilder.CreateIndex(
                name: "IX_SalesTargetSubject_Code",
                schema: "Sales",
                table: "SalesTargetSubject",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_SalesTargetSubject_SalesmanId",
                schema: "Sales",
                table: "SalesTargetSubject",
                column: "SalesmanId");

            // Create view Sales.vwSalesTargetHeader
            var sql = @"CREATE VIEW [Sales].[vwSalesTargetHeader]
AS
    SELECT t_h.*,
        u_c.Initial AS CreatedInitial,
        u_u.Initial AS UpdatedInitial,
        CASE t_h.Mark
            WHEN 'A' THEN 'Active'
            WHEN 'V' THEN 'Void' END AS [Status]
    FROM Sales.SalesTargetHeader t_h
    LEFT JOIN SystemManagement.[User] u_c
        ON u_c.Id = t_h.CreatedBy
    LEFT JOIN SystemManagement.[User] u_u
        ON u_u.Id = t_h.UpdatedBy";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SalesTargetDetail",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "SalesTargetSubject",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "SalesTargetHeader",
                schema: "Sales");
        }
    }
}
