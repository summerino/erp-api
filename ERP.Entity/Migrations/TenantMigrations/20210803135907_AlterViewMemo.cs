using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AlterViewMemo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			// Alter view Purchasing.vwDebitMemo
			var sql = @"ALTER VIEW [Purchasing].[vwDebitMemo]
AS
	SELECT m.*,
		m.Amount - m.Used AS Remaining,
		s.Initial AS SupInitial,
		s.[Name] AS SupName,
		CASE m.SrcTrans
			WHEN 1 THEN 'Deposit'
			WHEN 2 THEN 'Retur' END AS SrcTransName,
		CASE m.Mark
			WHEN 'A' THEN 'Active'
			WHEN 'V' THEN 'Void'
			WHEN 'PP' THEN 'Pending Payment'
			WHEN 'PU' THEN 'Partial Used'
			WHEN 'FU' THEN 'Full Used' END AS [Status]
	FROM Purchasing.DebitMemo m
	LEFT JOIN General.Supplier s
		ON s.Code = m.SupCode";
            migrationBuilder.Sql(sql);

			// Alter view Sales.vwCreditMemo
			sql = @"ALTER VIEW [Sales].[vwCreditMemo]
AS
	SELECT m.*,
		m.Amount - m.Used AS Remaining,
		c.Initial AS CustInitial,
		c.[Name] AS CustName,
		CASE m.SrcTrans
			WHEN 1 THEN 'Deposit'
			WHEN 2 THEN 'Retur' END AS typeName,
		CASE m.Mark
			WHEN 'A' THEN 'Active'
			WHEN 'V' THEN 'Void'
			WHEN 'PP' THEN 'Pending Payment'
			WHEN 'PU' THEN 'Partial Used'
			WHEN 'FU' THEN 'Full Used' END AS [Status]
	FROM Sales.CreditMemo m
	LEFT JOIN General.Customer c
		ON c.Code = m.CustCode";
            migrationBuilder.Sql(sql);

			// Alter view Accounting.vwBeginningBalanceCreditMemo
			sql = @"ALTER VIEW [Accounting].[vwBeginningBalanceCreditMemo]
AS
	SELECT cm.*,
		cm.Amount - cm.Used AS Remaining,
		CASE cm.[Type]
			WHEN 1 THEN 'Deposit'
			WHEN 2 THEN 'Retur' END AS TypeName,
		c.[Name] AS CustName,
		cr.Initial AS CreatedInitial,
		up.Initial AS UpdatedInitial
	FROM Accounting.BeginningBalanceCreditMemo cm
	LEFT JOIN General.Customer c
		ON cm.CustCode = c.Code
	LEFT JOIN SystemManagement.[User] cr
		ON cm.CreatedBy = cr.Id
	LEFT JOIN SystemManagement.[User] up
		ON cm.UpdatedBy = up.Id";
            migrationBuilder.Sql(sql);

			// Alter view Accounting.vwCreditMemo
			sql = @"ALTER VIEW [Accounting].[vwBeginningBalanceDebitMemo]
AS
	SELECT dm.*,
		dm.Amount - dm.Used AS Remaining,
		CASE dm.[Type]
			WHEN 1 THEN 'Deposit'
			WHEN 2 THEN 'Retur' END AS TypeName,
		s.[Name] AS SupName,
		cr.Initial AS CreatedInitial,
		up.Initial AS UpdatedInitial
	FROM Accounting.BeginningBalanceDebitMemo dm
	LEFT JOIN General.Supplier s
		ON dm.SupCode = s.Code
	LEFT JOIN SystemManagement.[User] cr
		ON dm.CreatedBy = cr.Id
	LEFT JOIN SystemManagement.[User] up
		ON dm.UpdatedBy = up.Id";
            migrationBuilder.Sql(sql);
		}

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
