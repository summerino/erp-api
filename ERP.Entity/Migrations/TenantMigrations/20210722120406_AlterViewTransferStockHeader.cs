using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AlterViewTransferStockHeader : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Alter view Inventory.vwTransferStockHeader
            var sql = @"ALTER VIEW [Inventory].[vwTransferStockHeader]
AS
    SELECT ts_h.*,
        w_f.Initial AS WarehouseInitialFrom,
        w_t.Initial AS WarehouseInitialTo,
        u_c.Initial AS CreatedInitial,
        u_u.Initial AS UpdatedInitial,
        u_a.Initial AS ApprovedInitial,
        CASE ts_h.Mark
            WHEN 'A' THEN 'Active'
            WHEN 'V' THEN 'Void' END AS [Status],
        CASE ts_h.[Type]
            WHEN 'OUT' THEN 'Barang Keluar'
            WHEN 'IN' THEN 'Barang Masuk'
            WHEN 'DT' THEN 'Transfer Langsung' END AS TypeInitial
    FROM Inventory.TransferStockHeader ts_h
    LEFT JOIN Inventory.Warehouse w_f
        ON w_f.Code = ts_h.WarehouseCodeFrom
    LEFT JOIN Inventory.Warehouse w_t
        ON w_t.Code = ts_h.WarehouseCodeTo
    LEFT JOIN SystemManagement.[User] u_c
        ON u_c.Id = ts_h.CreatedBy
    LEFT JOIN SystemManagement.[User] u_u
        ON u_u.Id = ts_h.UpdatedBy
    LEFT JOIN SystemManagement.[User] u_a
        ON u_a.Id = ts_h.ApprovedBy";
            migrationBuilder.Sql(sql);

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
