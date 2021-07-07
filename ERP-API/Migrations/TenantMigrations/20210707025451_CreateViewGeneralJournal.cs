using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP_API.Migrations.TenantMigrations
{
    public partial class CreateViewGeneralJournal : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create view Accounting.vwGeneralJournalHeader
            var sql = @"CREATE VIEW [Accounting].[vwGeneralJournalHeader]
AS
	SELECT g.*,
		cr.Initial AS CreatedInitial,
		up.Initial AS UpdatedInitial,
		ap.Initial AS ApprovedInitial
	FROM Accounting.GeneralJournalHeader g
	LEFT JOIN SystemManagement.[User] cr
		ON g.CreatedBy = cr.Id
	LEFT JOIN SystemManagement.[User] up
		ON g.UpdatedBy = up.Id
	LEFT JOIN SystemManagement.[User] ap
		ON g.ApprovedBy = ap.Id";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop view Accounting.vwGeneralJournalHeader
            var sql = @"DROP VIEW [Accounting].[vwGeneralJournalHeader]";
            migrationBuilder.Sql(sql);
        }
    }
}
