using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class CreateViewMobileReason : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			// Create view MobileSales.vwMobileReason
			var sql = @"CREATE VIEW [MobileSales].[vwMobileReason]
AS
	SELECT mr.*,
		CASE 
		WHEN mr.Type = 'V' THEN 'Alasan Kunjungan'
		WHEN mr.Type = 'NV' THEN 'Alasan Tidak Berkunjung'
		WHEN mr.Type = 'USV' THEN 'Alasan Kunjungan Diluar Rute'
		WHEN mr.Type = 'NO' THEN 'Alasan Tidak Ada Pemesanan'
		END AS TypeName,
		u.Initial AS UpdatedInitial
	FROM MobileSales.MobileReason mr
	LEFT JOIN SystemManagement.[User] u
		ON u.Id = mr.UpdatedBy";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
		{
			// Drop view MobileSales.vwMobileReason
			var sql = "DROP VIEW [MobileSales].[vwMobileReason]";
			migrationBuilder.Sql(sql);
		}
    }
}
