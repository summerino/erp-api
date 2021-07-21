using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class CreateViewBeginningBalanceMemo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BeginningBalanceCreditMemo_Supplier_CustCode",
                schema: "Accounting",
                table: "BeginningBalanceCreditMemo");

            migrationBuilder.DropForeignKey(
                name: "FK_BeginningBalanceDebitMemo_Customer_SupCode",
                schema: "Accounting",
                table: "BeginningBalanceDebitMemo");

            migrationBuilder.AddForeignKey(
                name: "FK_BeginningBalanceCreditMemo_Customer_CustCode",
                schema: "Accounting",
                table: "BeginningBalanceCreditMemo",
                column: "CustCode",
                principalSchema: "General",
                principalTable: "Customer",
                principalColumn: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_BeginningBalanceDebitMemo_Supplier_SupCode",
                schema: "Accounting",
                table: "BeginningBalanceDebitMemo",
                column: "SupCode",
                principalSchema: "General",
                principalTable: "Supplier",
                principalColumn: "Code");

            // Create view Accounting.vwBeginningBalanceCreditMemo
            var sql = @"CREATE VIEW [Accounting].[vwBeginningBalanceCreditMemo]
AS
	SELECT cm.*,
		c.[Name] AS CustName,
		cr.Initial AS CreatedInitial,
		up.Initial AS UpdatedInitial
	FROM Accounting.BeginningBalanceCreditMemo cm
	LEFT JOIN General.Customer c
		ON cm.CustCode = c.Code
	LEFT JOIN SystemManagement.[User] cr
		ON cm.CreatedBy = cr.Id
	LEFT JOIN SystemManagement.[User] up
		ON cm.UpdatedBy = up.Id";
            migrationBuilder.Sql(sql);

            // Create view Accounting.vwBeginningBalanceDebitMemo
            sql = @"CREATE VIEW [Accounting].[vwBeginningBalanceDebitMemo]
AS
	SELECT dm.*,
		s.[Name] AS SupName,
		cr.Initial AS CreatedInitial,
		up.Initial AS UpdatedInitial
	FROM Accounting.BeginningBalanceDebitMemo dm
	LEFT JOIN General.Supplier s
		ON dm.SupCode = s.Code
	LEFT JOIN SystemManagement.[User] cr
		ON dm.CreatedBy = cr.Id
	LEFT JOIN SystemManagement.[User] up
		ON dm.UpdatedBy = up.Id";
            migrationBuilder.Sql(sql);

			// Alter view General.vwApproval
			sql = @"ALTER VIEW [General].[vwApproval]
