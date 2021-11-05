using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AddingLatLngInMobileCustomer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Lat",
                schema: "MobileSales",
                table: "MobileCustomer",
                type: "decimal(9,6)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Lng",
                schema: "MobileSales",
                table: "MobileCustomer",
                type: "decimal(9,6)",
                nullable: true);

            // Alter view Expedition.vwExpeditionInvoiceHeader
            var sql = @"ALTER VIEW [Expedition].[vwExpeditionInvoiceHeader]
AS
	SELECT e.*,
		Amount - PaidAmount AS Remaining,
		s.Initial AS SupInitial,
		s.[Name] AS SupName,
		c.Initial AS CreatedInitial,
		u.Initial AS UpdatedInitial,
		a.Initial AS ApprovedInitial,
        CASE e.Mark
            WHEN 'A' THEN 'Active'
            WHEN 'PP' THEN 'Partial Payment'
            WHEN 'CMP' THEN 'Completed'
            WHEN 'V' THEN 'Void' END AS [Status]
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

			SELECT @totalHeader = Amount, @paidAmount = PaidAmount - @transAmount   
			FROM Expedition.ExpeditionInvoiceHeader 
			WHERE Code = @transCode

			SET @mark = 'A'
			IF (@paidAmount > 0) SET @mark = 'PP'

			UPDATE Expedition.ExpeditionInvoiceHeader SET PaidAmount = @paidAmount, Mark = @mark WHERE Code = @transCode;

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

			// Alter view Purchasing.vwPurchaseReceiveDetail
			sql = @"ALTER VIEW [Purchasing].[vwPurchaseReceiveDetail]
AS
	SELECT rcv_d.*,
		CASE WHEN rcv_h.SrcTrans = 1 THEN ISNULL(po_d.Qty, 0)
			 WHEN rtn_h.Type = 2 THEN ISNULL(rtn_d.Qty, 0)
			 WHEN rtn_h.Type = 3 THEN ISNULL(rtnx_d.Qty, 0)
			 ELSE 0 END AS OrderQty,
		CASE WHEN rcv_h.SrcTrans = 1 THEN ISNULL(po_d.Qty, 0) - ISNULL(po_d.QtyRcv, 0)
			 WHEN rtn_h.Type = 2 THEN ISNULL(rtn_d.Qty, 0) - ISNULL(rtn_d.QtyRcv, 0)
			 WHEN rtn_h.Type = 3 THEN ISNULL(rtnx_d.Qty, 0) - ISNULL(rtnx_d.QtyRcv, 0)
			 ELSE 0 END AS OutstandingQty,
		i.Initial AS ItemInitial,
		i.[Name] AS ItemName,
		i.UomBuyId AS ItemUomBuyId,
		uom_c_b.UnitEquivalent AS ItemUomBuyName,
		i.BuyPrice AS ItemBuyPrice,
		uom.Initial AS UomInitial,
		uom_c.UnitEquivalent AS UnitName
	FROM Purchasing.PurchaseReceiveDetail rcv_d
	LEFT JOIN Purchasing.PurchaseReceiveHeader rcv_h
		ON rcv_h.Code = rcv_d.Code
	LEFT JOIN Purchasing.PurchaseReturnHeader rtn_h
		ON rtn_h.Code = rcv_h.TransCode
	LEFT JOIN Purchasing.PurchaseOrderDetail po_d
		ON po_d.Id = rcv_d.TransDetailId
		AND rcv_h.SrcTrans = 1
	LEFT JOIN Purchasing.PurchaseReturnDetail rtn_d
		ON rtn_d.Id = rcv_d.TransDetailId
		AND rcv_h.SrcTrans = 2
	LEFT JOIN Purchasing.PurchaseReturnDetail rtnx_d
		ON rtnx_d.Id = rcv_d.TransDetailId
		AND rcv_h.SrcTrans = 2
	LEFT JOIN Inventory.Item i
		ON i.Id = rcv_d.ItemId
	LEFT JOIN Inventory.UoMConversion uom_c_b
		ON uom_c_b.Id = i.UomBuyId
	LEFT JOIN Inventory.UoM uom
		ON uom.Id = rcv_d.UomId
	LEFT JOIN Inventory.UoMConversion uom_c
		ON uom_c.Id = rcv_d.UnitId";
            migrationBuilder.Sql(sql);

            // Reorder column MobileSales.MobileCustomer
            sql = @"BEGIN TRANSACTION
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
ALTER TABLE MobileSales.MobileCustomer
	DROP CONSTRAINT FK_MobileCustomer_CustomerType_TypeId
GO
ALTER TABLE General.CustomerType SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE MobileSales.MobileCustomer
	DROP CONSTRAINT FK_MobileCustomer_Area_AreaId1
GO
ALTER TABLE MobileSales.MobileCustomer
	DROP CONSTRAINT FK_MobileCustomer_Area_AreaId2
GO
ALTER TABLE MobileSales.MobileCustomer
	DROP CONSTRAINT FK_MobileCustomer_Area_AreaId3
GO
ALTER TABLE MobileSales.MobileCustomer
	DROP CONSTRAINT FK_MobileCustomer_Area_AreaId4
GO
ALTER TABLE MobileSales.MobileCustomer
	DROP CONSTRAINT FK_MobileCustomer_Area_AreaId5
