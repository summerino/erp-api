using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP_API.Migrations.TenantMigrations
{
    public partial class UpdateViewSalesReturnDetail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			var sql = @"ALTER VIEW [Sales].[vwSalesReturnDetail]
						AS
							SELECT sr_d.*,
								i.Initial AS ItemInitial,
								i.[Name] AS ItemName,
								i.UomSellId AS ItemUomSellId,
								uom_c_s.UnitEquivalent AS ItemUomSellName,
								i.SellPrice AS ItemSellPrice,
								uom.Initial AS UomInitial,
								uom_c.UnitEquivalent AS UnitName
							FROM Sales.SalesReturnDetail sr_d
							LEFT JOIN Inventory.Item i
								ON i.Id = sr_d.ItemId
							LEFT JOIN Inventory.UoMConversion uom_c_s
								ON uom_c_s.Id = i.UomSellId
							LEFT JOIN Inventory.UoM uom
								ON uom.Id = sr_d.UomId
							LEFT JOIN Inventory.UoMConversion uom_c
								ON uom_c.Id = sr_d.UnitId
						GO";
			migrationBuilder.Sql(sql);
		}

        protected override void Down(MigrationBuilder migrationBuilder)
        {
			var sql = @"ALTER VIEW [Sales].[vwSalesReturnDetail]
						AS
							SELECT sr_d.*,
								i.[Name] AS ItemName,
								i.UomSellId AS ItemUomSellId,
								uom_c_s.UnitEquivalent AS ItemUomSellName,
								i.SellPrice AS ItemSellPrice,
								uom.Initial AS UomInitial,
								uom_c.UnitEquivalent AS UnitName
							FROM Sales.SalesReturnDetail sr_d
							LEFT JOIN Inventory.Item i
								ON i.Id = sr_d.ItemId
							LEFT JOIN Inventory.UoMConversion uom_c_s
								ON uom_c_s.Id = i.UomSellId
							LEFT JOIN Inventory.UoM uom
								ON uom.Id = sr_d.UomId
							LEFT JOIN Inventory.UoMConversion uom_c
								ON uom_c.Id = sr_d.UnitId
						GO";
			migrationBuilder.Sql(sql);
		}
    }
}
