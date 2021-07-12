using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class CreateInitialView : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			// Create view vw_items
			var sql = @"CREATE VIEW [dbo].[vw_items]
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
		uom_c_b.UnitEquivalent AS UomBuyName
	FROM msItems i
	LEFT JOIN msItemCategories c
		ON c.Id = i.CategoryId
	LEFT JOIN msUoMs uom
		ON uom.Id = i.UomId
	LEFT JOIN msUoMConversions uom_c_s
		ON uom_c_s.Id = i.UomSellId
	LEFT JOIN msUoMConversions uom_c_b
		ON uom_c_b.Id = i.UomBuyId";
            migrationBuilder.Sql(sql);

			// Create view vw_po_h
			sql = @"CREATE VIEW [dbo].[vw_po_h]
AS
	SELECT po_h.*,
		s.[Name] AS SupName,
		e.Initial AS RequestInitial
	FROM trPurchaseOrderHeaders po_h
	LEFT JOIN msSuppliers s
		ON s.Code = po_h.SupCode
	LEFT JOIN msEmployees e
		ON e.Id = po_h.RequestBy";
            migrationBuilder.Sql(sql);

			// Create view vw_po_d
			sql = @"CREATE VIEW [dbo].[vw_po_d]
AS
	SELECT po_d.*,
		i.[Name] AS ItemName,
		i.UomBuyId AS ItemUomBuyId,
		uom_c_b.UnitEquivalent AS ItemUomBuyName,
		i.BuyPrice AS ItemBuyPrice,
		uom.Initial AS UomInitial,
		uom_c.UnitEquivalent AS UnitName
	FROM trPurchaseOrderDetails po_d
	LEFT JOIN msItems i
		ON i.Id = po_d.ItemId
	LEFT JOIN msUoMConversions uom_c_b
		ON uom_c_b.Id = i.UomBuyId
	LEFT JOIN msUoMs uom
		ON uom.Id = po_d.UomId
	LEFT JOIN msUoMConversions uom_c
		ON uom_c.Id = po_d.UnitId";
            migrationBuilder.Sql(sql);

			// Create view vw_pr_h
            sql = @"CREATE VIEW [dbo].[vw_pr_h]
AS
	SELECT pr_h.*,
		s.[Name] AS SupName,
		e.Initial AS ReceiveInitial
	FROM trPurchaseReceiveHeaders pr_h
	LEFT JOIN msSuppliers s
		ON s.Code = pr_h.SupCode
	LEFT JOIN msEmployees e
		ON e.Id = pr_h.ReceiveBy";
            migrationBuilder.Sql(sql);

			// Create view vw_pr_d
            sql = @"CREATE VIEW [dbo].[vw_pr_d]
AS
	SELECT pr_d.*,
		ISNULL(po_d.Qty, 0) AS OrderQty,
		ISNULL(po_d.Qty, 0) - ISNULL(po_d.QtyRcv, 0) AS OutstandingQty,
		i.[Name] AS ItemName,
		i.UomBuyId AS ItemUomBuyId,
		uom_c_b.UnitEquivalent AS ItemUomBuyName,
		i.BuyPrice AS ItemBuyPrice,
		uom.Initial AS UomInitial,
		uom_c.UnitEquivalent AS UnitName
	FROM trPurchaseReceiveDetails pr_d
	LEFT JOIN trPurchaseOrderDetails po_d
		ON po_d.Id = pr_d.PODetailId
	LEFT JOIN msItems i
		ON i.Id = pr_d.ItemId
	LEFT JOIN msUoMConversions uom_c_b
		ON uom_c_b.Id = i.UomBuyId
	LEFT JOIN msUoMs uom
		ON uom.Id = pr_d.UomId
	LEFT JOIN msUoMConversions uom_c
		ON uom_c.Id = pr_d.UnitId";
            migrationBuilder.Sql(sql);
		}

        protected override void Down(MigrationBuilder migrationBuilder)
        {
			// Drop view vw_items
			migrationBuilder.Sql(@"DROP VIEW [dbo].[vw_items]");

			// Drop view vw_po_h
			migrationBuilder.Sql(@"DROP VIEW [dbo].[vw_po_h]");

			// Drop view vw_po_d
			migrationBuilder.Sql(@"DROP VIEW [dbo].[vw_po_d]");

			// Drop view vw_pr_h
			migrationBuilder.Sql(@"DROP VIEW [dbo].[vw_pr_h]");

			// Drop view vw_pr_d
			migrationBuilder.Sql(@"DROP VIEW [dbo].[vw_pr_d]");
		}
    }
}
