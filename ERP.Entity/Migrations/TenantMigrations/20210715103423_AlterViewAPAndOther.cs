using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AlterViewAPAndOther : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = @"ALTER VIEW [Purchasing].[vwDebitMemo]
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
			WHEN 'PP' THEN 'Pending Payment'
			WHEN 'PU' THEN 'Partial Used'
			WHEN 'FU' THEN 'Full Used' END AS [Status]
	FROM Purchasing.DebitMemo m
	LEFT JOIN General.Supplier s
		ON s.Code = m.SupCode";
            migrationBuilder.Sql(sql);

            sql = @"ALTER VIEW [Sales].[vwCreditMemo]
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
			WHEN 'PP' THEN 'Pending Payment'
			WHEN 'PU' THEN 'Partial Used'
			WHEN 'FU' THEN 'Full Used' END AS [Status]
	FROM Sales.CreditMemo m
	LEFT JOIN General.Customer c
		ON c.Code = m.CustCode";
            migrationBuilder.Sql(sql);

			sql = @"ALTER VIEW [Finance].[VwAP] AS
SELECT B.Code, B.SupCode, S.[Name] AS SupName, [Date],CurrCode, CurrRate as Rate, Amount, PaidAmount, Amount - PaidAmount as Remaining, B.Notes
FROM Accounting.BeginningBalanceAP B
JOIN General.Supplier S ON B.SupCode = S.Code
WHERE B.PaidAmount < B.Amount AND B.IsActive = 1
UNION
SELECT P.Code,P.SupCode, S.[Name] as SupName, P.[Date],P.CurrCode, 1 AS Rate, Total as Amount, PaidAmount, Total - PaidAmount as Remaining, P.Notes 
FROM Purchasing.PurchaseInvoiceHeader P
JOIN General.Supplier S ON P.SupCode = S.Code
WHERE P.PaidAmount < P.Total AND (P.Mark = 'A' OR P.Mark = 'PP')";
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
WHERE S.PaidAmount < S.Total AND (S.Mark = 'A' OR S.Mark = 'PP')";
			migrationBuilder.Sql(sql);

			sql = @"ALTER PROCEDURE [dbo].[sp_restore_cash_bank_transaction]
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
	Declare @Mark varchar(3)
	DECLARE @Used Decimal(18, 2)
	WHILE EXISTS(SELECT * FROM #tmp_cb)
	BEGIN
		
		SELECT TOP 1 @Id = Id,@Type = [Type], @TransCode=TransCode, @Amount = Amount FROM #tmp_cb

		IF (@Type = 'AP')
			BEGIN
				
				SELECT @TotalHeader = Total, @PaidAmount = PaidAmount - @Amount   
				FROM Purchasing.PurchaseInvoiceHeader 
				WHERE Code=@TransCode
				
				SET @Mark = 'A'
				IF(@PaidAmount > 0)
					BEGIN
						SET @Mark = 'PP'
					END

				UPDATE Purchasing.PurchaseInvoiceHeader SET PaidAmount=@PaidAmount, Mark=@Mark WHERE Code=@TransCode

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
				
				SET @Mark = 'A'
				IF(@PaidAmount > 0)
					BEGIN
						SET @Mark = 'PP'
					END
				
				UPDATE Sales.SalesInvoiceHeader SET PaidAmount=@PaidAmount, Mark=@Mark WHERE Code=@TransCode

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
				
				SET @Used = ((SELECT Used FROM Purchasing.DebitMemo where Code = @TransCode) - @Amount)
				SET @Mark = 'A'
				IF(@Used > 0)
					BEGIN
						SET @Mark = 'PU'
					END
				UPDATE Purchasing.DebitMemo SET Used = @Used, Mark='A' WHERE Code = @TransCode
			END
		ELSE IF (@Type = 'RDPC' OR @Type = 'SR')
			BEGIN
				SET @Used = ((SELECT Used FROM Sales.CreditMemo where Code = @TransCode) - @Amount)
				SET @Mark = 'A'
				IF(@Used > 0)
					BEGIN
						SET @Mark = 'PU'
					END
				UPDATE Sales.CreditMemo SET Used = @Used, Mark=@Mark WHERE Code = @TransCode
			END
		DELETE #tmp_cb WHERE Id = @Id
	END
	DROP TABLE #tmp_cb
 END";
			migrationBuilder.Sql(sql);

			sql = @"ALTER VIEW [Purchasing].[vwPurchaseInvoiceHeader]
AS
    SELECT pi_h.*,
		pi_h.Total - pi_h.PaidAmount as Remaining,
        s.[Name] AS SupName,
        e.Initial AS IssuedInitial,
        u_c.Initial AS CreatedInitial,
        u_u.Initial AS UpdatedInitial,
        u_a.Initial AS ApprovedInitial,
        CASE pi_h.Mark
            WHEN 'A' THEN 'Active'
            WHEN 'PP' THEN 'Partial Payment'
            WHEN 'CMP' THEN 'Completed'
            WHEN 'V' THEN 'Void' END AS [Status]
    FROM Purchasing.PurchaseInvoiceHeader pi_h
    LEFT JOIN General.Supplier s
        ON s.Code = pi_h.SupCode
    LEFT JOIN General.Employee e
        ON e.Id = pi_h.IssuedBy
    LEFT JOIN SystemManagement.[User] u_c
        ON u_c.Id = pi_h.CreatedBy
    LEFT JOIN SystemManagement.[User] u_u
        ON u_u.Id = pi_h.UpdatedBy
    LEFT JOIN SystemManagement.[User] u_a
        ON u_a.Id = pi_h.ApprovedBy";
			migrationBuilder.Sql(sql);

			sql = @"ALTER VIEW [Sales].[vwSalesInvoiceHeader]
AS
    SELECT si_h.*,
		si_h.Total - si_h.PaidAmount AS Remaining,
        c.[Name] AS CustName,
        e.Initial AS IssuedInitial,
        u_c.Initial AS CreatedInitial,
        u_u.Initial AS UpdatedInitial,
        u_a.Initial AS ApprovedInitial,
        CASE si_h.Mark
            WHEN 'A' THEN 'Active'
            WHEN 'PP' THEN 'Pending Payment'
            WHEN 'CMP' THEN 'Completed'
            WHEN 'V' THEN 'Void' END AS [Status]
    FROM Sales.SalesInvoiceHeader si_h
    LEFT JOIN General.Customer c
        ON c.Code = si_h.CustCode
    LEFT JOIN General.Employee e
        ON e.Id = si_h.IssuedBy
    LEFT JOIN SystemManagement.[User] u_c
        ON u_c.Id = si_h.CreatedBy
    LEFT JOIN SystemManagement.[User] u_u
        ON u_u.Id = si_h.UpdatedBy
    LEFT JOIN SystemManagement.[User] u_a
        ON u_a.Id = si_h.ApprovedBy";
			migrationBuilder.Sql(sql);
			
		}

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var sql = @"ALTER VIEW [Purchasing].[vwDebitMemo]
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

            sql = @"ALTER VIEW [Sales].[vwCreditMemo]
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
			WHEN 'PP' THEN 'Partial Payment'
			WHEN 'PU' THEN 'Partial Used'
			WHEN 'FU' THEN 'Full Used' END AS [Status]
	FROM Sales.CreditMemo m
	LEFT JOIN General.Customer c
		ON c.Code = m.CustCode";
            migrationBuilder.Sql(sql);

			sql = @"ALTER VIEW [Finance].[VwAP] AS
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

			sql = @"ALTER PROCEDURE [dbo].[sp_restore_cash_bank_transaction]
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

			sql = @"ALTER VIEW [Purchasing].[vwPurchaseInvoiceHeader]
