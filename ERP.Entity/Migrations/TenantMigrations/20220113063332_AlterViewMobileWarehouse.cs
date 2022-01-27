using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AlterViewMobileWarehouse : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Alter view MobileWarehouse.vwMobileDeliveryItemHeader
            var sql = @"ALTER VIEW [MobileWarehouse].[vwMobileDeliveryItemHeader]
AS
    SELECT md_h.*,
        u_c.Initial AS CreatedInitial,
        u_u.Initial AS UpdatedInitial,
        u_a.Initial AS ApprovedInitial,
        u_r.Initial AS RejectedInitial,
        CASE md_h.Mark
            WHEN 'A' THEN 'Aktif'
            WHEN 'APR' THEN 'Disetujui'
            WHEN 'REJ' THEN 'Ditolak' END AS [Status],
		dp_h.[Date],
		dp_h.WarehouseCode,
		dp_h.WarehouseInitial
    FROM MobileWarehouse.MobileDeliveryItemHeader md_h
    LEFT JOIN SystemManagement.[User] u_c
        ON u_c.Id = md_h.CreatedBy
    LEFT JOIN SystemManagement.[User] u_u
        ON u_u.Id = md_h.UpdatedBy
    LEFT JOIN SystemManagement.[User] u_a
        ON u_a.Id = md_h.ApprovedBy
	LEFT JOIN SystemManagement.[User] u_r
        ON u_r.Id = md_h.RejectedBy
	LEFT JOIN Sales.vwDeliveryPlanHeader dp_h
		ON dp_h.Code = md_h.DlvPlanCode";
            migrationBuilder.Sql(sql);

            // Alter view MobileWarehouse.vwMobileTransferStockHeader
            sql = @"ALTER VIEW [MobileWarehouse].[vwMobileTransferStockHeader]
AS
    SELECT mt_h.*,
        u_c.Initial AS CreatedInitial,
        u_u.Initial AS UpdatedInitial,
        u_a.Initial AS ApprovedInitial,
        u_r.Initial AS RejectedInitial,
        CASE mt_h.Mark
            WHEN 'A' THEN 'Aktif'
            WHEN 'APR' THEN 'Disetujui'
            WHEN 'REJ' THEN 'Ditolak' END AS [Status],
		ts_h.[Date],
		ts_h.WarehouseCodeFrom,
		ts_h.WarehouseInitialFrom,
		ts_h.WarehouseCodeTo,
		ts_h.WarehouseInitialTo
    FROM MobileWarehouse.MobileTransferStockHeader mt_h
    LEFT JOIN SystemManagement.[User] u_c
        ON u_c.Id = mt_h.CreatedBy
    LEFT JOIN SystemManagement.[User] u_u
        ON u_u.Id = mt_h.UpdatedBy
    LEFT JOIN SystemManagement.[User] u_a
        ON u_a.Id = mt_h.ApprovedBy
	LEFT JOIN SystemManagement.[User] u_r
        ON u_r.Id = mt_h.RejectedBy
	LEFT JOIN Inventory.vwTransferStockHeader ts_h
		ON ts_h.Code = mt_h.TransferCode";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
