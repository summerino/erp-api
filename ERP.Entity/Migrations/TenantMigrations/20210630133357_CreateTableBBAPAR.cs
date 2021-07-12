using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class CreateTableBBAPAR : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BeginningBalanceAP",
                schema: "Accounting",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    DueDate = table.Column<DateTime>(type: "date", nullable: false),
                    SupCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    CurrCode = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    CurrRate = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BeginningBalanceAP", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BeginningBalanceAP_Currency_CurrCode",
                        column: x => x.CurrCode,
                        principalSchema: "General",
                        principalTable: "Currency",
                        principalColumn: "Code");
                    table.ForeignKey(
                        name: "FK_BeginningBalanceAP_Supplier_SupCode",
                        column: x => x.SupCode,
                        principalSchema: "General",
                        principalTable: "Supplier",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "BeginningBalanceAR",
                schema: "Accounting",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    DueDate = table.Column<DateTime>(type: "date", nullable: false),
                    CustCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    CurrCode = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    CurrRate = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BeginningBalanceAR", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BeginningBalanceAR_Currency_CurrCode",
                        column: x => x.CurrCode,
                        principalSchema: "General",
                        principalTable: "Currency",
                        principalColumn: "Code");
                    table.ForeignKey(
                        name: "FK_BeginningBalanceAR_Customer_CustCode",
                        column: x => x.CustCode,
                        principalSchema: "General",
                        principalTable: "Customer",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BeginningBalanceAP_Code",
                schema: "Accounting",
                table: "BeginningBalanceAP",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BeginningBalanceAP_CurrCode",
                schema: "Accounting",
                table: "BeginningBalanceAP",
                column: "CurrCode");

            migrationBuilder.CreateIndex(
                name: "IX_BeginningBalanceAP_SupCode",
                schema: "Accounting",
                table: "BeginningBalanceAP",
                column: "SupCode");

            migrationBuilder.CreateIndex(
                name: "IX_BeginningBalanceAR_Code",
                schema: "Accounting",
                table: "BeginningBalanceAR",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BeginningBalanceAR_CurrCode",
                schema: "Accounting",
                table: "BeginningBalanceAR",
                column: "CurrCode");

            migrationBuilder.CreateIndex(
                name: "IX_BeginningBalanceAR_CustCode",
                schema: "Accounting",
                table: "BeginningBalanceAR",
                column: "CustCode");

            // Create view Accounting.vwCOA
            var sql = @"CREATE VIEW [Accounting].[vwCOA]
AS
	SELECT c.*,
		t.[Name] as TypeName,
		cr.Initial as CreatedInitial,
		up.Initial as UpdatedInitial,
		CASE 
			WHEN EXISTS (
				SELECT cd.ParentId 
				FROM Accounting.COA cd 
				WHERE cd.ParentId = c.Id
			) 
			THEN 1 
			ELSE 0 
		END AS IsParent
	FROM Accounting.COA c
	LEFT JOIN Accounting.COAType t
		ON c.TypeId = t.Id
	LEFT JOIN SystemManagement.[User] cr
		ON c.CreatedBy = cr.Id
	LEFT JOIN SystemManagement.[User] up
		ON c.CreatedBy = up.Id";
            migrationBuilder.Sql(sql);

            // Create view General.vwApproval
            sql = @"CREATE VIEW [General].[vwApproval]
AS
	SELECT *
	FROM (
		SELECT Code, [Name],'Aktiva Tetap' as SourceTrans, UpdatedDate, 15 as Seq
		FROM AssetManagement.FixedAsset
		WHERE ApprovedBy is null
		UNION
		SELECT Code, Notes as [Name],'Expedition Invoice' as SourceTrans, UpdatedDate, 14 as Seq
		FROM Expedition.ExpeditionInvoiceHeader
		WHERE ApprovedBy is null
		UNION
		SELECT Code, Notes as [Name],'Penyesuaian' as SourceTrans, UpdatedDate, 1 as Seq
		FROM Inventory.AdjustmentHeader
		WHERE ApprovedBy is null
		UNION
		SELECT Code, Notes as [Name],'Transfer Persediaan' as SourceTrans, UpdatedDate, 2 as Seq
		FROM Inventory.TransferStockHeader
		WHERE ApprovedBy is null
		UNION
		SELECT Code, Notes as [Name],'Order Pembelian' as SourceTrans, UpdatedDate, 3 as Seq
		FROM Purchasing.PurchaseOrderHeader
		WHERE ApprovedBy is null
		UNION
		SELECT Code, Notes as [Name],'Faktur Pembelian' as SourceTrans, UpdatedDate,5 as Seq
		FROM Purchasing.PurchaseInvoiceHeader
		WHERE ApprovedBy is null
		UNION
		SELECT Code, '-' as [Name],'Penerimaan' as SourceTrans, UpdatedDate, 4 as Seq
		FROM Purchasing.PurchaseReceiveHeader
		WHERE ApprovedBy is null
		UNION
		SELECT Code, Notes as [Name],'Retur Pembelian' as SourceTrans, UpdatedDate, 6 as Seq
		FROM Purchasing.PurchaseReturnHeader
		WHERE ApprovedBy is null
		UNION
		SELECT Code, Notes as [Name],'Order Penjualan' as SourceTrans, UpdatedDate, 7 as Seq
		FROM Sales.SalesOrderHeader
		WHERE ApprovedBy is null
		UNION
		SELECT Code, Notes as [Name],'Retur Penjualan' as SourceTrans, UpdatedDate, 10 as Seq
		FROM Sales.SalesReturnHeader
		WHERE ApprovedBy is null
		UNION
		SELECT Code, Notes as [Name],'Faktur Penjualan' as SourceTrans, UpdatedDate, 9 as Seq
		FROM Sales.SalesInvoiceHeader
		WHERE ApprovedBy is null
		UNION
		SELECT Code, Notes as [Name],'Surat Jalan' as SourceTrans, UpdatedDate, 8 as Seq
		FROM Sales.SalesDeliveryHeader
		WHERE ApprovedBy is null
		UNION
		SELECT Code, [Name],'Promo' as SourceTrans, UpdatedDate, 11 as Seq
		FROM Sales.PromoHeader
		WHERE ApprovedBy is null
		UNION
		SELECT Code, '-' as [Name],'Rencana Kunjungan' as SourceTrans, UpdatedDate, 12 as Seq
		FROM Sales.VisitPlanHeader
		WHERE ApprovedBy is null
		UNION
		SELECT Code, '-' as [Name],'Perintah Kunjungan' as SourceTrans, UpdatedDate, 13 as Seq
		FROM Sales.VisitOrder
		WHERE ApprovedBy is null
	) as Tbl";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BeginningBalanceAP",
                schema: "Accounting");

            migrationBuilder.DropTable(
                name: "BeginningBalanceAR",
                schema: "Accounting");

            // Drop view Accounting.COA
            var sql = @"DROP VIEW [Accounting].[vwCOA]";
            migrationBuilder.Sql(sql);

            // Drop view General.vwApproval
            sql = @"DROP VIEW [General].[vwApproval]";
            migrationBuilder.Sql(sql);
        }
    }
}