GO
ALTER TABLE Sales.Area SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE MobileSales.MobileCustomer
	DROP CONSTRAINT FK_MobileCustomer_Customer_CustCode
GO
ALTER TABLE General.Customer SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE MobileSales.Tmp_MobileCustomer
	(
	Code varchar(17) NOT NULL,
	CustCode varchar(8) NULL,
	Initial varchar(20) NOT NULL,
	Name varchar(50) NOT NULL,
	TypeId int NOT NULL,
	InitialAddress varchar(20) NOT NULL,
	Address1 varchar(100) NOT NULL,
	Address2 varchar(100) NULL,
	ContactPerson varchar(50) NULL,
	Phone varchar(30) NOT NULL,
	Fax varchar(15) NULL,
	AreaId1 int NULL,
	AreaId2 int NULL,
	AreaId3 int NULL,
	AreaId4 int NULL,
	AreaId5 int NULL,
	Lat decimal(9, 6) NULL,
	Lng decimal(9, 6) NULL,
	Mark varchar(3) NOT NULL,
	CreatedBy int NOT NULL,
	CreatedDate datetime NOT NULL,
	UpdatedBy int NOT NULL,
	UpdatedDate datetime NOT NULL,
	ApprovedBy int NULL,
	ApprovedDate datetime NULL,
	RejectedBy int NULL,
	RejectedDate datetime NULL
	)  ON [PRIMARY]
GO
ALTER TABLE MobileSales.Tmp_MobileCustomer SET (LOCK_ESCALATION = TABLE)
GO
IF EXISTS(SELECT * FROM MobileSales.MobileCustomer)
	 EXEC('INSERT INTO MobileSales.Tmp_MobileCustomer (Code, CustCode, Initial, Name, TypeId, InitialAddress, Address1, Address2, ContactPerson, Phone, Fax, AreaId1, AreaId2, AreaId3, AreaId4, AreaId5, Lat, Lng, Mark, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, ApprovedBy, ApprovedDate, RejectedBy, RejectedDate)
		SELECT Code, CustCode, Initial, Name, TypeId, InitialAddress, Address1, Address2, ContactPerson, Phone, Fax, AreaId1, AreaId2, AreaId3, AreaId4, AreaId5, Lat, Lng, Mark, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, ApprovedBy, ApprovedDate, RejectedBy, RejectedDate FROM MobileSales.MobileCustomer WITH (HOLDLOCK TABLOCKX)')
GO
DROP TABLE MobileSales.MobileCustomer
GO
EXECUTE sp_rename N'MobileSales.Tmp_MobileCustomer', N'MobileCustomer', 'OBJECT' 
GO
ALTER TABLE MobileSales.MobileCustomer ADD CONSTRAINT
	PK_MobileCustomer PRIMARY KEY CLUSTERED 
	(
	Code
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX IX_MobileCustomer_AreaId1 ON MobileSales.MobileCustomer
	(
	AreaId1
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_MobileCustomer_AreaId2 ON MobileSales.MobileCustomer
	(
	AreaId2
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_MobileCustomer_AreaId3 ON MobileSales.MobileCustomer
	(
	AreaId3
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_MobileCustomer_AreaId4 ON MobileSales.MobileCustomer
	(
	AreaId4
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_MobileCustomer_AreaId5 ON MobileSales.MobileCustomer
	(
	AreaId5
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_MobileCustomer_CustCode ON MobileSales.MobileCustomer
	(
	CustCode
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_MobileCustomer_TypeId ON MobileSales.MobileCustomer
	(
	TypeId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE MobileSales.MobileCustomer ADD CONSTRAINT
	FK_MobileCustomer_Customer_CustCode FOREIGN KEY
	(
	CustCode
	) REFERENCES General.Customer
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE MobileSales.MobileCustomer ADD CONSTRAINT
	FK_MobileCustomer_Area_AreaId1 FOREIGN KEY
	(
	AreaId1
	) REFERENCES Sales.Area
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE MobileSales.MobileCustomer ADD CONSTRAINT
	FK_MobileCustomer_Area_AreaId2 FOREIGN KEY
	(
	AreaId2
	) REFERENCES Sales.Area
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE MobileSales.MobileCustomer ADD CONSTRAINT
	FK_MobileCustomer_Area_AreaId3 FOREIGN KEY
	(
	AreaId3
	) REFERENCES Sales.Area
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE MobileSales.MobileCustomer ADD CONSTRAINT
	FK_MobileCustomer_Area_AreaId4 FOREIGN KEY
	(
	AreaId4
	) REFERENCES Sales.Area
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE MobileSales.MobileCustomer ADD CONSTRAINT
	FK_MobileCustomer_Area_AreaId5 FOREIGN KEY
	(
	AreaId5
	) REFERENCES Sales.Area
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE MobileSales.MobileCustomer ADD CONSTRAINT
	FK_MobileCustomer_CustomerType_TypeId FOREIGN KEY
	(
	TypeId
	) REFERENCES General.CustomerType
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
COMMIT";
            migrationBuilder.Sql(sql);

			// Refresh view MobileSales.vwMobileCustomer
			sql = "execute sp_refreshview 'MobileSales.vwMobileCustomer'";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Lat",
                schema: "MobileSales",
                table: "MobileCustomer");

            migrationBuilder.DropColumn(
                name: "Lng",
                schema: "MobileSales",
                table: "MobileCustomer");
        }
    }
}
