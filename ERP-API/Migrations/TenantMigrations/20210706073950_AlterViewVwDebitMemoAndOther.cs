using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Web.API.Migrations.TenantMigrations
{
    public partial class AlterViewVwDebitMemoAndOther : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = @"ALTER VIEW [Sales].[vwCreditMemo]
AS
	SELECT m.*,
		m.Amount - m.Used AS Remaining,
		c.Initial AS CustInitial,
		c.[Name] AS CustName,
		CASE m.SrcTrans
			WHEN 1 THEN 'Deposit'
			WHEN 2 THEN 'Return'
			WHEN 3 THEN 'Return (Same Item)' END AS SrcTransName,
		CASE m.Mark
			WHEN 'A' THEN 'Active'
			WHEN 'V' THEN 'Void'
			WHEN 'PU' THEN 'Partial Used'
			WHEN 'FU' THEN 'Full Used' END AS [Status]
	FROM Sales.CreditMemo m
	LEFT JOIN General.Customer c
		ON c.Code = m.CustCode";

			migrationBuilder.Sql(sql);

			sql = @"ALTER VIEW [Purchasing].[vwDebitMemo]
AS
	SELECT m.*,
		m.Amount - m.Used AS Remaining,
		s.Initial AS SupInitial,
		s.[Name] AS SupName,
		CASE m.SrcTrans
			WHEN 1 THEN 'Deposit'
			WHEN 2 THEN 'Return'
			WHEN 3 THEN 'Return (Same Item)' END AS SrcTransName,
		CASE m.Mark
			WHEN 'A' THEN 'Active'
			WHEN 'V' THEN 'Void'
			WHEN 'PU' THEN 'Partial Used'
			WHEN 'FU' THEN 'Full Used' END AS [Status]
	FROM Purchasing.DebitMemo m
	LEFT JOIN General.Supplier s
		ON s.Code = m.SupCode";

			migrationBuilder.Sql(sql);

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
			var sql = @"ALTER VIEW [Sales].[vwCreditMemo]
AS
	SELECT m.*,
		c.[Name] AS CustName,
		CASE m.SrcTrans
			WHEN 1 THEN 'Deposit'
			WHEN 2 THEN 'Return'
			WHEN 3 THEN 'Return (Same Item)' END AS SrcTransName,
		CASE m.Mark
			WHEN 'A' THEN 'Active'
			WHEN 'V' THEN 'Void'
			WHEN 'PU' THEN 'Partial Used'
			WHEN 'FU' THEN 'Full Used' END AS [Status]
	FROM Sales.CreditMemo m
	LEFT JOIN General.Customer c
		ON c.Code = m.CustCode";

			migrationBuilder.Sql(sql);

			sql = @"ALTER VIEW [Purchasing].[vwDebitMemo]
AS
	SELECT m.*,
		s.[Name] AS SupName,
		CASE m.SrcTrans
			WHEN 1 THEN 'Deposit'
			WHEN 2 THEN 'Return'
			WHEN 3 THEN 'Return (Same Item)' END AS SrcTransName,
		CASE m.Mark
			WHEN 'A' THEN 'Active'
			WHEN 'V' THEN 'Void'
			WHEN 'PU' THEN 'Partial Used'
			WHEN 'FU' THEN 'Full Used' END AS [Status]
	FROM Purchasing.DebitMemo m
	LEFT JOIN General.Supplier s
		ON s.Code = m.SupCode";

			migrationBuilder.Sql(sql);
		}
    }
}
