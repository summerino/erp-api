using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AlterVwApproval : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			var sql = @"ALTER VIEW [General].[vwApproval]
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
		SELECT Code, Notes as [Name],'Penyesuaian' as SourceTrans, UpdatedDate, 2 as Seq
		FROM Inventory.AdjustmentHeader
		WHERE ApprovedBy is null
		UNION
		SELECT Code, Notes as [Name],'Transfer Persediaan' as SourceTrans, UpdatedDate, 1 as Seq
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
		SELECT Code, Notes as [Name],'Rencana Pengiriman' as SourceTrans, UpdatedDate, 11 as Seq
		FROM Sales.DeliveryPlanHeader
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
			var sql = @"ALTER VIEW [General].[vwApproval]
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
    }
}
