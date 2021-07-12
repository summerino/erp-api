using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP_API.Migrations.TenantMigrations
{
    public partial class AddViewVwApAndOther : Migration
    {
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			var sql = @"CREATE VIEW [Finance].[VwAP] AS
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

			sql = @"CREATE VIEW [Finance].[VwAR] AS
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

			sql = @"CREATE PROCEDURE [dbo].[sp_restore_cash_bank_transaction]
	@code varchar(17)
AS
BEGIN
SET NOCOUNT ON;	
	SELECT D.Id, D.TransCode as TransCode, D.[Type], D.Amount
	INTO #tmp_cb
	FROM Finance.GeneralCashBankHeader H
	JOIN Finance.GeneralCashBankDetail D ON H.Code = D.Code
	WHERE H.Code = @code

	-- RESTORE TRANSACTION
	DECLARE @Id int
	DECLARE @TransCode varchar(17)
	DECLARE @Type varchar(5)
	DECLARE @Amount decimal(18,2)
	
	DECLARE @IdDetail int
	DECLARE @TotalDetail Decimal(18, 2)
	DECLARE @TotalHeader Decimal(18, 2)
	DECLARE @PaidAmount Decimal(18, 2)
	DECLARE @ProRate Decimal(18, 2)
	WHILE EXISTS(SELECT * FROM #tmp_cb)
	BEGIN
		
		SELECT TOP 1 @Id = Id,@Type = [Type], @TransCode=TransCode, @Amount = Amount FROM #tmp_cb

		IF (@Type = 'AP')
			BEGIN
				
				SELECT @TotalHeader = Total, @PaidAmount = PaidAmount - @Amount   
				FROM Purchasing.PurchaseInvoiceHeader 
				WHERE Code=@TransCode

				UPDATE Purchasing.PurchaseInvoiceHeader SET PaidAmount=@PaidAmount WHERE Code=@TransCode

				-- update detail
				SELECT Id, Total, RcvCode
				INTO #tmp_pi
				FROM Purchasing.PurchaseInvoiceDetail 
				WHERE Code = @TransCode

				
				DECLARE @RcvCode varchar(17)

				WHILE EXISTS(SELECT * FROM #tmp_pi)
				BEGIN
					SELECT TOP 1 @IdDetail = Id, @TotalDetail=Total,@RcvCode = RcvCode FROM #tmp_pi
					SET @ProRate = @PaidAmount * @TotalDetail / @TotalHeader
					
					UPDATE Purchasing.PurchaseReceiveHeader SET PaidAmount=@ProRate WHERE Code=@RcvCode 
					
					DELETE FROM #tmp_pi
				END
				DROP TABLE #tmp_pi

			END
		ELSE IF (@Type = 'AR')
			BEGIN
				
				SELECT @TotalHeader = Total, @PaidAmount = PaidAmount - @Amount   
				FROM Sales.SalesInvoiceHeader 
				WHERE Code=@TransCode

				UPDATE Sales.SalesInvoiceHeader SET PaidAmount=@PaidAmount WHERE Code=@TransCode

				-- update detail
				SELECT Id, Total, DoCode
				INTO #tmp_si
				FROM Sales.SalesInvoiceDetail
				WHERE Code = @TransCode

				DECLARE @DoCode varchar(17)

				WHILE EXISTS(SELECT * FROM #tmp_si)
				BEGIN
					SELECT TOP 1 @IdDetail = Id, @TotalDetail=Total,@DoCode = DoCode FROM #tmp_si
					SET @ProRate = @PaidAmount * @TotalDetail / @TotalHeader
					
					UPDATE Sales.SalesDeliveryHeader SET PaidAmount=@ProRate WHERE Code=@DoCode 
					
					DELETE FROM #tmp_si
				END
				DROP TABLE #tmp_si
			END
		ELSE IF (@Type = 'DPC')
			BEGIN
				UPDATE Sales.CreditMemo SET Mark='PP' WHERE Code=@TransCode;
			END
		ELSE IF (@Type = 'DPS')
			BEGIN
				UPDATE Purchasing.DebitMemo SET Mark='PP' WHERE Code=@TransCode;
			END
		ELSE IF (@Type = 'PR' OR @Type = 'RDPS')
			BEGIN
				UPDATE Purchasing.DebitMemo SET Used = ((SELECT Used FROM Purchasing.DebitMemo where Code = @TransCode) - @Amount) WHERE Code = @TransCode
			END
		ELSE IF (@Type = 'RDPC' OR @Type = 'SR')
			BEGIN
				UPDATE Sales.CreditMemo SET Used = ((SELECT Used FROM Sales.CreditMemo where Code = @TransCode) - @Amount) WHERE Code = @TransCode
			END
		DELETE #tmp_cb WHERE Id = @Id
	END
	DROP TABLE #tmp_cb
 END";

			migrationBuilder.Sql(sql);

		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			var sql = @"DROP VIEW Finance.VwAP";
			migrationBuilder.Sql(sql);

			sql = @"DROP VIEW Finance.VwAR";
			migrationBuilder.Sql(sql);

			sql = "DROP PROCEDURE [dbo].[sp_restore_cash_bank_transaction]";
			migrationBuilder.Sql(sql);
		}
	}
}
