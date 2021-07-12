using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class CreateViewVisitOrder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create view Sales.vwVisitOrderInvoice
            var sql = @"CREATE VIEW [Sales].[vwVisitOrderInvoice]
AS
	SELECT voi.*,
		c.[Name] AS CustomerName,
		sih.[Date] AS TransactionDate,
		sih.DueDate AS InvoiceDueDate,
		e.FirstName + ' ' + e.LastName AS SalesName,
		sih.Total
	FROM Sales.VisitOrderInvoice voi
	LEFT JOIN Sales.SalesInvoiceHeader sih
		ON voi.InvCode = sih.Code
	LEFT JOIN Sales.SalesOrderHeader soh
		ON sih.SOCode = soh.Code
	LEFT JOIN General.Employee e
		ON soh.SalesBy = e.Id
	LEFT JOIN General.Customer c
		ON sih.CustCode = c.Code";
            migrationBuilder.Sql(sql);

            // Alter view Sales.vwVisitOrder
            sql = @"ALTER VIEW [Sales].[vwVisitOrder]
AS
    SELECT vo.*,
		e.Initial AS SalesmanInitial,
		e.FirstName + ' ' + e.LastName AS SalesmanName,
		sg.Initial AS GroupInitial,
		sg.[Name] AS GroupName,
		CASE WHEN vo.VisitPlanCode IS NULL
			THEN 'Manual' 
			ELSE 'Rencana Kunjungan' END AS SourceTransaction,
        CASE vo.Mark
            WHEN 'A' THEN 'Active'
            WHEN 'V' THEN 'Void' END AS [Status],
        u_c.Initial AS CreatedInitial,
        u_u.Initial AS UpdatedInitial,
        u_a.Initial AS ApprovedInitial
    FROM Sales.VisitOrder vo
	LEFT JOIN General.Employee e
		ON vo.SalesmanId = e.Id
	LEFT JOIN Sales.SalesmanGroup sg
		ON e.SalesGroupId = sg.Id
    LEFT JOIN SystemManagement.[User] u_c
        ON u_c.Id = vo.CreatedBy
    LEFT JOIN SystemManagement.[User] u_u
        ON u_u.Id = vo.UpdatedBy
    LEFT JOIN SystemManagement.[User] u_a
        ON u_a.Id = vo.ApprovedBy";
            migrationBuilder.Sql(sql);

            // Alter view General.vwEmployee
            sql = @"ALTER VIEW [General].[vwEmployee]
AS
	SELECT e.*,
		sg.Initial AS GroupInitial,
		sg.[Name] AS GroupName,
		e.FirstName + ' ' + e.LastName AS FullName,
		CASE WHEN e.Sex = 1 THEN 'Pria' ELSE 'Wanita' END AS Gender,
		u.Initial AS UpdatedInitial
	FROM General.Employee e
	LEFT JOIN Sales.SalesmanGroup sg
		ON e.SalesGroupId = sg.Id
	LEFT JOIN SystemManagement.[User] u
		ON u.Id = e.UpdatedBy";
            migrationBuilder.Sql(sql);

			// Create view Sales.vwVisitOrderCustomer
			sql = @"CREATE VIEW [Sales].[vwVisitOrderCustomer]
AS
	SELECT voc.*,
		c.Initial AS CustomerInitial,
		c.[Name] AS CustomerName,
		ca.Address1 AS [Address],
		a1.[Name] AS AreaName1,
		a2.[Name] AS AreaName2,
		e.Initial AS ReplacemanInitial,
		e.FirstName + ' ' + e.LastName AS ReplacemanName
	FROM Sales.VisitOrderCustomer voc
	LEFT JOIN General.Customer c
		ON voc.CustCode = c.Code
	LEFT JOIN General.CustomerAddress ca
		ON c.Code = ca.Code AND ca.IsDefault = 1
	LEFT JOIN General.Employee e
		ON voc.ReplacingForSalesmanId = e.Id
	LEFT JOIN Sales.Area a1
		ON c.AreaId1 = a1.Id
	LEFT JOIN Sales.Area a2
		ON c.AreaId2 = a2.Id";
            migrationBuilder.Sql(sql);

			// Alter view SystemManagement.vwUser
			sql = @"ALTER VIEW [SystemManagement].[vwUser]
AS
    SELECT a.*,
        r.[Name] As RoleName,
        e.Initial As EmployeeInitial,
        u.Initial AS UpdatedInitial
    FROM SystemManagement.[User] a
    LEFT JOIN SystemManagement.[User] u
        ON u.Id = a.UpdatedBy
    LEFT JOIN General.Employee e
        ON a.EmployeeId = e.Id
    LEFT JOIN SystemManagement.[Role] r
        ON a.RoleId = r.Id";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop view Sales.vwVisitOrderInvoice
            var sql = @"DROP VIEW [Sales].[vwVisitOrderInvoice]";
            migrationBuilder.Sql(sql);

			// Drop view Sales.vwVisitOrderCustomer
			sql = @"DROP VIEW [Sales].[vwVisitOrderCustomer]";
            migrationBuilder.Sql(sql);
		}
    }
}