AS
	SELECT *
	FROM (
		SELECT Code, PurchaseDate as [Date], [Name],'Aktiva Tetap' as SourceTrans, UpdatedDate, 15 as Seq
		FROM AssetManagement.FixedAsset
		WHERE ApprovedBy is null
		UNION
		SELECT Code, [Date], Notes as [Name],'Expedition Invoice' as SourceTrans, UpdatedDate, 14 as Seq
		FROM Expedition.ExpeditionInvoiceHeader
		WHERE ApprovedBy is null
		UNION
		SELECT Code, [Date], Notes as [Name],'Penyesuaian' as SourceTrans, UpdatedDate, 2 as Seq
		FROM Inventory.AdjustmentHeader
		WHERE ApprovedBy is null
		UNION
		SELECT Code, [Date], Notes as [Name],'Transfer Persediaan' as SourceTrans, UpdatedDate, 1 as Seq
		FROM Inventory.TransferStockHeader
		WHERE ApprovedBy is null
		UNION
		SELECT Code, [Date], Notes as [Name],'Order Pembelian' as SourceTrans, UpdatedDate, 3 as Seq
		FROM Purchasing.PurchaseOrderHeader
		WHERE ApprovedBy is null
		UNION
		SELECT Code, [Date], Notes as [Name],'Faktur Pembelian' as SourceTrans, UpdatedDate,5 as Seq
		FROM Purchasing.PurchaseInvoiceHeader
		WHERE ApprovedBy is null
		UNION
		SELECT Code, [Date], '-' as [Name],'Penerimaan' as SourceTrans, UpdatedDate, 4 as Seq
		FROM Purchasing.PurchaseReceiveHeader
		WHERE ApprovedBy is null
		UNION
		SELECT Code, [Date], Notes as [Name],'Retur Pembelian' as SourceTrans, UpdatedDate, 6 as Seq
		FROM Purchasing.PurchaseReturnHeader
		WHERE ApprovedBy is null
		UNION
		SELECT Code, [Date], Notes as [Name],'Order Penjualan' as SourceTrans, UpdatedDate, 7 as Seq
		FROM Sales.SalesOrderHeader
		WHERE ApprovedBy is null
		UNION
		SELECT Code, [Date], Notes as [Name],'Retur Penjualan' as SourceTrans, UpdatedDate, 10 as Seq
		FROM Sales.SalesReturnHeader
		WHERE ApprovedBy is null
		UNION
		SELECT Code, [Date], Notes as [Name],'Faktur Penjualan' as SourceTrans, UpdatedDate, 9 as Seq
		FROM Sales.SalesInvoiceHeader
		WHERE ApprovedBy is null
		UNION
		SELECT Code, [Date], Notes as [Name],'Surat Jalan' as SourceTrans, UpdatedDate, 8 as Seq
		FROM Sales.SalesDeliveryHeader
		WHERE ApprovedBy is null
		UNION
		SELECT Code, [Date], Notes as [Name],'Rencana Pengiriman' as SourceTrans, UpdatedDate, 11 as Seq
		FROM Sales.DeliveryPlanHeader
		WHERE ApprovedBy is null
		UNION
		SELECT Code, StartDate as [Date], [Name],'Promo' as SourceTrans, UpdatedDate, 11 as Seq
		FROM Sales.PromoHeader
		WHERE ApprovedBy is null
		UNION
		SELECT Code, [Date], '-' as [Name],'Rencana Kunjungan' as SourceTrans, UpdatedDate, 12 as Seq
		FROM Sales.VisitPlanHeader
		WHERE ApprovedBy is null
		UNION
		SELECT Code, [Date], '-' as [Name],'Perintah Kunjungan' as SourceTrans, UpdatedDate, 13 as Seq
		FROM Sales.VisitOrder
		WHERE ApprovedBy is null
	) as Tbl";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BeginningBalanceCreditMemo_Customer_CustCode",
                schema: "Accounting",
                table: "BeginningBalanceCreditMemo");

            migrationBuilder.DropForeignKey(
                name: "FK_BeginningBalanceDebitMemo_Supplier_SupCode",
                schema: "Accounting",
                table: "BeginningBalanceDebitMemo");

            migrationBuilder.AddForeignKey(
                name: "FK_BeginningBalanceCreditMemo_Supplier_CustCode",
                schema: "Accounting",
                table: "BeginningBalanceCreditMemo",
                column: "CustCode",
                principalSchema: "General",
                principalTable: "Supplier",
                principalColumn: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_BeginningBalanceDebitMemo_Customer_SupCode",
                schema: "Accounting",
                table: "BeginningBalanceDebitMemo",
                column: "SupCode",
                principalSchema: "General",
                principalTable: "Customer",
                principalColumn: "Code");

            // Drop view Accounting.vwBeginningBalanceDebitMemo
            var sql = @"DROP VIEW [Accounting].[vwBeginningBalanceDebitMemo]";
            migrationBuilder.Sql(sql);

			// Drop view Accounting.vwBeginningBalanceCreditMemo
			sql = @"DROP VIEW [Accounting].[vwBeginningBalanceCreditMemo]";
            migrationBuilder.Sql(sql);
        }
	}
}
