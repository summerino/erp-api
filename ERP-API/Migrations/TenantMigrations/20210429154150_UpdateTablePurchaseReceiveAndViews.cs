using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Web.API.Migrations.TenantMigrations
{
    public partial class UpdateTablePurchaseReceiveAndViews : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "POCode",
                schema: "Purchasing",
                table: "PurchaseReceiveHeader",
                newName: "TransCode");

            migrationBuilder.RenameColumn(
                name: "PODetailId",
                schema: "Purchasing",
                table: "PurchaseReceiveDetail",
                newName: "TransDetailId");

            migrationBuilder.AlterColumn<string>(
                name: "RcvCode",
                schema: "Purchasing",
                table: "PurchaseReturnHeader",
                type: "varchar(17)",
                unicode: false,
                maxLength: 17,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(17)",
                oldUnicode: false,
                oldMaxLength: 17);

            migrationBuilder.AddColumn<short>(
                name: "SrcTrans",
                schema: "Purchasing",
                table: "PurchaseReceiveHeader",
                type: "smallint",
                nullable: false,
                defaultValue: (short)1);

            // Create view SystemManagement.vwUser
            var sql = @"CREATE VIEW [Purchasing].[vwPurchaseReturnHeader]
AS
	SELECT pr_h.*,
		s.[Name] AS SupName,
		e.Initial AS ShippedInitial,
		u_c.Initial AS CreatedInitial,
		u_u.Initial AS UpdatedInitial,
		CASE pr_h.Mark
			WHEN 'A' THEN 'Active'
			WHEN 'V' THEN 'Void'
			WHEN 'PR' THEN 'Partial Received'
			WHEN 'CMP' THEN 'Completed'
			WHEN 'CLS' THEN 'Closed' END AS [Status]
	FROM Purchasing.PurchaseReturnHeader pr_h
	LEFT JOIN General.Supplier s
		ON s.Code = pr_h.SupCode
	LEFT JOIN General.Employee e
		ON e.Id = pr_h.ShippedBy
	LEFT JOIN SystemManagement.[User] u_c
		ON u_c.Id = pr_h.CreatedBy
	LEFT JOIN SystemManagement.[User] u_u
		ON u_u.Id = pr_h.UpdatedBy";
            migrationBuilder.Sql(sql);

            // Create view Purchasing.vwPurchaseReturnDetail
            sql = @"CREATE VIEW [Purchasing].[vwPurchaseReturnDetail]
AS
	SELECT pr_d.*,
		i.[Name] AS ItemName,
		i.UomBuyId AS ItemUomBuyId,
		uom_c_b.UnitEquivalent AS ItemUomBuyName,
		i.BuyPrice AS ItemBuyPrice,
		uom.Initial AS UomInitial,
		uom_c.UnitEquivalent AS UnitName
	FROM Purchasing.PurchaseReturnDetail pr_d
	LEFT JOIN Inventory.Item i
		ON i.Id = pr_d.ItemId
	LEFT JOIN Inventory.UoMConversion uom_c_b
		ON uom_c_b.Id = i.UomBuyId
	LEFT JOIN Inventory.UoM uom
		ON uom.Id = pr_d.UomId
	LEFT JOIN Inventory.UoMConversion uom_c
		ON uom_c.Id = pr_d.UnitId";
            migrationBuilder.Sql(sql);

            // Alter view Purchasing.vwPurchaseReceiveHeader
            sql = @"ALTER VIEW [Purchasing].[vwPurchaseReceiveHeader]
AS
	SELECT pr_h.*,
		s.[Name] AS SupName,
		e.Initial AS ReceiveInitial,
		u_c.Initial AS CreatedInitial,
		u_u.Initial AS UpdatedInitial,
		CASE pr_h.Mark
			WHEN 'A' THEN 'Active'
			WHEN 'V' THEN 'Void'
			WHEN 'INV' THEN 'Invoiced' END AS [Status]
	FROM Purchasing.PurchaseReceiveHeader pr_h
	LEFT JOIN General.Supplier s
		ON s.Code = pr_h.SupCode
	LEFT JOIN General.Employee e
		ON e.Id = pr_h.ReceiveBy
	LEFT JOIN SystemManagement.[User] u_c
		ON u_c.Id = pr_h.CreatedBy
	LEFT JOIN SystemManagement.[User] u_u
		ON u_u.Id = pr_h.UpdatedBy";
            migrationBuilder.Sql(sql);

            // Alter view Purchasing.vwPurchaseReceiveDetail
            sql = @"ALTER VIEW [Purchasing].[vwPurchaseReceiveDetail]
