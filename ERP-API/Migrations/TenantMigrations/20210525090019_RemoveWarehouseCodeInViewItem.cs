using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP_API.Migrations.TenantMigrations
{
    public partial class RemoveWarehouseCodeInViewItem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Alter view Inventory.vwItem
            var sql = @"ALTER VIEW [Inventory].[vwItem]
AS
    SELECT i.*,
        CASE i.TypeId
            WHEN 0 THEN 'Raw Material'
            WHEN 1 THEN 'Work In Process'
            WHEN 2 THEN 'Finished Good'
            WHEN 3 THEN 'Service'
            WHEN 4 THEN 'Consignment'
            WHEN 5 THEN 'Kits'
            WHEN 6 THEN 'Resell'
            ELSE '' END AS TypeName,
        c.[Name] AS CategoryName,
        uom.Initial AS UomInitial,
        uom_c_s.UnitEquivalent AS UomSellName,
        uom_c_b.UnitEquivalent AS UomBuyName,
        u_c.Initial AS CreatedInitial,
        u_u.Initial AS UpdatedInitial,
        wq.WarehouseCode,
        ISNULL(wq.QtyOnHand, 0) As QtyOnHand
    FROM Inventory.Item i
    LEFT JOIN Inventory.ItemCategory c
        ON c.Id = i.CategoryId
    LEFT JOIN Inventory.UoM uom
        ON uom.Id = i.UomId
    LEFT JOIN Inventory.UoMConversion uom_c_s
        ON uom_c_s.Id = i.UomSellId
    LEFT JOIN Inventory.UoMConversion uom_c_b
        ON uom_c_b.Id = i.UomBuyId
    LEFT JOIN SystemManagement.[User] u_c
        ON u_c.Id = c.CreatedBy
    LEFT JOIN SystemManagement.[User] u_u
        ON u_u.Id = c.UpdatedBy
    LEFT JOIN (
		SELECT WarehouseCode,ItemId,
			SUM(QtyOnHand) AS QtyOnHand,
			SUM(QtyOnOrder) AS QtyOnOrder,
			SUM(QtyOnIndent) AS QtyOnIndent,
			SUM(QtyOnTransfer) AS QtyOnTransfer
		FROM Inventory.WarehouseQuantity
		GROUP BY WarehouseCode, ItemId
	) wq
        ON wq.ItemId = i.Id";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
