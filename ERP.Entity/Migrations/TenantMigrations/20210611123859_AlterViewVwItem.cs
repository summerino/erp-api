using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AlterViewVwItem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        ISNULL(wq.QtyOnHand, 0) As QtyOnHand,
		(ISNULL(wq.QtyOnHand, 0) - ISNULL(wq.QtyOnOrder, 0)) / ISNULL((SELECT EXP(SUM(LOG(Conversion))) as result FROM Inventory.UoMConversion where uomid = i.uomid and seq <= uom_c_s.seq),1) as SellQtyAvailable,
		(ISNULL(wq.QtyOnHand, 0) - ISNULL(wq.QtyOnIndent, 0)) / ISNULL((SELECT EXP(SUM(LOG(Conversion))) as result FROM Inventory.UoMConversion where uomid = i.uomid and seq <= uom_c_b.seq),1) as BuyQtyAvailable
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
		SELECT ItemId,
			SUM(QtyOnHand) AS QtyOnHand,
			SUM(QtyOnOrder) AS QtyOnOrder,
			SUM(QtyOnIndent) AS QtyOnIndent,
			SUM(QtyOnTransfer) AS QtyOnTransfer
		FROM Inventory.WarehouseQuantity
		GROUP BY ItemId
	) wq
        ON wq.ItemId = i.Id";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
		SELECT ItemId,
			SUM(QtyOnHand) AS QtyOnHand,
			SUM(QtyOnOrder) AS QtyOnOrder,
			SUM(QtyOnIndent) AS QtyOnIndent,
			SUM(QtyOnTransfer) AS QtyOnTransfer
		FROM Inventory.WarehouseQuantity
		GROUP BY ItemId
	) wq
        ON wq.ItemId = i.Id";
            migrationBuilder.Sql(sql);
        }
    }
}
