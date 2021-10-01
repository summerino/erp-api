using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class CreateViewMobileCost : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create view MobileSales.vwMobileCostHeader
            var sql = @"CREATE VIEW [MobileSales].[vwMobileCostHeader]
AS
	SELECT h.*,
		emp.Initial AS SalesmanInitial,
		emp.FirstName AS SalesmanName,
		u_c.Initial AS CreatedInitial,
		u_u.Initial AS UpdatedInitial,
		u_a.Initial AS ApprovedInitial,
		u_r.Initial AS RejectedInitial,
		CASE h.Mark
			WHEN 'A' THEN 'Active'
			WHEN 'APR' THEN 'Approved'
			WHEN 'REJ' THEN 'Rejected' END AS [Status]
	FROM MobileSales.MobileCostHeader h
	LEFT JOIN General.Employee emp
			ON emp.Id = h.SalesmanId
	LEFT JOIN SystemManagement.[User] u_c
		ON u_c.Id = h.CreatedBy
	LEFT JOIN SystemManagement.[User] u_u
		ON u_u.Id = h.UpdatedBy
	LEFT JOIN SystemManagement.[User] u_a
		ON u_a.Id = h.ApprovedBy
	LEFT JOIN SystemManagement.[User] u_r
		ON u_r.Id = h.RejectedBy";
            migrationBuilder.Sql(sql);

            // Create view MobileSales.vwMobileCostDetail
            sql = @"CREATE VIEW [MobileSales].[vwMobileCostDetail]
AS
	SELECT d.*,
		c.[Name] AS CoaName
	FROM MobileSales.MobileCostDetail d
	LEFT JOIN Accounting.COA c
		ON c.Code = d.CoaCode";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop view MobileSales.vwMobileCostHeader
            var sql = @"DROP VIEW [MobileSales].[vwMobileCostHeader]";
            migrationBuilder.Sql(sql);

            // Drop view MobileSales.vwMobileCostDetail
            sql = @"DROP VIEW [MobileSales].[vwMobileCostDetail]";
            migrationBuilder.Sql(sql);
        }
    }
}
