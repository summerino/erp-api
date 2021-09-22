using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AlterViewReturn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
		{
            // Alter view Purchasing.vwPurchaseReturnHeader
            var sql = @"ALTER VIEW [Purchasing].[vwPurchaseReturnHeader]
AS
	SELECT pr_h.*,
		s.[Name] AS SupName,
		e.Initial AS ShippedInitial,
		u_c.Initial AS CreatedInitial,
		u_u.Initial AS UpdatedInitial,
		CASE pr_h.[Type] 
			When '1' THEN 'Tukar Memo'
			When '2' THEN 'Tukar Barang Sama'
			When '3' THEN 'Tukar Barang Beda' End As TypeName,
		CASE pr_h.Mark
			WHEN 'A' THEN 'Active'
			WHEN 'V' THEN 'Void'
			WHEN 'PR' THEN 'Partial Received'
			WHEN 'CMP' THEN 'Completed'
			WHEN 'CLS' THEN 'Closed' END AS [Status]
	FROM Purchasing.PurchaseReturnHeader pr_h
	LEFT JOIN General.Supplier s
		ON s.Code = pr_h.SupCode
	LEFT JOIN General.Employee e
		ON e.Id = pr_h.ShippedBy
	LEFT JOIN SystemManagement.[User] u_c
		ON u_c.Id = pr_h.CreatedBy
	LEFT JOIN SystemManagement.[User] u_u
		ON u_u.Id = pr_h.UpdatedBy";
            migrationBuilder.Sql(sql);

            // Alter view Sales.vwSalesReturnHeader
            sql = @"ALTER VIEW [Sales].[vwSalesReturnHeader]
AS
	SELECT sr_h.*,
		c.[Name] AS CustName,
		e.Initial AS SalesInitial,
		u_c.Initial AS CreatedInitial,
		u_u.Initial AS UpdatedInitial,
		u_a.Initial AS ApprovedInitial,
		CASE sr_h.[Type]
			When '1' THEN 'Tukar Memo'
			When '2' THEN 'Tukar Barang Sama'
			When '3' THEN 'Tukar Barang Beda' End As TypeName,
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
		}

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
