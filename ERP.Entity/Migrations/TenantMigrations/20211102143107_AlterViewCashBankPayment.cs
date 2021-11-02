using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AlterViewCashBankPayment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			// Alter view Finance.vwGeneralCashBankHeader
			var sql = @"ALTER VIEW [Finance].[vwGeneralCashBankHeader]
AS
	SELECT h.*, 
		c.[Name] AS CoaName, 
		u_c.Initial AS CreatedInitial,
		u_u.Initial AS UpdatedInitial,
		u_a.Initial AS ApprovedInitial,
		CASE h.[Type]
			WHEN 'C' THEN 'Kas Bank Keluar'
			WHEN 'D' THEN 'Kas Bank Masuk' END AS [TypeName],
		CASE h.Mark
			WHEN 'A' THEN 'Active'
			WHEN 'V' THEN 'Void'
			WHEN 'CMP' THEN 'Completed'
			WHEN 'CLS' THEN 'Closed' END AS [Status]
	FROM (
		SELECT *
		FROM Finance.GeneralCashBankHeader
		WHERE IsInterCashBank = 0
	) h
	LEFT JOIN Accounting.COA c 
		ON c.Code = h.CoaCode
	LEFT JOIN SystemManagement.[User] u_c
		ON u_c.Id = h.CreatedBy
	LEFT JOIN SystemManagement.[User] u_u
		ON u_u.Id = h.UpdatedBy
	LEFT JOIN SystemManagement.[User] u_a
		ON u_a.Id = h.ApprovedBy";
            migrationBuilder.Sql(sql);

			// Alter view Finance.vwDebitCreditPayment
			sql = @"ALTER VIEW [Finance].[vwDebitCreditPayment]
AS
	SELECT h.Code, TransCode, d.Amount, TypeAmount
	FROM Finance.GeneralCashBankHeader h
	JOIN Finance.GeneralCashBankDetail d
		ON h.Code = d.Code
	WHERE h.Mark = 'A'
	UNION
	SELECT DebitMemoCode AS Code, InvCode AS TransCode, DebitMemoAmount AS Amount, 'D' AS TypeAmount
	FROM Purchasing.PurchaseInvoiceDebitMemo 
	UNION
	SELECT CreditMemoCode AS Code, InvCode AS TransCode, CreditMemoAmount AS Amount, 'C' AS TypeAmount
	FROM Sales.SalesInvoiceCreditMemo";
            migrationBuilder.Sql(sql);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