AS
    SELECT pi_h.*,
		pi_h.Total - pi_h.PaidAmount as Remaining,
        s.[Name] AS SupName,
        e.Initial AS IssuedInitial,
        u_c.Initial AS CreatedInitial,
        u_u.Initial AS UpdatedInitial,
        u_a.Initial AS ApprovedInitial,
        CASE pi_h.Mark
            WHEN 'A' THEN 'Active'
            WHEN 'V' THEN 'Void' END AS [Status]
    FROM Purchasing.PurchaseInvoiceHeader pi_h
    LEFT JOIN General.Supplier s
        ON s.Code = pi_h.SupCode
    LEFT JOIN General.Employee e
        ON e.Id = pi_h.IssuedBy
    LEFT JOIN SystemManagement.[User] u_c
        ON u_c.Id = pi_h.CreatedBy
    LEFT JOIN SystemManagement.[User] u_u
        ON u_u.Id = pi_h.UpdatedBy
    LEFT JOIN SystemManagement.[User] u_a
        ON u_a.Id = pi_h.ApprovedBy";
			migrationBuilder.Sql(sql);

			sql = @"ALTER VIEW [Sales].[vwSalesInvoiceHeader]
AS
    SELECT si_h.*,
		si_h.Total - si_h.PaidAmount AS Remaining,
        c.[Name] AS CustName,
        e.Initial AS IssuedInitial,
        u_c.Initial AS CreatedInitial,
        u_u.Initial AS UpdatedInitial,
        u_a.Initial AS ApprovedInitial,
        CASE si_h.Mark
            WHEN 'A' THEN 'Active'
            WHEN 'V' THEN 'Void' END AS [Status]
    FROM Sales.SalesInvoiceHeader si_h
    LEFT JOIN General.Customer c
        ON c.Code = si_h.CustCode
    LEFT JOIN General.Employee e
        ON e.Id = si_h.IssuedBy
    LEFT JOIN SystemManagement.[User] u_c
        ON u_c.Id = si_h.CreatedBy
    LEFT JOIN SystemManagement.[User] u_u
        ON u_u.Id = si_h.UpdatedBy
    LEFT JOIN SystemManagement.[User] u_a
        ON u_a.Id = si_h.ApprovedBy";
			migrationBuilder.Sql(sql);
		}
    }
}
