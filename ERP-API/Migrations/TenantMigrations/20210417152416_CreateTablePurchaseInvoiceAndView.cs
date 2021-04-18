using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP_API.Migrations.TenantMigrations
{
    public partial class CreateTablePurchaseInvoiceAndView : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Purchasing");

            migrationBuilder.CreateTable(
                name: "PurchaseInvoiceDetail",
                schema: "Purchasing",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    RcvCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    ShipmentFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    HandlingFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FinalDisc = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    DPP = table.Column<decimal>(type: "decimal(19,6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseInvoiceDetail", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseInvoiceHeader",
                schema: "Purchasing",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    DueDate = table.Column<DateTime>(type: "date", nullable: false),
                    POCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: true),
                    RefNo = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    SupCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    IssuedBy = table.Column<long>(type: "bigint", nullable: false),
                    CurrCode = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    Mark = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseInvoiceHeader", x => x.Code);
                });

            // Create view Purchasing.vwPurchaseInvoiceHeader
            var sql = @"CREATE VIEW [Purchasing].[vwPurchaseInvoiceHeader]
AS
	SELECT pi_h.*,
		s.[Name] AS SupName,
		e.Initial AS IssuedInitial
	FROM Purchasing.PurchaseInvoiceHeader pi_h
	LEFT JOIN msSuppliers s
		ON s.Code = pi_h.SupCode
	LEFT JOIN msEmployees e
		ON e.Id = pi_h.IssuedBy";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PurchaseInvoiceDetail",
                schema: "Purchasing");

            migrationBuilder.DropTable(
                name: "PurchaseInvoiceHeader",
                schema: "Purchasing");

            // Drop view Purchasing.vwPurchaseInvoiceHeader
            migrationBuilder.Sql(@"DROP VIEW Purchasing.vwPurchaseInvoiceHeader");
        }
    }
}
