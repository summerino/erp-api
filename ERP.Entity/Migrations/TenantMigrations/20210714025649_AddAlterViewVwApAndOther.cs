using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AddAlterViewVwApAndOther : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = @"ALTER VIEW [Finance].[VwAP] AS
SELECT B.Code, B.SupCode, S.[Name] AS SupName, [Date],CurrCode, CurrRate as Rate, Amount, PaidAmount, Amount - PaidAmount as Remaining, B.Notes
FROM Accounting.BeginningBalanceAP B
JOIN General.Supplier S ON B.SupCode = S.Code
WHERE B.PaidAmount < B.Amount AND B.IsActive = 1
UNION
SELECT P.Code,P.SupCode, S.[Name] as SupName, P.[Date],P.CurrCode, 1 AS Rate, Total as Amount, PaidAmount, Total - PaidAmount as Remaining, P.Notes 
FROM Purchasing.PurchaseInvoiceHeader P
JOIN General.Supplier S ON P.SupCode = S.Code
WHERE P.PaidAmount < P.Total AND P.Mark = 'A'";
            migrationBuilder.Sql(sql);

            sql = @"ALTER VIEW [Finance].[VwAR] AS
SELECT B.Code, B.CustCode, C.[Name] AS CustName, [Date],CurrCode, CurrRate as Rate , Amount, PaidAmount, Amount - PaidAmount as Remaining, B.Notes
FROM Accounting.BeginningBalanceAR B
JOIN General.Customer C ON B.CustCode = C.Code
WHERE B.PaidAmount < B.Amount AND B.IsActive = 1
UNION
SELECT S.Code,S.CustCode, C.[Name] as CustName, S.[Date],S.CurrCode, 1 AS Rate, Total as Amount, PaidAmount, Total - PaidAmount as Remaining, S.Notes 
FROM Sales.SalesInvoiceHeader S
JOIN General.Customer C ON S.CustCode = C.Code
WHERE S.PaidAmount < S.Total AND S.Mark = 'A'";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var sql = @"ALTER VIEW [Finance].[VwAP] AS
SELECT B.Code, B.SupCode, S.[Name] AS SupName, [Date],CurrCode, CurrRate as Rate, Amount, PaidAmount, Amount - PaidAmount as Remaining, B.Notes
FROM Accounting.BeginningBalanceAP B
JOIN General.Supplier S ON B.SupCode = S.Code
WHERE B.PaidAmount < B.Amount AND B.IsActive = 1
UNION
SELECT P.Code,P.SupCode, S.[Name] as SupName, P.[Date],P.CurrCode, CR.Amount AS Rate, Total as Amount, PaidAmount, Total - PaidAmount as Remaining, P.Notes 
FROM Purchasing.PurchaseInvoiceHeader P
JOIN General.Supplier S ON P.SupCode = S.Code
JOIN Accounting.CurrencyRate CR ON P.CurrCode = CR.CurrCode 
WHERE P.PaidAmount < P.Total AND P.Mark = 'A'";
            migrationBuilder.Sql(sql);

            sql = @"ALTER VIEW [Finance].[VwAR] AS
SELECT B.Code, B.CustCode, C.[Name] AS CustName, [Date],CurrCode, CurrRate as Rate , Amount, PaidAmount, Amount - PaidAmount as Remaining, B.Notes
FROM Accounting.BeginningBalanceAR B
JOIN General.Customer C ON B.CustCode = C.Code
WHERE B.PaidAmount < B.Amount AND B.IsActive = 1
UNION
SELECT S.Code,S.CustCode, C.[Name] as CustName, S.[Date],S.CurrCode, CR.Amount AS Rate, Total as Amount, PaidAmount, Total - PaidAmount as Remaining, S.Notes 
FROM Sales.SalesInvoiceHeader S
JOIN General.Customer C ON S.CustCode = C.Code
JOIN Accounting.CurrencyRate CR ON S.CurrCode = CR.CurrCode 
WHERE S.PaidAmount < S.Total AND S.Mark = 'A'";
            migrationBuilder.Sql(sql);
        }
    }
}
