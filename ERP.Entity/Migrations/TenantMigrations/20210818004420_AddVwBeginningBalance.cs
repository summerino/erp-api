using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AddVwBeginningBalance : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = @"CREATE VIEW [Inventory].[vwBeginningBalanceStockHeader]
AS
	SELECT bb_h.*,
		w.Initial AS WarehouseInitial,
		u_c.Initial AS CreatedInitial,
		u_u.Initial AS UpdatedInitial		
	FROM Inventory.BeginningBalanceHeader bb_h
	LEFT JOIN Inventory.Warehouse w
		ON w.Code = bb_h.WarehouseCode
	LEFT JOIN SystemManagement.[User] u_c
		ON u_c.Id = bb_h.CreatedBy
	LEFT JOIN SystemManagement.[User] u_u
		ON u_u.Id = bb_h.UpdatedBy";
            migrationBuilder.Sql(sql);

            sql = @"CREATE VIEW [Inventory].[vwBeginningBalanceStockDetail]
AS
	SELECT bb_d.*,
		i.[Name] AS ItemName
	FROM Inventory.BeginningBalanceDetail bb_d
	LEFT JOIN Inventory.Item i
		ON i.Id = bb_d.ItemId
";
            migrationBuilder.Sql(sql);

			sql = @"CREATE VIEW [Inventory].[vwBeginningBalanceStockItem]
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
    ON u.Id = c.UpdatedBy
";
			migrationBuilder.Sql(sql);
		}

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var sql = @"DROP VIEW [Inventory].[vwBeginningBalanceStockHeader]";
            migrationBuilder.Sql(sql);

            sql = @"DROP VIEW [Inventory].[vwBeginningBalanceStockDetail]";
            migrationBuilder.Sql(sql);

			sql = @"DROP VIEW[Inventory].[vwBeginningBalanceStockItem]";
			migrationBuilder.Sql(sql);
		}
    }
}
