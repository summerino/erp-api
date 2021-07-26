using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AlterProcGeneralCashBank : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Alter view Finance.vwGeneralCashBankHeader
            var sql = @"ALTER VIEW [Finance].[vwGeneralCashBankHeader] as 
SELECT H.*, 
	c.[Name] AS CoaName, 
	u_c.Initial AS CreatedInitial,
    u_u.Initial AS UpdatedInitial,
    u_a.Initial AS ApprovedInitial,
    CASE h.Mark
        WHEN 'A' THEN 'Active'
        WHEN 'V' THEN 'Void'
        WHEN 'CMP' THEN 'Completed'
        WHEN 'CLS' THEN 'Closed' END AS [Status],
	CASE h.Type
		WHEN 'C' THEN 'Kas Bank Keluar'
		WHEN 'D' THEN 'Kas Bank Masuk' END AS [TypeName]
FROM Finance.GeneralCashBankHeader h
LEFT JOIN Accounting.COA c 
	ON c.Code = h.CoaCode
LEFT JOIN SystemManagement.[User] u_c
    ON u_c.Id = h.CreatedBy
LEFT JOIN SystemManagement.[User] u_u
    ON u_u.Id = h.UpdatedBy
LEFT JOIN SystemManagement.[User] u_a
    ON u_a.Id = h.ApprovedBy";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
