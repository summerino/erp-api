using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Web.API.Migrations.TenantMigrations
{
    public partial class CreateViewvwAdjustmentItem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = @"CREATE VIEW [Inventory].[vwAdjustmentItem]
AS 
SELECT i.*, 
	ISNULL(wq.QtyOnHand,0) AS QtyOnHand, w.code AS WarehouseCode,
	c.[Name] AS CategoryName,
	CASE i.TypeId
		WHEN 0 THEN 'Raw Material'
		WHEN 1 THEN 'Work In Process'
		WHEN 2 THEN 'Finished Good'
		WHEN 3 THEN 'Service'
		WHEN 4 THEN 'Consignment'
		WHEN 5 THEN 'Kits'
		WHEN 6 THEN 'Resell'
		ELSE '' END AS TypeName,
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
			var sql = "DROP VIEW [Inventory].[vwAdjustmentItem]";
			migrationBuilder.Sql(sql);
        }
    }
}
