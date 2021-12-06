using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AlterViewSalesOrder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Alter view Sales.vwSalesOrderHeader
            var sql = @"ALTER VIEW [Sales].[vwSalesOrderHeader]
AS
    SELECT so_h.*,
        c.[Name] AS CustName,
        e.Initial AS SalesInitial,
		e.FirstName AS SalesName,
        u_c.Initial AS CreatedInitial,
        u_u.Initial AS UpdatedInitial,
        u_a.Initial AS ApprovedInitial,
        CASE so_h.Mark
            WHEN 'A' THEN 'Active'
            WHEN 'V' THEN 'Void'
            WHEN 'PS' THEN 'Partial Shipped'
            WHEN 'CMP' THEN 'Completed'
            WHEN 'CLS' THEN 'Closed' END AS [Status]
    FROM Sales.SalesOrderHeader so_h
    LEFT JOIN General.Customer c
        ON c.Code = so_h.CustCode
    LEFT JOIN General.Employee e
        ON e.Id = so_h.SalesBy
    LEFT JOIN SystemManagement.[User] u_c
        ON u_c.Id = so_h.CreatedBy
    LEFT JOIN SystemManagement.[User] u_u
        ON u_u.Id = so_h.UpdatedBy
    LEFT JOIN SystemManagement.[User] u_a
        ON u_a.Id = so_h.ApprovedBy";
            migrationBuilder.Sql(sql);

            // Alter view Sales.vwSalesDeliveryHeader
            sql = @"ALTER VIEW [Sales].[vwSalesDeliveryHeader]
AS
	SELECT do_h.*,
		c.[Name] AS CustName,
		ca.Address1 AS CustAddress,
		sa.[Name] AS CustArea,
		e.Initial AS ShippedInitial,
		u.Initial AS UpdatedInitial,
		CASE do_h.Mark
			WHEN 'A' THEN 'Active'
			WHEN 'V' THEN 'Void'
			WHEN 'INV' THEN 'Invoiced' END AS [Status]
	FROM Sales.SalesDeliveryHeader do_h
	LEFT JOIN General.Customer c
		ON c.Code = do_h.CustCode
	LEFT JOIN General.CustomerAddress ca
		ON ca.Code = c.Code
		AND ca.IsDefault = 1
	LEFT JOIN Sales.Area sa
		ON sa.Id = c.AreaId1
	LEFT JOIN General.Employee e
		ON e.Id = do_h.ShippedBy
	LEFT JOIN SystemManagement.[User] u
		ON u.Id = do_h.UpdatedBy";
            migrationBuilder.Sql(sql);

            // Alter view Sales.vwSalesInvoiceHeader
            sql = @"ALTER VIEW [Sales].[vwSalesInvoiceHeader]
AS
    SELECT si_h.*,
		si_h.Total - si_h.PaidAmount AS Remaining,
        c.[Name] AS CustName,
		ca.Address1 AS CustAddress,
		sa.[Name] AS CustArea,
        e.Initial AS IssuedInitial,
        u_c.Initial AS CreatedInitial,
        u_u.Initial AS UpdatedInitial,
        u_a.Initial AS ApprovedInitial,
        CASE si_h.Mark
            WHEN 'A' THEN 'Active'
            WHEN 'PP' THEN 'Pending Payment'
            WHEN 'CMP' THEN 'Completed'
            WHEN 'V' THEN 'Void' END AS [Status]
    FROM Sales.SalesInvoiceHeader si_h
    LEFT JOIN General.Customer c
        ON c.Code = si_h.CustCode
	LEFT JOIN General.CustomerAddress ca
		ON ca.Code = c.Code
		AND ca.IsDefault = 1
	LEFT JOIN Sales.Area sa
		ON sa.Id = c.AreaId1
    LEFT JOIN General.Employee e
        ON e.Id = si_h.IssuedBy
    LEFT JOIN SystemManagement.[User] u_c
        ON u_c.Id = si_h.CreatedBy
    LEFT JOIN SystemManagement.[User] u_u
        ON u_u.Id = si_h.UpdatedBy
    LEFT JOIN SystemManagement.[User] u_a
        ON u_a.Id = si_h.ApprovedBy
";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
