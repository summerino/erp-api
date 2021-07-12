using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Web.API.Migrations.TenantMigrations
{
    public partial class UpdateTablePurchaseReturnAndOthers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RcvQty",
                schema: "Purchasing",
                table: "PurchaseReturnDetail",
                newName: "QtyRcv");

            migrationBuilder.AddColumn<bool>(
                name: "IncludeTax",
                schema: "Purchasing",
                table: "PurchaseReturnHeader",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // Create view SystemManagement.vwUser
            var sql = @"CREATE VIEW [SystemManagement].[vwUser]
AS
    SELECT a.*,
        r.[Name] As RoleName,
        e.Username As EmployeeUsername,
        u.Initial AS UpdatedInitial
    FROM SystemManagement.[User] a
    LEFT JOIN SystemManagement.[User] u
        ON u.Id = a.UpdatedBy
    LEFT JOIN General.Employee e
        ON u.EmployeeId = e.Id
    LEFT JOIN SystemManagement.[Role] r
        ON u.RoleId = r.Id";
            migrationBuilder.Sql(sql);

            // Alter view Purchasing.vwPurchaseReceiveDetail
            sql = @"ALTER VIEW [Purchasing].[vwPurchaseReceiveDetail]
AS
	SELECT pr_d.*,
		ISNULL(po_d.Qty, 0) AS OrderQty,
		ISNULL(po_d.Qty, 0) - ISNULL(po_d.QtyRcv, 0) AS OutstandingQty,
		i.Initial AS ItemInitial,
		i.[Name] AS ItemName,
		i.UomBuyId AS ItemUomBuyId,
		uom_c_b.UnitEquivalent AS ItemUomBuyName,
		i.BuyPrice AS ItemBuyPrice,
		uom.Initial AS UomInitial,
		uom_c.UnitEquivalent AS UnitName
	FROM Purchasing.PurchaseReceiveDetail  pr_d
	LEFT JOIN Purchasing.PurchaseOrderDetail po_d
		ON po_d.Id = pr_d.PODetailId
	LEFT JOIN Inventory.Item i
		ON i.Id = pr_d.ItemId
	LEFT JOIN Inventory.UoMConversion uom_c_b
		ON uom_c_b.Id = i.UomBuyId
	LEFT JOIN Inventory.UoM uom
		ON uom.Id = pr_d.UomId
	LEFT JOIN Inventory.UoMConversion uom_c
		ON uom_c.Id = pr_d.UnitId";
            migrationBuilder.Sql(sql);

            // Alter store procedure dbo.vwPurchaseReceiveDetail
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

			// Alter store procedure dbo.sp_update_stock_mutation_from_do
			sql = @"ALTER PROCEDURE [dbo].[sp_update_stock_mutation_from_do]
	@code varchar(17),
	@date date,
	@soCode varchar(17)
AS
BEGIN TRY

	DECLARE @warehouseCode varchar(8)

	SELECT @warehouseCode = WarehouseCode
	FROM Sales.SalesDeliveryHeader
	WHERE Code = @code

	SELECT *
	INTO #tmp_do
	FROM Sales.SalesDeliveryDetail
	WHERE Code = @code

	-- Delete stock mutation that doesn't have in sales delivery item detail
	DELETE Inventory.StockMutation
	WHERE RefCode1 = @code
	AND Src = 'DO'
	AND NOT EXISTS (
		SELECT *
		FROM #tmp_do
		WHERE Code = RefCode1
		AND Id = RefDetailId1
	)

	-- Update stock mutation that exists in sales delivery item detail
	UPDATE sm
	SET sm.WarehouseCode = @warehouseCode,
		sm.[Date] = @date,
		sm.ItemId = #tmp_do.ItemId,
		sm.UomId = #tmp_do.UomId,
		sm.UnitId = #tmp_do.UnitId,
		sm.Qty = #tmp_do.Qty,
		sm.RefCode2 = @soCode
	FROM Inventory.StockMutation sm, #tmp_do
	WHERE sm.RefCode1 = #tmp_do.Code
	AND sm.RefDetailId1 = #tmp_do.Id
	AND sm.Src = 'DO'

	-- Insert stock mutation that doesn't have with sales delivery item detail
	INSERT INTO Inventory.StockMutation
		SELECT @warehouseCode, @date, ItemId, UomId, UnitId, Qty, Code, Id, @soCode, 'DO', 0, 0
		FROM #tmp_do do
		WHERE NOT EXISTS (
			SELECT Id
			FROM Inventory.StockMutation sm
			WHERE sm.RefCode1 = do.Code
			AND sm.RefDetailId1 = do.Id
			AND sm.Src = 'DO'
		)

	-- Drop temp tables
	DROP TABLE #tmp_do
	
	SELECT *
	INTO #tmp_stk
	FROM Inventory.StockMutation WHERE RefCode1 = @code
	
	DECLARE @Id int
	DECLARE @UomId int
	DECLARE @UnitId int
	DECLARE @IsBaseUnit int
	DECLARE @Seq int

	WHILE EXISTS(SELECT * FROM #tmp_stk)
	BEGIN
		SELECT TOP 1 @Id = Id, @UomId = UomId, @UnitId = UnitId FROM #tmp_stk
		SELECT @IsBaseUnit = IsBaseUnit, @Seq = Seq FROM Inventory.UoMConversion WHERE UomId = @UomId AND Id = @UnitId

		IF @IsBaseUnit = 1 
		BEGIN
			UPDATE Inventory.StockMutation SET BaseQty = Qty,BaseUnit = UnitId WHERE Id = @Id
		END
		ELSE
		BEGIN
			UPDATE Inventory.StockMutation SET BaseQty = Qty * (SELECT EXP(SUM(LOG(Conversion))) FROM Inventory.UoMConversion WHERE UomId = @UomId AND Seq <= @Seq),
			BaseUnit = (SELECT Id FROM Inventory.UoMConversion WHERE UomId = @UomId AND IsBaseUnit = 1) WHERE Id = @Id
		END

		DELETE #tmp_stk WHERE Id = @Id
	END

	DROP TABLE #tmp_stk
END TRY
BEGIN CATCH
	-- Drop temp tables
	IF OBJECT_ID('tempdb.dbo.#tmp_do') IS NOT NULL
		DROP TABLE #tmp_do
	IF OBJECT_ID('tempdb.dbo.#tmp_stk') IS NOT NULL
		DROP TABLE #tmp_stk

	-- Raise error
	EXEC dbo.sp_raiseerror
END CATCH";
			migrationBuilder.Sql(sql);

			// Alter store procedure dbo.sp_update_stock_mutation_from_rcv
			sql = @"ALTER PROCEDURE [dbo].[sp_update_stock_mutation_from_rcv]
	@code varchar(17),
	@date date,
	@poCode varchar(17)
AS
BEGIN TRY

	SELECT *
	INTO #tmp_pr
	FROM Purchasing.PurchaseReceiveDetail
	WHERE Code = @code

	-- Delete stock mutation that doesn't have in purchase receive item detail
	DELETE Inventory.StockMutation
	WHERE RefCode1 = @code
	AND Src = 'RCV'
	AND NOT EXISTS (
		SELECT *
		FROM #tmp_pr
		WHERE Code = RefCode1
		AND Id = RefDetailId1
	)

	-- Update stock mutation that exists in purchase receive item detail
	UPDATE sm
	SET sm.WarehouseCode = #tmp_pr.WarehouseCode,
		sm.[Date] = @date,
		sm.ItemId = #tmp_pr.ItemId,
		sm.UomId = #tmp_pr.UomId,
		sm.UnitId = #tmp_pr.UnitId,
		sm.Qty = #tmp_pr.Qty,
		sm.RefCode2 = CASE WHEN [Type] = 0 THEN @poCode ELSE NULL END
	FROM Inventory.StockMutation sm, #tmp_pr
	WHERE sm.RefCode1 = #tmp_pr.Code
	AND sm.RefDetailId1 = #tmp_pr.Id
	AND sm.Src = 'RCV'

	-- Insert stock mutation that doesn't have with purchase receive item detail
	INSERT INTO Inventory.StockMutation
		SELECT WarehouseCode, @date, ItemId, UomId, UnitId, Qty, Code, Id,
			CASE WHEN [Type] = 0 THEN @poCode ELSE NULL END,
			'RCV', 0, 0
		FROM #tmp_pr pr
		WHERE NOT EXISTS (
			SELECT Id
			FROM Inventory.StockMutation sm
			WHERE sm.RefCode1 = pr.Code
			AND sm.RefDetailId1 = pr.Id
			AND sm.Src = 'RCV'
		)

	-- Drop temp tables
	DROP TABLE #tmp_pr
	
	SELECT *
	INTO #tmp_stk
	FROM Inventory.StockMutation WHERE RefCode1 = @code
	
	DECLARE @Id int
	DECLARE @UomId int
	DECLARE @UnitId int
	DECLARE @IsBaseUnit int
	DECLARE @Seq int

	WHILE EXISTS(SELECT * FROM #tmp_stk)
	BEGIN
		SELECT TOP 1 @Id = Id, @UomId = UomId, @UnitId = UnitId FROM #tmp_stk
		SELECT @IsBaseUnit = IsBaseUnit, @Seq = Seq FROM Inventory.UoMConversion WHERE UomId = @UomId AND Id = @UnitId

		IF @IsBaseUnit = 1 
		BEGIN
			UPDATE Inventory.StockMutation SET BaseQty = Qty,BaseUnit = UnitId WHERE Id = @Id
		END
		ELSE
		BEGIN
			UPDATE Inventory.StockMutation SET BaseQty = Qty * (SELECT EXP(SUM(LOG(Conversion))) FROM Inventory.UoMConversion WHERE UomId = @UomId AND Seq <= @Seq),
			BaseUnit = (SELECT Id FROM Inventory.UoMConversion WHERE UomId = @UomId AND IsBaseUnit = 1) WHERE Id = @Id
		END

		DELETE #tmp_stk WHERE Id = @Id
	END

	DROP TABLE #tmp_stk
END TRY
BEGIN CATCH
	-- Drop temp tables
	IF OBJECT_ID('tempdb.dbo.#tmp_pr') IS NOT NULL
		DROP TABLE #tmp_pr
	IF OBJECT_ID('tempdb.dbo.#tmp_stk') IS NOT NULL
		DROP TABLE #tmp_stk

	-- Raise error
	EXEC dbo.sp_raiseerror
END CATCH";
            migrationBuilder.Sql(sql);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IncludeTax",
                schema: "Purchasing",
                table: "PurchaseReturnHeader");

            migrationBuilder.RenameColumn(
                name: "QtyRcv",
                schema: "Purchasing",
                table: "PurchaseReturnDetail",
                newName: "RcvQty");

            // Drop view SystemManagement.vwUser
            var sql = @"DROP VIEW [SystemManagement].[vwUser]";
            migrationBuilder.Sql(sql);
        }
    }
}
