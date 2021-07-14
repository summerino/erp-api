using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class CreateTableBeginningBalanceStock : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BeginningBalanceHeader",
                schema: "Inventory",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    WarehouseCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    Notes = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BeginningBalanceHeader", x => x.Code);
                    table.ForeignKey(
                        name: "FK_BeginningBalanceHeader_Warehouse_WarehouseCode",
                        column: x => x.WarehouseCode,
                        principalSchema: "Inventory",
                        principalTable: "Warehouse",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "BeginningBalanceDetail",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BeginningBalanceDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BeginningBalanceDetail_BeginningBalanceHeader_Code",
                        column: x => x.Code,
                        principalSchema: "Inventory",
                        principalTable: "BeginningBalanceHeader",
                        principalColumn: "Code");
                    table.ForeignKey(
                        name: "FK_BeginningBalanceDetail_Item_ItemId",
                        column: x => x.ItemId,
                        principalSchema: "Inventory",
                        principalTable: "Item",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BeginningBalanceDetail_UoM_UomId",
                        column: x => x.UomId,
                        principalSchema: "Inventory",
                        principalTable: "UoM",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BeginningBalanceDetail_UoMConversion_UnitId",
                        column: x => x.UnitId,
                        principalSchema: "Inventory",
                        principalTable: "UoMConversion",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BeginningBalanceDetail_Code",
                schema: "Inventory",
                table: "BeginningBalanceDetail",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_BeginningBalanceDetail_ItemId",
                schema: "Inventory",
                table: "BeginningBalanceDetail",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_BeginningBalanceDetail_UnitId",
                schema: "Inventory",
                table: "BeginningBalanceDetail",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_BeginningBalanceDetail_UomId",
                schema: "Inventory",
                table: "BeginningBalanceDetail",
                column: "UomId");

            migrationBuilder.CreateIndex(
                name: "IX_BeginningBalanceHeader_WarehouseCode",
                schema: "Inventory",
                table: "BeginningBalanceHeader",
                column: "WarehouseCode");

            var sql = @"DROP VIEW [Finance].[VwAP]";
            migrationBuilder.Sql(sql);

            sql = @"DROP VIEW [Finance].[VwAR]";
            migrationBuilder.Sql(sql);

            sql = @"CREATE VIEW [Finance].[vwAP] AS
SELECT B.Code, B.SupCode, S.[Name] AS SupName, [Date],CurrCode, CurrRate as Rate, Amount, PaidAmount, Amount - PaidAmount as Remaining, B.Notes
FROM Accounting.BeginningBalanceAP B
JOIN General.Supplier S ON B.SupCode = S.Code
WHERE B.PaidAmount < B.Amount AND B.IsActive = 1
UNION
SELECT P.Code,P.SupCode, S.[Name] as SupName, P.[Date],P.CurrCode, 1 AS Rate, Total as Amount, PaidAmount, Total - PaidAmount as Remaining, P.Notes 
FROM Purchasing.PurchaseInvoiceHeader P
JOIN General.Supplier S ON P.SupCode = S.Code
WHERE P.PaidAmount < P.Total AND P.Mark = 'A'";
            migrationBuilder.Sql(sql);

            sql = @"CREATE VIEW [Finance].[vwAR] AS
SELECT B.Code, B.CustCode, C.[Name] AS CustName, [Date],CurrCode, CurrRate as Rate , Amount, PaidAmount, Amount - PaidAmount as Remaining, B.Notes
FROM Accounting.BeginningBalanceAR B
JOIN General.Customer C ON B.CustCode = C.Code
WHERE B.PaidAmount < B.Amount AND B.IsActive = 1
UNION
SELECT S.Code,S.CustCode, C.[Name] as CustName, S.[Date],S.CurrCode, 1 AS Rate, Total as Amount, PaidAmount, Total - PaidAmount as Remaining, S.Notes 
FROM Sales.SalesInvoiceHeader S
JOIN General.Customer C ON S.CustCode = C.Code
WHERE S.PaidAmount < S.Total AND S.Mark = 'A'";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BeginningBalanceDetail",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "BeginningBalanceHeader",
                schema: "Inventory");
        }
    }
}
