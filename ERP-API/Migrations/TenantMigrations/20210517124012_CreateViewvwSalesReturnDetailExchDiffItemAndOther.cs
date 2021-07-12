using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Web.API.Migrations.TenantMigrations
{
    public partial class CreateViewvwSalesReturnDetailExchDiffItemAndOther : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = @"Create VIEW [Sales].[vwSalesReturnDetailExchDiffItem]
						AS
							SELECT sr_d.*,
								i.initial as ItemInitial,
								i.[Name] AS ItemName,
								i.UomSellId AS ItemUomSellId,
								uom_c_s.UnitEquivalent AS ItemUomSellName,
								i.SellPrice AS ItemSellPrice,
								uom.Initial AS UomInitial,
								uom_c.UnitEquivalent AS UnitName
							FROM Sales.SalesReturnDetailExchDiffItem sr_d
							LEFT JOIN Inventory.Item i
								ON i.Id = sr_d.ItemId
							LEFT JOIN Inventory.UoMConversion uom_c_s
								ON uom_c_s.Id = i.UomSellId
							LEFT JOIN Inventory.UoM uom
								ON uom.Id = sr_d.UomId
							LEFT JOIN Inventory.UoMConversion uom_c
								ON uom_c.Id = sr_d.UnitId";
            migrationBuilder.Sql(sql);

			sql = @"ALTER VIEW [Sales].[vwSalesReturnHeader]
					AS
						SELECT sr_h.*,
							CASE sr_h.type 
								When '1' THEN 'Tukar Memo'
								When '2' THEN 'Tukar Barang Sama'
								When '3' THEN 'Tukar Barang Beda' End As [ReturnType],
							c.[Name] AS CustName,
							e.Initial AS SalesInitial,
							u_c.Initial AS CreatedInitial,
							u_u.Initial AS UpdatedInitial,
							u_a.Initial AS ApprovedInitial,
							CASE sr_h.Mark
								WHEN 'A' THEN 'Active'
								WHEN 'V' THEN 'Void'
								WHEN 'PS' THEN 'Partial Shipped'
								WHEN 'CMP' THEN 'Completed'
								WHEN 'CLS' THEN 'Closed' END AS [Status]
						FROM Sales.SalesReturnHeader sr_h
						LEFT JOIN General.Customer c
							ON c.Code = sr_h.CustCode
						LEFT JOIN General.Employee e
							ON e.Id = sr_h.SalesBy
						LEFT JOIN SystemManagement.[User] u_c
							ON u_c.Id = sr_h.CreatedBy
						LEFT JOIN SystemManagement.[User] u_u
							ON u_u.Id = sr_h.UpdatedBy
						LEFT JOIN SystemManagement.[User] u_a
							ON u_a.Id = sr_h.ApprovedBy";
			migrationBuilder.Sql(sql);

			sql = @"CREATE VIEW [Purchasing].[VwPurchaseReturnDetailExchDiffItem]
					AS
						SELECT pr_d.*,
							i.[Name] AS ItemName,
							i.Initial AS ItemInitial,
							i.UomBuyId AS ItemUomBuyId,
							uom_c_b.UnitEquivalent AS ItemUomBuyName,
							i.BuyPrice AS ItemBuyPrice,
							uom.Initial AS UomInitial,
							uom_c.UnitEquivalent AS UnitName
						FROM Purchasing.PurchaseReturnDetailExchDiffItem pr_d
						LEFT JOIN Inventory.Item i
							ON i.Id = pr_d.ItemId
						LEFT JOIN Inventory.UoMConversion uom_c_b
							ON uom_c_b.Id = i.UomBuyId
						LEFT JOIN Inventory.UoM uom
							ON uom.Id = pr_d.UomId
						LEFT JOIN Inventory.UoMConversion uom_c
							ON uom_c.Id = pr_d.UnitId";
			migrationBuilder.Sql(sql);
		}

        protected override void Down(MigrationBuilder migrationBuilder)
        {
			var sql = "DROP VIEW [Purchasing].[VwPurchaseReturnDetailExchDiffItem]";
			migrationBuilder.Sql(sql);

			sql = "DROP VIEW [Sales].[vwSalesReturnDetailExchDiffItem]";
			migrationBuilder.Sql(sql);

			sql = @"ALTER VIEW [Sales].[vwSalesReturnHeader]
					AS
						SELECT sr_h.*,
							c.[Name] AS CustName,
							e.Initial AS SalesInitial,
							u_c.Initial AS CreatedInitial,
							u_u.Initial AS UpdatedInitial,
							u_a.Initial AS ApprovedInitial,
							CASE sr_h.Mark
								WHEN 'A' THEN 'Active'
								WHEN 'V' THEN 'Void'
								WHEN 'PS' THEN 'Partial Shipped'
								WHEN 'CMP' THEN 'Completed'
								WHEN 'CLS' THEN 'Closed' END AS[Status]
						FROM Sales.SalesReturnHeader sr_h
						LEFT JOIN General.Customer c
							ON c.Code = sr_h.CustCode
						LEFT JOIN General.Employee e
							ON e.Id = sr_h.SalesBy
						LEFT JOIN SystemManagement.[User] u_c
							ON u_c.Id = sr_h.CreatedBy
						LEFT JOIN SystemManagement.[User] u_u
							ON u_u.Id = sr_h.UpdatedBy
						LEFT JOIN SystemManagement.[User] u_a
							ON u_a.Id = sr_h.ApprovedBy";
			migrationBuilder.Sql(sql);
		}
    }
}
