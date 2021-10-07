using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class CreateViewMobileVisitLog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create view MobileSales.vwMobileVisitLog
            var sql = @"CREATE VIEW [MobileSales].[vwMobileVisitLog]
AS
	SELECT vl.*,
		emp.Initial AS SalesmanInitial,
		emp.FirstName AS SalesmanName,
		cus.Initial AS CustomerInitial,
		cus.[Name] AS CustomerName,
		u_c.Initial AS CreatedInitial,
		u_u.Initial AS UpdatedInitial,
		u_a.Initial AS ApprovedInitial,
		u_r.Initial AS RejectedInitial,
		CASE vl.Mark
			WHEN 'A' THEN 'Active'
			WHEN 'APR' THEN 'Approved'
			WHEN 'REJ' THEN 'Rejected' END AS [Status]
	FROM MobileSales.MobileVisitLog vl
	LEFT JOIN General.Employee emp
		ON emp.Id = vl.SalesmanId
	LEFT JOIN General.Customer cus
		ON cus.Code = vl.CustCode
	LEFT JOIN SystemManagement.[User] u_c
		ON u_c.Id = vl.CreatedBy
	LEFT JOIN SystemManagement.[User] u_u
		ON u_u.Id = vl.UpdatedBy
	LEFT JOIN SystemManagement.[User] u_a
		ON u_a.Id = vl.ApprovedBy
	LEFT JOIN SystemManagement.[User] u_r
		ON u_r.Id = vl.RejectedBy";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop view MobileSales.vwMobileVisitLog
            var sql = @"DROP VIEW [MobileSales].[vwMobileVisitLog]";
            migrationBuilder.Sql(sql);
        }
    }
}
