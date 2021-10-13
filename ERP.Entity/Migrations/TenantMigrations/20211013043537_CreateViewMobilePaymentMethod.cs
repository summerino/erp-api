using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class CreateViewMobilePaymentMethod : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create view MobileSales.vwMobilePaymentMethod
            var sql = @"CREATE VIEW [MobileSales].[vwMobilePaymentMethod]
AS
	SELECT mp.*,
		u.Initial AS UpdatedInitial,
		c.[Name] AS CoaName
	FROM MobileSales.MobilePaymentMethod mp
	LEFT JOIN SystemManagement.[User] u
		ON u.Id = mp.UpdatedBy
	LEFT JOIN Accounting.COA c
		ON c.Code = mp.CoaCode";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
		{
			// Drop view MobileSales.vwMobilePaymentMethod
            var sql = @"DROP VIEW [MobileSales].[vwMobilePaymentMethod]";
            migrationBuilder.Sql(sql);
		}
    }
}
