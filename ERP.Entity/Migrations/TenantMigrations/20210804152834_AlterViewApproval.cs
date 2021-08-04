using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AlterViewApproval : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Alter view Purchasing.vwDebitMemo
            var sql = @"ALTER VIEW [General].[vwApproval]
AS
	SELECT ts.Code, ts.[Date],
		w_f.[Name] + ' (' + w_f.Initial + ') - ' + w_t.[Name] + ' (' + w_t.Initial + ')' AS Descr,
		'Transfer Persediaan' AS SourceTrans, ts.UpdatedDate, 1 AS Seq, 9 AS ActionId
	FROM (
		SELECT Code, [Date], WarehouseCodeFrom, WarehouseCodeTo, UpdatedDate
		FROM Inventory.TransferStockHeader
		WHERE ApprovedBy IS NULL
		AND IsConsignee = 0
		AND Mark <> 'V'
	) ts
	LEFT JOIN Inventory.Warehouse w_f
		ON w_f.Code = ts.WarehouseCodeFrom
	LEFT JOIN Inventory.Warehouse w_t
		ON w_t.Code = ts.WarehouseCodeTo

	UNION ALL
	SELECT ts.Code, ts.[Date],
		w_f.[Name] + ' (' + w_f.Initial + ') - ' + w_t.[Name] + ' (' + w_t.Initial + ')',
		'Konsinyasi', ts.UpdatedDate, 2, 13
	FROM (
		SELECT Code, [Date], WarehouseCodeFrom, WarehouseCodeTo, UpdatedDate
		FROM Inventory.TransferStockHeader
		WHERE ApprovedBy IS NULL
		AND IsConsignee = 1
		AND Mark <> 'V'
	) ts
	LEFT JOIN Inventory.Warehouse w_f
		ON w_f.Code = ts.WarehouseCodeFrom
	LEFT JOIN Inventory.Warehouse w_t
		ON w_t.Code = ts.WarehouseCodeTo

	UNION ALL
	SELECT adj.Code, adj.[Date],
		w.Initial + ' - ' + w.[Name],
		'Penyesuaian', adj.UpdatedDate, 3, 14
	FROM (
		SELECT Code, [Date], WarehouseCode, UpdatedDate
		FROM Inventory.AdjustmentHeader
		WHERE ApprovedBy IS NULL
		AND Mark <> 'V'
	) adj
	LEFT JOIN Inventory.Warehouse w
		ON w.Code = adj.WarehouseCode

	UNION ALL
	SELECT po.Code, po.[Date],
		po.SupCode + ' - ' + s.[Name],
		'Order Pembelian', po.UpdatedDate, 4, 15
	FROM (
		SELECT Code, [Date], SupCode, UpdatedDate
		FROM Purchasing.PurchaseOrderHeader
		WHERE ApprovedBy IS NULL
		AND Mark <> 'V'
	) po
	LEFT JOIN General.Supplier s
		ON s.Code = po.SupCode

	UNION ALL
	SELECT rcv.Code, rcv.[Date],
		rcv.SupCode + ' - ' + s.[Name],
		'Penerimaan', rcv.UpdatedDate, 5, 16
	FROM (
		SELECT Code, [Date], SupCode, UpdatedDate
		FROM Purchasing.PurchaseReceiveHeader
		WHERE ApprovedBy IS NULL
		AND Mark <> 'V'
	) rcv
	LEFT JOIN General.Supplier s
		ON s.Code = rcv.SupCode

	UNION ALL
	SELECT pi.Code, pi.[Date],
		pi.SupCode + ' - ' + s.[Name],
		'Faktur Pembelian', pi.UpdatedDate, 6, 17
	FROM (
		SELECT Code, [Date], SupCode, UpdatedDate
		FROM Purchasing.PurchaseInvoiceHeader
		WHERE ApprovedBy IS NULL
		AND Mark <> 'V'
	) pi
	LEFT JOIN General.Supplier s
		ON s.Code = pi.SupCode

	UNION ALL
	SELECT pr.Code, pr.[Date],
		pr.SupCode + ' - ' + s.[Name],
		'Retur Pembelian', pr.UpdatedDate, 7, 18
	FROM (
		SELECT Code, [Date], SupCode, UpdatedDate
		FROM Purchasing.PurchaseReturnHeader
		WHERE ApprovedBy IS NULL
		AND Mark <> 'V'
	) pr
	LEFT JOIN General.Supplier s
		ON s.Code = pr.SupCode

	UNION ALL
	SELECT Code, StartDate, [Name], 'Promo', UpdatedDate, 8, 19
	FROM Sales.PromoHeader
	WHERE ApprovedBy IS NULL
	AND Mark <> 'V'

	UNION ALL
	SELECT so.Code, so.[Date],
		so.CustCode + ' - ' + c.[Name],
		'Order Penjualan', so.UpdatedDate, 9, 20
	FROM (
		SELECT Code, [Date], CustCode, UpdatedDate
		FROM Sales.SalesOrderHeader
		WHERE ApprovedBy IS NULL
		AND Mark <> 'V'
	) so
	LEFT JOIN General.Customer c
		ON c.Code = so.CustCode

	UNION ALL
	SELECT dlv.Code, dlv.[Date],
		dlv.CustCode + ' - ' + c.[Name],
		'Surat Jalan', dlv.UpdatedDate, 10, 21
	FROM (
		SELECT Code, [Date], CustCode, UpdatedDate
		FROM Sales.SalesDeliveryHeader
		WHERE ApprovedBy IS NULL
		AND Mark <> 'V'
	) dlv
	LEFT JOIN General.Customer c
		ON c.Code = dlv.CustCode

	UNION ALL
	SELECT si.Code, si.[Date],
		si.CustCode + ' - ' + c.[Name],
		'Faktur Penjualan', si.UpdatedDate, 11, 22
	FROM (
		SELECT Code, [Date], CustCode, UpdatedDate
		FROM Sales.SalesInvoiceHeader
		WHERE ApprovedBy IS NULL
		AND Mark <> 'V'
	) si
	LEFT JOIN General.Customer c
		ON c.Code = si.CustCode

	UNION ALL
	SELECT sr.Code, sr.[Date],
		sr.CustCode + ' - ' + c.[Name],
		'Retur Penjualan', sr.UpdatedDate, 12, 23
	FROM (
		SELECT Code, [Date], CustCode, UpdatedDate
		FROM Sales.SalesReturnHeader
		WHERE ApprovedBy IS NULL
		AND Mark <> 'V'
	) sr
	LEFT JOIN General.Customer c
		ON c.Code = sr.CustCode

	UNION ALL
	SELECT dlv_p.Code, dlv_p.[Date],
		w.Initial + ' - ' + w.[Name],
		'Rencana Pengiriman', dlv_p.UpdatedDate, 13, 24
	FROM (
		SELECT Code, [Date], WarehouseCode, UpdatedDate
		FROM Sales.DeliveryPlanHeader
		WHERE ApprovedBy IS NULL
		AND Mark <> 'V'
	) dlv_p
	LEFT JOIN Inventory.Warehouse w
		ON w.Code = dlv_p.WarehouseCode

	--UNION ALL
	--SELECT vst_p.Code, vst_p.[Date],
	--	 sg.Initial + ' - ' + sg.[Name],
	--	'Rencana Kunjungan', vst_p.UpdatedDate, 14
	--FROM (
	--	SELECT Code, [Date], GroupId, UpdatedDate
	--	FROM Sales.VisitPlanHeader
	--	WHERE ApprovedBy IS NULL
	--	AND Mark <> 'V'
	--) vst_p
	--LEFT JOIN Sales.SalesmanGroup sg
	--	ON sg.Id = vst_p.GroupId

	UNION ALL
	SELECT vst_o.Code, vst_o.[Date],
		e.Initial + ' - ' + e.[FirstName],
		'Perintah Kunjungan', vst_o.UpdatedDate, 14, 25
	FROM (
		SELECT Code, [Date], SalesmanId, UpdatedDate
		FROM Sales.VisitOrder
		WHERE ApprovedBy IS NULL
		AND Mark <> 'V'
	) vst_o
	LEFT JOIN General.Employee e
		ON e.Id = vst_o.SalesmanId

	UNION ALL
	SELECT ei.Code, ei.[Date],
		ei.SupCode + ' - ' + s.[Name],
		'Faktur Ekspedisi', ei.UpdatedDate, 15, 26
	FROM (
		SELECT Code, [Date], SupCode, UpdatedDate
		FROM Expedition.ExpeditionInvoiceHeader
		WHERE ApprovedBy IS NULL
		AND Mark <> 'V'
	) ei
	LEFT JOIN General.Supplier s
		ON s.Code = ei.SupCode

	UNION ALL
	SELECT cb.Code, cb.[Date],
		cb.CoaCode + ' - ' + c.[Name],
		'Kas Bank Umum', cb.UpdatedDate, 16, 27
	FROM (
		SELECT Code, [Date], CoaCode, UpdatedDate
		FROM Finance.GeneralCashBankHeader
		WHERE ApprovedBy IS NULL
		AND IsInterCashBank = 0
		AND Mark <> 'V'
	) cb
	LEFT JOIN Accounting.COA c
		ON c.Code = cb.CoaCode

	UNION ALL
	SELECT cb_h.Code, cb_h.[Date],
		c_h.[Name] + ' (' + cb_h.CoaCode + ') - ' + c_d.[Name] + ' (' + cb_d.CoaCode + ')',
		'Pemindahan Dana', cb_h.UpdatedDate, 17, 28
	FROM (
		SELECT Code, [Date], CoaCode, UpdatedDate
		FROM Finance.GeneralCashBankHeader
		WHERE ApprovedBy IS NULL
		AND IsInterCashBank = 1
		AND Mark <> 'V'
	) cb_h
    LEFT JOIN Finance.GeneralCashBankDetail cb_d
        ON cb_d.Code = cb_h.Code
	LEFT JOIN Accounting.COA c_h
		ON c_h.Code = cb_h.CoaCode
	LEFT JOIN Accounting.COA c_d
		ON c_d.Code = cb_d.CoaCode
	WHERE cb_d.[Type] = 'ICBO'

	UNION ALL
	SELECT Code, [Date], Notes, 'Jurnal Umum', UpdatedDate, 18, 29
	FROM Accounting.GeneralJournalHeader
	WHERE ApprovedBy IS NULL
	AND Mark <> 'V'

	UNION ALL
	SELECT Code, PurchaseDate, [Name], 'Aktiva Tetap', UpdatedDate, 19, 30
	FROM AssetManagement.FixedAsset
	WHERE ApprovedBy IS NULL
	AND Mark <> 'V'";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
