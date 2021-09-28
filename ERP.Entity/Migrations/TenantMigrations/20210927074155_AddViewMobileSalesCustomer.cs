using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AddViewMobileSalesCustomer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = @"CREATE VIEW [MobileSales].[vwMobileCustomer]
                    AS
                        SELECT mc.*,
                            u_c.Initial AS CreatedInitial,
                            u_u.Initial AS UpdatedInitial,
                            u_a.Initial AS ApprovedInitial,
                            u_r.Initial AS RejectedInitial,
                            CASE mc.Mark
                                WHEN 'A' THEN 'Active'
                                WHEN 'APR' THEN 'Approved'
                                WHEN 'REJ' THEN 'Rejected' END AS [Status]
                        FROM MobileSales.MobileCustomer mc
                        LEFT JOIN SystemManagement.[User] u_c
                            ON u_c.Id = mc.CreatedBy
                        LEFT JOIN SystemManagement.[User] u_u
                            ON u_u.Id = mc.UpdatedBy
                        LEFT JOIN SystemManagement.[User] u_a
                            ON u_a.Id = mc.ApprovedBy
		                    LEFT JOIN SystemManagement.[User] u_r
                            ON u_r.Id = mc.RejectedBy
                    GO";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var sql = @"DROP VIEW [MobileSales].[vwMobileCustomer]";
            migrationBuilder.Sql(sql);
        }
    }
}