AS
	SELECT rcv_d.*,
		CASE WHEN rcv_h.SrcTrans = 1 THEN ISNULL(po_d.Qty, 0)
			 ELSE ISNULL(rtn_d.Qty, 0) END AS OrderQty,
		CASE WHEN rcv_h.SrcTrans = 1 THEN ISNULL(po_d.Qty, 0) - ISNULL(po_d.QtyRcv, 0)
			ELSE ISNULL(rtn_d.Qty, 0) - ISNULL(rtn_d.QtyRcv, 0) END AS OutstandingQty,
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
	LEFT JOIN Purchasing.PurchaseOrderDetail po_d
		ON po_d.Id = rcv_d.TransDetailId
		AND rcv_h.SrcTrans = 1
	LEFT JOIN Purchasing.PurchaseReturnDetail rtn_d
		ON rtn_d.Id = rcv_d.TransDetailId
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

			// Alter store procedure dbo.sp_generate_autono
			sql = @"ALTER PROCEDURE [dbo].[sp_generate_autono]
	@code varchar(20),
	@date date = null,
	@newAutoNo varchar(20) = null OUTPUT
AS
BEGIN TRANSACTION

	DECLARE @result varchar(30) = ''

	IF ISNULL(@code, '') != ''
	BEGIN
		
		DECLARE @format varchar(32), @digitFormat varchar(32), @digit int,
				@myNumber varchar(20),
				@lastRunNo int
				
		/* Get format */
		SELECT @format = [Value]
		FROM SystemManagement.SystemParameter
		WHERE Code = @code
		
		IF @format IS NOT NULL
		BEGIN
		
			IF @date IS NULL
				SET @date = GETDATE()

			/* Replace format for date */
			SET @format = REPLACE(@format, '{Y}', FORMAT(@date, 'yy'))
			SET @format = REPLACE(@format, '{M}', FORMAT(@date, 'MM'))
			SET @format = REPLACE(@format, '{D}', FORMAT(@date, 'dd'))

			SET @digitFormat = SUBSTRING(@format, PATINDEX('%{[0-9]}%', @format), 5)
			SET @digit = CONVERT(int, REPLACE(REPLACE(@digitFormat, '{', ''), '}', ''))
	
			/* Get sequence number */
			SELECT @lastRunNo = LastRunNo
			FROM SystemManagement.SequenceNumber
			WHERE Code = @code
			AND [Format] = @format

			IF @lastRunNo IS NULL
			BEGIN
				SET @lastRunNo = 0
			END

			SET @lastRunNo = @lastRunNo + 1
			
			SET @myNumber = '00000000000' + CONVERT(varchar, @lastRunNo)
			
			/* Replace format for sequence number */
			SET @result = REPLACE(@format, @digitFormat, RIGHT(@myNumber, @digit))

			/* Update history in SystemManagement.SequenceNumber */
			UPDATE SystemManagement.SequenceNumber
			SET LastRunNo = @lastRunNo
			WHERE Code = @code
			AND [Format] = @format

			/* Insert history in SystemManagement.SequenceNumber if no record affected */
			IF @@ROWCOUNT = 0
			BEGIN
				INSERT INTO SystemManagement.SequenceNumber (Code, [Format], LastRunNo)
				VALUES (@code, @format, @lastRunNo)
			END
		END

		/* Set output parameter */
		SET @newAutoNo = @result

	END
	
	/* return */
	SELECT @result AS value
	
COMMIT TRANSACTION";
            migrationBuilder.Sql(sql);

			// Alter store procedure dbo.sp_update_po_rcv_qty
			sql = @"ALTER PROCEDURE [dbo].[sp_update_po_rcv_qty]
	@code varchar(17)
