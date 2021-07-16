using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AddViewVwPRAndOther : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = @"CREATE VIEW [Finance].[vwPR] AS
SELECT B.Code, B.SupCode, S.[Name] AS SupName, [Date],CurrCode, CurrRate as Rate, Amount, Used, Amount - Used as Remaining, B.Notes
FROM Accounting.BeginningBalanceDebitMemo B
JOIN General.Supplier S ON B.SupCode = S.Code
WHERE B.Used < B.Amount AND B.IsActive = 1
UNION
SELECT P.Code,P.SupCode, S.[Name] as SupName, P.[Date],P.CurrCode, 1 AS Rate, Amount, Used, Amount - Used as Remaining, P.Notes 
FROM Purchasing.vwDebitMemo P
JOIN General.Supplier S ON P.SupCode = S.Code
WHERE P.Used < P.Amount AND (P.Mark = 'A' OR P.Mark = 'PU')";
            migrationBuilder.Sql(sql);

            sql = @"CREATE VIEW [Finance].[vwSR] AS
SELECT B.Code, B.CustCode, C.[Name] AS CustName, [Date],CurrCode, CurrRate as Rate , Amount, Used, Amount - Used as Remaining, B.Notes
FROM Accounting.BeginningBalanceCreditMemo B
JOIN General.Customer C ON B.CustCode = C.Code
WHERE B.Used < B.Amount AND B.IsActive = 1
UNION
SELECT S.Code,S.CustCode, C.[Name] as CustName, S.[Date],S.CurrCode, 1 AS Rate, Amount, Used, Amount - Used as Remaining, S.Notes
FROM Sales.vwCreditMemo S
JOIN General.Customer C ON S.CustCode = C.Code
WHERE S.Used < S.Amount AND (S.Mark = 'A' OR S.Mark = 'PU')";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var sql = @"DROP VIEW [Finance].[vwPR]";
            migrationBuilder.Sql(sql);

            sql = @"DROP VIEW [Finance].[vwSR]";
            migrationBuilder.Sql(sql);
        }
    }
}
