using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AddViewDebitCreditPayment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = @"CREATE view [Finance].[vwDebitCreditPayment]
as
select TransCode, d.Amount, TypeAmount
from Finance.GeneralCashBankHeader h
join Finance.GeneralCashBankDetail d on h.Code = d.Code
where h.Mark = 'A'
union
select InvCode as TransCode, DebitMemoAmount as Amount, 'D' as TypeAmount
from Purchasing.PurchaseInvoiceDebitMemo 
union
select InvCode as TransCode, CreditMemoAmount as Amount, 'C' as TypeAmount
from Sales.SalesInvoiceCreditMemo ";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var sql = "DROP VIEW [Finance].[vwDebitCreditPayment]";
            migrationBuilder.Sql(sql);
        }
    }
}
