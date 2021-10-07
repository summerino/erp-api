using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class CreateProcTransferStockPrint : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create procedure dbo.sp_get_ts_print_data
            var sql = @"CREATE PROCEDURE [dbo].[sp_get_ts_print_data]
	@code varchar(17)
AS
BEGIN

	SELECT ts_h.Code, ts_h.[Date], ts_h.Notes,
		CASE ts_h.IsConsignee
			WHEN 1 THEN
				CASE ts_h.[Type]
					WHEN 'C' THEN 'Titipan Barang'
					WHEN 'RC' THEN 'Retur Titipan Barang'
				END
			ELSE
				CASE ts_h.[Type]
					WHEN 'DT' THEN 'Transfer Persediaan (Transfer Langsung)'
					WHEN 'IN' THEN 'Transfer Persediaan (Barang Masuk)'
					WHEN 'OUT' THEN 'Transfer Persediaan (Barang Keluar)'
				END
		END AS TitleCaption,
		ts_d.Qty,
		i.Initial AS ItemInitial, i.[Name] AS ItemName,
		uom_c.UnitToConvert AS ItemUnitName,
		w_f.Initial AS WarehouseFromInitial, w_f.[Name] AS WarehouseFromName,
		w_t.Initial AS WarehouseToInitial, w_t.[Name] AS WarehouseToName,
		e.Initial AS CreatedInitial
	FROM (
		SELECT *
		FROM Inventory.TransferStockHeader
		WHERE Code = @code
		AND Mark <> 'V'
	) ts_h
	LEFT JOIN Inventory.TransferStockDetail ts_d
		ON ts_d.Code = ts_h.Code
	LEFT JOIN Inventory.Item i
		ON i.Id = ts_d.ItemId
	LEFT JOIN Inventory.UoMConversion uom_c
		ON uom_c.Id = ts_d.UnitId
	LEFT JOIN Inventory.Warehouse w_f
		ON w_f.Code = ts_h.WarehouseCodeFrom
	LEFT JOIN Inventory.Warehouse w_t
		ON w_t.Code = ts_h.WarehouseCodeTo
	LEFT JOIN General.Employee e
		ON e.Id = ts_h.CreatedBy
	WHERE ts_d.ItemId IS NOT NULL
	ORDER BY i.Initial
	
END";
            migrationBuilder.Sql(sql);

			// Create view MobileSales.vwMobilePaymentInvoice
			sql = @"CREATE VIEW [MobileSales].[vwMobilePaymentInvoice]
AS
	SELECT pin.*,
		emp.Initial AS SalesmanInitial,
		emp.FirstName AS SalesmanName,
		cus.Initial AS CustomerInitial,
		cus.[Name] AS CustomerName,
		u_c.Initial AS CreatedInitial,
		u_u.Initial AS UpdatedInitial,
		u_a.Initial AS ApprovedInitial,
		u_r.Initial AS RejectedInitial,
		coa.[Name] AS CoaName,
		CASE pin.Mark
			WHEN 'A' THEN 'Active'
			WHEN 'APR' THEN 'Approved'
			WHEN 'REJ' THEN 'Rejected' END AS [Status]
	FROM MobileSales.MobilePaymentInvoice pin
	LEFT JOIN General.Employee emp
		ON emp.Id = pin.SalesmanId
	LEFT JOIN General.Customer cus
		ON cus.Code = pin.CustCode
	LEFT JOIN Accounting.COA coa
		ON coa.Code = pin.CoaCode
	LEFT JOIN SystemManagement.[User] u_c
		ON u_c.Id = pin.CreatedBy
	LEFT JOIN SystemManagement.[User] u_u
		ON u_u.Id = pin.UpdatedBy
	LEFT JOIN SystemManagement.[User] u_a
		ON u_a.Id = pin.ApprovedBy
	LEFT JOIN SystemManagement.[User] u_r
		ON u_r.Id = pin.RejectedBy";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop procedure dbo.sp_get_ts_print_data
            var sql = @"DROP PROCEDURE [dbo].[sp_get_ts_print_data]";
            migrationBuilder.Sql(sql);

			// Drop view MobileSales.vwMobilePaymentInvoice
			sql = @"DROP VIEW [MobileSales].[vwMobilePaymentInvoice]";
            migrationBuilder.Sql(sql);
        }
    }
}
