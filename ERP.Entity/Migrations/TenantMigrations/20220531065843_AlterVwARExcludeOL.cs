using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AlterVwARExcludeOL : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			// Alter view Finance.vwAR
			var sql = @"ALTER VIEW [Finance].[vwAR]    
AS    
	SELECT B.Code, B.CustCode, C.[Name] AS CustName, [Date], CurrCode, Rate AS Rate,    
		Amount, PaidAmount, Amount - PaidAmount AS Remaining, B.Notes, 'BB' AS Src    
	FROM Accounting.BeginningBalanceAR B    
	JOIN General.Customer C    
		ON B.CustCode = C.Code    
	WHERE B.PaidAmount < B.Amount AND B.IsActive = 1    
	UNION ALL    
	SELECT S.Code,S.CustCode, C.[Name] AS CustName, S.[Date], S.CurrCode, 1 AS Rate,    
		Total AS Amount, PaidAmount, Total - PaidAmount AS Remaining, S.Notes, 'SI' AS Src    
	FROM Sales.SalesInvoiceHeader S    
	JOIN General.Customer C    
		ON S.CustCode = C.Code    
	WHERE S.PaidAmount < S.Total AND S.Mark NOT IN ('V', 'OL')";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
		{
			// Alter view Finance.vwAR
            var sql = @"ALTER VIEW [Finance].[vwAR]    
AS    
	SELECT B.Code, B.CustCode, C.[Name] AS CustName, [Date], CurrCode, Rate AS Rate,    
		Amount, PaidAmount, Amount - PaidAmount AS Remaining, B.Notes, 'BB' AS Src    
	FROM Accounting.BeginningBalanceAR B    
	JOIN General.Customer C    
		ON B.CustCode = C.Code    
	WHERE B.PaidAmount < B.Amount AND B.IsActive = 1    
	UNION ALL    
	SELECT S.Code,S.CustCode, C.[Name] AS CustName, S.[Date], S.CurrCode, 1 AS Rate,    
		Total AS Amount, PaidAmount, Total - PaidAmount AS Remaining, S.Notes, 'SI' AS Src    
	FROM Sales.SalesInvoiceHeader S    
	JOIN General.Customer C    
		ON S.CustCode = C.Code    
	WHERE S.PaidAmount < S.Total AND S.Mark = 'V'";
            migrationBuilder.Sql(sql);

		}
    }
}
