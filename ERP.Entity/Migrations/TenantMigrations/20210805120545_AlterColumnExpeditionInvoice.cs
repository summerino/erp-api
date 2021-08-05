using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AlterColumnExpeditionInvoice : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PaidAmount",
                schema: "Expedition",
                table: "ExpeditionInvoiceHeader",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            // Reorder column Expedition.ExpeditionInvoiceHeader
            var sql = @"BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Expedition.ExpeditionInvoiceHeader
	DROP CONSTRAINT FK_ExpeditionInvoiceHeader_Supplier_SupCode
GO
ALTER TABLE General.Supplier SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE Expedition.Tmp_ExpeditionInvoiceHeader
	(
	Code varchar(17) NOT NULL,
	Date date NOT NULL,
	DueDate date NOT NULL,
	SrcTrans smallint NOT NULL,
	RefNo varchar(30) NULL,
	SupCode varchar(8) NOT NULL,
	CurrCode varchar(3) NOT NULL,
	Rate decimal(18, 2) NOT NULL,
	Amount decimal(18, 2) NOT NULL,
	PaidAmount decimal(18, 2) NOT NULL,
	Notes varchar(256) NULL,
	Mark varchar(3) NOT NULL,
	CreatedBy int NOT NULL,
	CreatedDate datetime NOT NULL,
	UpdatedBy int NOT NULL,
	UpdatedDate datetime NOT NULL,
	ApprovedBy int NULL,
	ApprovedDate datetime NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Expedition.Tmp_ExpeditionInvoiceHeader SET (LOCK_ESCALATION = TABLE)
GO
IF EXISTS(SELECT * FROM Expedition.ExpeditionInvoiceHeader)
	 EXEC('INSERT INTO Expedition.Tmp_ExpeditionInvoiceHeader (Code, Date, DueDate, SrcTrans, RefNo, SupCode, CurrCode, Rate, Amount, PaidAmount, Notes, Mark, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, ApprovedBy, ApprovedDate)
		SELECT Code, Date, DueDate, SrcTrans, RefNo, SupCode, CurrCode, Rate, Amount, PaidAmount, Notes, Mark, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, ApprovedBy, ApprovedDate FROM Expedition.ExpeditionInvoiceHeader WITH (HOLDLOCK TABLOCKX)')
GO
ALTER TABLE Expedition.ExpeditionInvoiceDetail
	DROP CONSTRAINT FK_ExpeditionInvoiceDetail_ExpeditionInvoiceHeader_Code
GO
DROP TABLE Expedition.ExpeditionInvoiceHeader
GO
EXECUTE sp_rename N'Expedition.Tmp_ExpeditionInvoiceHeader', N'ExpeditionInvoiceHeader', 'OBJECT' 
GO
ALTER TABLE Expedition.ExpeditionInvoiceHeader ADD CONSTRAINT
	PK_ExpeditionInvoiceHeader PRIMARY KEY CLUSTERED 
	(
	Code
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX IX_ExpeditionInvoiceHeader_SupCode ON Expedition.ExpeditionInvoiceHeader
	(
	SupCode
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE Expedition.ExpeditionInvoiceHeader ADD CONSTRAINT
	FK_ExpeditionInvoiceHeader_Supplier_SupCode FOREIGN KEY
	(
	SupCode
	) REFERENCES General.Supplier
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Expedition.ExpeditionInvoiceDetail ADD CONSTRAINT
	FK_ExpeditionInvoiceDetail_ExpeditionInvoiceHeader_Code FOREIGN KEY
	(
	Code
	) REFERENCES Expedition.ExpeditionInvoiceHeader
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Expedition.ExpeditionInvoiceDetail SET (LOCK_ESCALATION = TABLE)
GO
COMMIT";
            migrationBuilder.Sql(sql);

            // Alter view Expedition.vwExpeditionInvoiceHeader
            sql = @"ALTER VIEW [Expedition].[vwExpeditionInvoiceHeader]
AS
	SELECT e.*,
		Amount - PaidAmount AS Remaining,
		s.Initial AS SupInitial,
		s.[Name] AS SupName,
		c.Initial AS CreatedInitial,
		u.Initial AS UpdatedInitial,
		a.Initial AS ApprovedInitial
	FROM Expedition.ExpeditionInvoiceHeader e
	LEFT JOIN General.Supplier s
		ON e.SupCode = s.Code
	LEFT JOIN SystemManagement.[User] c
		ON c.Id = e.CreatedBy
	LEFT JOIN SystemManagement.[User] u
		ON u.Id = e.UpdatedBy
	LEFT JOIN SystemManagement.[User] a
		ON a.Id = e.ApprovedBy";
            migrationBuilder.Sql(sql);

			// Alter procedure dbo.sp_restore_cash_bank_transaction
			sql = @"ALTER PROCEDURE [dbo].[sp_restore_cash_bank_transaction]
	@code varchar(17)
AS
BEGIN

	SET NOCOUNT ON;

	SELECT d.Id, d.TransCode as TransCode, d.[Type], d.TransAmount, d.Src
	INTO #tmp_cb
	FROM Finance.GeneralCashBankHeader h
	JOIN Finance.GeneralCashBankDetail d ON h.Code = d.Code
	WHERE h.Code = @code

	-- Restore transaction
	DECLARE @id int
	DECLARE @type varchar(5), @transCode varchar(17), @src varchar(5)
	DECLARE @transAmount decimal(18, 2)
	
	DECLARE @idDetail int
	DECLARE @totalDetail Decimal(18, 2)
	DECLARE @totalHeader Decimal(18, 2)
	DECLARE @paidAmount Decimal(18, 2)
	DECLARE @proRate Decimal(18, 2)
	Declare @mark varchar(3)
	DECLARE @used Decimal(18, 2)

	WHILE EXISTS(SELECT * FROM #tmp_cb)
	BEGIN
		
		SELECT TOP 1 @id = Id, @type = [Type], @transCode = TransCode, @transAmount = TransAmount, @src = Src
		FROM #tmp_cb
		
		IF (@type = 'AR' AND @src = 'BB')
		BEGIN

			UPDATE Accounting.BeginningBalanceAR 
			SET PaidAmount = PaidAmount - @transAmount
			WHERE Code = @transCode;

		END
		ELSE IF (@type = 'AR')
		BEGIN

			SELECT @totalHeader = Total, @paidAmount = PaidAmount - @transAmount   
			FROM Sales.SalesInvoiceHeader 
			WHERE Code = @transCode
				
			SET @mark = 'A'
			IF (@paidAmount > 0) SET @mark = 'PP'
			
			UPDATE Sales.SalesInvoiceHeader SET PaidAmount = @paidAmount, Mark = @mark WHERE Code = @transCode

			-- Update detail
			SELECT Id, Total, DoCode
			INTO #tmp_si
			FROM Sales.SalesInvoiceDetail
			WHERE Code = @transCode

			DECLARE @doCode varchar(17)

			WHILE EXISTS(SELECT * FROM #tmp_si)
			BEGIN
				SELECT TOP 1 @idDetail = Id, @totalDetail = Total, @doCode = DoCode FROM #tmp_si
				SET @proRate = @paidAmount * @totalDetail / @totalHeader
					
				UPDATE Sales.SalesDeliveryHeader SET PaidAmount = @proRate WHERE Code = @doCode 
					
				DELETE FROM #tmp_si
			END

			DROP TABLE #tmp_si

		END
		ELSE IF (@type = 'AP' AND @src = 'BB')
		BEGIN

			UPDATE Accounting.BeginningBalanceAP 
			SET PaidAmount = PaidAmount - @transAmount
			WHERE Code = @transCode;

		END
		ELSE IF (@type = 'AP')
		BEGIN

			SELECT @totalHeader = Total, @paidAmount = PaidAmount - @transAmount   
			FROM Purchasing.PurchaseInvoiceHeader 
			WHERE Code = @transCode
				
			SET @mark = 'A'
			IF (@paidAmount > 0) SET @mark = 'PP'

			UPDATE Purchasing.PurchaseInvoiceHeader SET PaidAmount = @paidAmount, Mark = @mark WHERE Code = @transCode

			-- Update detail
			SELECT Id, Total, RcvCode
			INTO #tmp_pi
			FROM Purchasing.PurchaseInvoiceDetail 
			WHERE Code = @transCode
				
			DECLARE @rcvCode varchar(17)

			WHILE EXISTS(SELECT * FROM #tmp_pi)
			BEGIN
				SELECT TOP 1 @idDetail = Id, @totalDetail = Total, @rcvCode = RcvCode FROM #tmp_pi
				SET @proRate = @paidAmount * @totalDetail / @totalHeader
					
				UPDATE Purchasing.PurchaseReceiveHeader SET PaidAmount = @proRate WHERE Code = @rcvCode 
					
				DELETE FROM #tmp_pi
			END

			DROP TABLE #tmp_pi

		END
		ELSE IF (@type = 'EPAP')
		BEGIN

			UPDATE Expedition.ExpeditionInvoiceHeader
			SET PaidAmount = PaidAmount - @transAmount
			WHERE Code = @transCode;

		END
		ELSE IF (@type = 'DPC')
		BEGIN

			UPDATE Sales.CreditMemo
			SET Mark = 'PP'
			WHERE Code = @transCode;

		END
		ELSE IF (@type = 'DPS')
		BEGIN

			UPDATE Purchasing.DebitMemo
			SET Mark = 'PP'
			WHERE Code = @transCode;

		END
		ELSE IF ((@type = 'RDPC' OR @type = 'SR') AND @src = 'BB')
		BEGIN

			UPDATE Accounting.BeginningBalanceCreditMemo
			SET Used = Used - @transAmount
			WHERE Code = @transCode;

		END
		ELSE IF (@type = 'RDPC' OR @type = 'SR')
		BEGIN

			SET @used = ((SELECT Used FROM Sales.CreditMemo where Code = @transCode) - @transAmount)
			SET @mark = 'A'
			IF (@used > 0) SET @mark = 'PU'
			UPDATE Sales.CreditMemo SET Used = @used, Mark=@mark WHERE Code = @transCode

		END
		ELSE IF ((@type = 'RDPS' OR @type = 'PR') AND @src = 'BB')
		BEGIN

			UPDATE Accounting.BeginningBalanceDebitMemo
			SET Used = Used - @transAmount
			WHERE Code = @transCode;

		END
		ELSE IF (@type = 'RDPS' OR @type = 'PR')
		BEGIN

			SET @used = ((SELECT Used FROM Purchasing.DebitMemo where Code = @transCode) - @transAmount)
			SET @mark = 'A'
			IF (@used > 0) SET @mark = 'PU'
			UPDATE Purchasing.DebitMemo SET Used = @used, Mark = 'A' WHERE Code = @transCode

		END

		DELETE #tmp_cb WHERE Id = @id

	END

	DROP TABLE #tmp_cb

END";
            migrationBuilder.Sql(sql);

            //// Alter view Purchasing.vwDebitMemo
            //sql = @"ALTER VIEW [General].[vwApproval]";
            //migrationBuilder.Sql(sql);

            //// Alter view Purchasing.vwDebitMemo
            //sql = @"ALTER VIEW [General].[vwApproval]";
            //migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaidAmount",
                schema: "Expedition",
                table: "ExpeditionInvoiceHeader");
        }
    }
}
