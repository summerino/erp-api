using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Web.API.Migrations.TenantMigrations
{
    public partial class AddViewVwGeneralCashBankHeaderAndOther : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = @"CREATE VIEW Finance.vwGeneralCashBankHeader as 
SELECT H.*, 
	c.[Name] AS CoaName, 
	u_c.Initial AS CreatedInitial,
    u_u.Initial AS UpdatedInitial,
    u_a.Initial AS ApprovedInitial,
    CASE h.Mark
        WHEN 'A' THEN 'Active'
        WHEN 'V' THEN 'Void'
        WHEN 'CMP' THEN 'Completed'
        WHEN 'CLS' THEN 'Closed' END AS [Status]
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

            sql = @"CREATE VIEW Finance.vwGeneralCashBankDetail as 
SELECT d.*, 
	c.[Name] AS CoaName
FROM Finance.GeneralCashBankDetail d
LEFT JOIN Accounting.COA c

    ON c.Code = d.CoaCode";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var sql = "DROP VIEW Finance.vwGeneralCashBankHeader";
            migrationBuilder.Sql(sql);

            sql = "DROP VIEW Finance.vwGeneralCashBankDetail";
            migrationBuilder.Sql(sql);
        }
    }
}
