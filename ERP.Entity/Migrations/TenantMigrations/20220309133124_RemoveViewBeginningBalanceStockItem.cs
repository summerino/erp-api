using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class RemoveViewBeginningBalanceStockItem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop view Inventory.vwBeginningBalanceStockItem
            var sql = @"DROP VIEW [Inventory].[vwBeginningBalanceStockItem]";
            migrationBuilder.Sql(sql);

            // Alter view Inventory.vwAdjustmentItem
            sql = @"ALTER VIEW [Inventory].[vwAdjustmentItem]
AS 
SELECT i.*, 
	ISNULL(wq.QtyOnHand,0) AS QtyOnHand, w.code AS WarehouseCode,
	c.[Name] AS CategoryName,
	uom.Initial AS UomInitial,
	uom_c_s.UnitEquivalent AS UomSellName,
	uom_c_b.UnitEquivalent AS UomBuyName,
	u.Initial AS UpdatedInitial
FROM Inventory.Warehouse w
LEFT JOIN Inventory.Item i
    ON 1 = 1
LEFT JOIN Inventory.ItemCategory c
    ON c.Id = i.CategoryId
LEFT JOIN Inventory.WarehouseQuantity wq
    ON wq.WarehouseCode = w.Code
    AND wq.ItemId = i.Id 
LEFT JOIN Inventory.UoM uom
    ON uom.Id = i.UomId
LEFT JOIN Inventory.UoMConversion uom_c_s
    ON uom_c_s.Id = i.UomSellId
LEFT JOIN Inventory.UoMConversion uom_c_b
    ON uom_c_b.Id = i.UomBuyId
LEFT JOIN SystemManagement.[User] u
    ON u.Id = c.UpdatedBy";
            migrationBuilder.Sql(sql);

            // Refresh view Inventory.vwWarehouse
            sql = @"exec sp_refreshview 'Inventory.vwWarehouse'";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