AS
BEGIN TRY

	DECLARE @orderQty decimal(18, 2), @rcvQty decimal(18, 2)

	-- Get purchase order data
	SELECT Code, ItemId, UnitId, Qty
	INTO #tmp_po
	FROM Purchasing.PurchaseOrderDetail po_d
	WHERE EXISTS (
		SELECT Code
		FROM Purchasing.PurchaseOrderHeader po_h
		WHERE Code = @code
		AND Mark NOT IN ('V', 'CLS')
		AND po_h.Code = po_d.Code
	)
	AND [Type] = 0
	
	-- Return if purchase order not exists
	IF NOT EXISTS (SELECT Code FROM #tmp_po)
	BEGIN
		DROP TABLE #tmp_po
		RETURN
	END
	
	-- Get purchase receive data
	SELECT ItemId, UnitId, SUM(Qty) AS QtyRcv
	INTO #tmp_pr
	FROM Purchasing.PurchaseReceiveDetail pr_d
	WHERE EXISTS (
		SELECT Code
		FROM Purchasing.PurchaseReceiveHeader pr_h
		WHERE TransCode = @code
		AND Mark <> 'V'
		AND pr_h.Code = pr_d.Code
	)
	AND [Type] = 0
	GROUP BY ItemId, UnitId

	-- Join all
	SELECT po.Code, po.ItemId, po.UnitId, po.Qty,
		ISNULL(pr.QtyRcv, 0) AS QtyRcv
	INTO #tmp_all
	FROM #tmp_po po
	LEFT JOIN #tmp_pr pr
		ON pr.ItemId = po.ItemId
		AND pr.UnitId = po.UnitId

	-- Drop temp tables
	DROP TABLE #tmp_po
	DROP TABLE #tmp_pr

	-- Calculate sum
	SELECT @orderQty = SUM(Qty),
		@rcvQty = SUM(QtyRcv)
	FROM #tmp_all

	-- Update PO header mark
	UPDATE Purchasing.PurchaseOrderHeader
	SET Mark = (
		CASE WHEN @rcvQty = 0 THEN 'A'
			WHEN @rcvQty >= @orderQty THEN 'CMP'
			ELSE 'PR' END
	)
	WHERE Code = @code
	AND Mark NOT IN ('V', 'CLS')

	-- Update PO detail receive qty item
	UPDATE po_d
	SET po_d.QtyRcv = #tmp_all.QtyRcv
	FROM Purchasing.PurchaseOrderDetail po_d, #tmp_all
	WHERE po_d.Code = #tmp_all.Code
	AND po_d.ItemId = #tmp_all.ItemId
	AND po_d.UnitId = #tmp_all.UnitId

	-- Drop temp table
	DROP TABLE #tmp_all

END TRY
BEGIN CATCH
	-- Drop temp tables
	IF OBJECT_ID('tempdb.dbo.#tmp_po') IS NOT NULL
		DROP TABLE #tmp_po
	IF OBJECT_ID('tempdb.dbo.#tmp_pr') IS NOT NULL
		DROP TABLE #tmp_pr
	IF OBJECT_ID('tempdb.dbo.#tmp_all') IS NOT NULL
		DROP TABLE #tmp_all

	-- Raise error
	EXEC dbo.sp_raiseerror
END CATCH";
            migrationBuilder.Sql(sql);

            // Reordering column in table Inventory.StockMutation
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
CREATE TABLE Inventory.Tmp_StockMutation
	(
	Id bigint NOT NULL IDENTITY (1, 1),
	WarehouseCode varchar(8) NOT NULL,
	Date date NOT NULL,
	ItemId int NOT NULL,
	UomId int NOT NULL,
	UnitId int NOT NULL,
	Qty decimal(18, 2) NOT NULL,
	BaseUnit int NOT NULL,
	BaseQty decimal(18, 2) NOT NULL,
	RefCode1 varchar(17) NOT NULL,
	RefDetailId1 bigint NOT NULL,
	RefCode2 varchar(17) NULL,
	Src varchar(5) NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Inventory.Tmp_StockMutation SET (LOCK_ESCALATION = TABLE)
GO
SET IDENTITY_INSERT Inventory.Tmp_StockMutation ON
GO
IF EXISTS(SELECT * FROM Inventory.StockMutation)
	 EXEC('INSERT INTO Inventory.Tmp_StockMutation (Id, WarehouseCode, Date, ItemId, UomId, UnitId, Qty, BaseUnit, BaseQty, RefCode1, RefDetailId1, RefCode2, Src)
		SELECT Id, WarehouseCode, Date, ItemId, UomId, UnitId, Qty, BaseUnit, BaseQty, RefCode1, RefDetailId1, RefCode2, Src FROM Inventory.StockMutation WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT Inventory.Tmp_StockMutation OFF
GO
DROP TABLE Inventory.StockMutation
GO
EXECUTE sp_rename N'Inventory.Tmp_StockMutation', N'StockMutation', 'OBJECT' 
GO
ALTER TABLE Inventory.StockMutation ADD CONSTRAINT
	PK_StockMutation PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
COMMIT";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SrcTrans",
                schema: "Purchasing",
                table: "PurchaseReceiveHeader");

            migrationBuilder.RenameColumn(
                name: "TransCode",
                schema: "Purchasing",
                table: "PurchaseReceiveHeader",
                newName: "POCode");

            migrationBuilder.RenameColumn(
                name: "TransDetailId",
                schema: "Purchasing",
                table: "PurchaseReceiveDetail",
                newName: "PODetailId");

            migrationBuilder.AlterColumn<string>(
                name: "RcvCode",
                schema: "Purchasing",
                table: "PurchaseReturnHeader",
                type: "varchar(17)",
                unicode: false,
                maxLength: 17,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(17)",
                oldUnicode: false,
                oldMaxLength: 17,
                oldNullable: true);

            // Drop view Purchasing.vwPurchaseReturnHeader
            var sql = @"CREATE VIEW [Purchasing].[vwPurchaseReturnHeader]";
            migrationBuilder.Sql(sql);

            // Drop view Purchasing.vwPurchaseReturnDetail
            sql = @"CREATE VIEW [Purchasing].[vwPurchaseReturnDetail]";
            migrationBuilder.Sql(sql);
        }
    }
}
