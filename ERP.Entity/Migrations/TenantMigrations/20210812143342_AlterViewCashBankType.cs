using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AlterViewCashBankType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			// Alter view Finance.vwCashBankType
			var sql = @"ALTER VIEW [Finance].[vwCashBankType]
AS
	SELECT t.*,
		sp.[Value] AS CoaCode,
		c.[Name] AS CoaName,
		CASE t.Code
			WHEN 'AR' THEN 31
			WHEN 'AP' THEN 32
			WHEN 'EPAP' THEN 33
			WHEN 'TU' THEN 34
			WHEN 'DPC' THEN 35
			WHEN 'RDPC' THEN 36
			WHEN 'DPS' THEN 37
			WHEN 'RDPS' THEN 38
			WHEN 'SR' THEN 39
			WHEN 'PR' THEN 40
			ELSE 0 END AS ActionId
	FROM Finance.CashBankType t
	LEFT JOIN SystemManagement.SystemParameter sp
		ON sp.Code = t.SysParCode
	LEFT JOIN Accounting.COA c
		ON c.Code = sp.[Value]";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
