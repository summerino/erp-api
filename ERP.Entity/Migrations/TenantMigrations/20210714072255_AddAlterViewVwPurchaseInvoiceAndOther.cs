using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AddAlterViewVwPurchaseInvoiceAndOther : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = @"ALTER VIEW [Purchasing].[vwPurchaseReceiveHeader]
AS
	SELECT pr_h.*,
		pr_h.Total - pr_h.PaidAmount AS Remaining,
		s.[Name] AS SupName,
		e.Initial AS ReceiveInitial,
		u_c.Initial AS CreatedInitial,
		u_u.Initial AS UpdatedInitial,
		CASE pr_h.Mark
			WHEN 'A' THEN 'Active'
			WHEN 'V' THEN 'Void'
			WHEN 'INV' THEN 'Invoiced' END AS [Status]
	FROM Purchasing.PurchaseReceiveHeader pr_h
	LEFT JOIN General.Supplier s
		ON s.Code = pr_h.SupCode
	LEFT JOIN General.Employee e
		ON e.Id = pr_h.ReceiveBy
	LEFT JOIN SystemManagement.[User] u_c
		ON u_c.Id = pr_h.CreatedBy
	LEFT JOIN SystemManagement.[User] u_u
		ON u_u.Id = pr_h.UpdatedBy";
            migrationBuilder.Sql(sql);

            sql = @"ALTER VIEW [Sales].[vwSalesInvoiceHeader]
AS
    SELECT si_h.*,
		si_h.Total - si_h.PaidAmount AS Remaining,
        c.[Name] AS CustName,
        e.Initial AS IssuedInitial,
        u_c.Initial AS CreatedInitial,
        u_u.Initial AS UpdatedInitial,
        u_a.Initial AS ApprovedInitial,
        CASE si_h.Mark
            WHEN 'A' THEN 'Active'
            WHEN 'V' THEN 'Void' END AS [Status]
    FROM Sales.SalesInvoiceHeader si_h
    LEFT JOIN General.Customer c
        ON c.Code = si_h.CustCode
    LEFT JOIN General.Employee e
        ON e.Id = si_h.IssuedBy
    LEFT JOIN SystemManagement.[User] u_c
        ON u_c.Id = si_h.CreatedBy
    LEFT JOIN SystemManagement.[User] u_u
        ON u_u.Id = si_h.UpdatedBy
    LEFT JOIN SystemManagement.[User] u_a
        ON u_a.Id = si_h.ApprovedBy";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var sql = @"ALTER VIEW [Purchasing].[vwPurchaseReceiveHeader]
AS
	SELECT pr_h.*,
		s.[Name] AS SupName,
		e.Initial AS ReceiveInitial,
		u_c.Initial AS CreatedInitial,
		u_u.Initial AS UpdatedInitial,
		CASE pr_h.Mark
			WHEN 'A' THEN 'Active'
			WHEN 'V' THEN 'Void'
			WHEN 'INV' THEN 'Invoiced' END AS [Status]
	FROM Purchasing.PurchaseReceiveHeader pr_h
	LEFT JOIN General.Supplier s
		ON s.Code = pr_h.SupCode
	LEFT JOIN General.Employee e
		ON e.Id = pr_h.ReceiveBy
	LEFT JOIN SystemManagement.[User] u_c
		ON u_c.Id = pr_h.CreatedBy
	LEFT JOIN SystemManagement.[User] u_u
		ON u_u.Id = pr_h.UpdatedBy";
            migrationBuilder.Sql(sql);

            sql = @"ALTER VIEW [Sales].[vwSalesInvoiceHeader]
AS
    SELECT si_h.*,
        c.[Name] AS CustName,
        e.Initial AS IssuedInitial,
        u_c.Initial AS CreatedInitial,
        u_u.Initial AS UpdatedInitial,
        u_a.Initial AS ApprovedInitial,
        CASE si_h.Mark
            WHEN 'A' THEN 'Active'
            WHEN 'V' THEN 'Void' END AS [Status]
    FROM Sales.SalesInvoiceHeader si_h
    LEFT JOIN General.Customer c
        ON c.Code = si_h.CustCode
    LEFT JOIN General.Employee e
        ON e.Id = si_h.IssuedBy
    LEFT JOIN SystemManagement.[User] u_c
        ON u_c.Id = si_h.CreatedBy
    LEFT JOIN SystemManagement.[User] u_u
        ON u_u.Id = si_h.UpdatedBy
    LEFT JOIN SystemManagement.[User] u_a
        ON u_a.Id = si_h.ApprovedBy";
            migrationBuilder.Sql(sql);
        }
    }
}
