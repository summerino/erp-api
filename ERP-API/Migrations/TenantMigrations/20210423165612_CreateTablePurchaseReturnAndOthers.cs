using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Web.API.Migrations.TenantMigrations
{
    public partial class CreateTablePurchaseReturnAndOthers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_msSystemParameters",
                schema: "dbo",
                table: "msSystemParameters");

            migrationBuilder.DropPrimaryKey(
                name: "PK_msSequenceNumbers",
                schema: "dbo",
                table: "msSequenceNumbers");

            migrationBuilder.RenameTable(
                name: "msSystemParameters",
                schema: "dbo",
                newName: "SystemParameter",
                newSchema: "SystemManagement");

            migrationBuilder.RenameTable(
                name: "msSequenceNumbers",
                schema: "dbo",
                newName: "SequenceNumber",
                newSchema: "SystemManagement");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SystemParameter",
                schema: "SystemManagement",
                table: "SystemParameter",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SequenceNumber",
                schema: "SystemManagement",
                table: "SequenceNumber",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "ItemGroup",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Initial = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemGroup", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ItemGroupSubGroup",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemGroupId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Value = table.Column<string>(type: "varchar(1000)", unicode: false, maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemGroupSubGroup", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseReturnDetail",
                schema: "Purchasing",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    RcvDetailId = table.Column<long>(type: "bigint", nullable: true),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RcvQty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WarehouseCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    WarehouseCodeIn = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: true),
                    Length = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Width = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Height = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Weight = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
                    DimensionMeasurement = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    WeightMeasurement = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    UnitPrice = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    Disc = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    TaxId = table.Column<int>(type: "int", nullable: true),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NettPrice = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    DPP = table.Column<decimal>(type: "decimal(19,6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseReturnDetail", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseReturnHeader",
                schema: "Purchasing",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    RcvCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    RefNo = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    Type = table.Column<short>(type: "smallint", nullable: false),
                    SupCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    ShippedBy = table.Column<long>(type: "bigint", nullable: false),
                    ApproveBy = table.Column<long>(type: "bigint", nullable: true),
                    CurrCode = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ShipmentFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    HandlingFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FinalDisc = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DPP = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    Mark = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseReturnHeader", x => x.Code);
                });

            // Create view General.vwVehicle
            var sql = @"CREATE VIEW [General].[vwVehicle]
AS
    SELECT v.*,
        t.[Name] AS TypeName,
		u.Initial AS UpdatedInitial
    FROM General.Vehicle v
    LEFT JOIN General.VehicleType t
        ON v.TypeId = t.Id
	LEFT JOIN SystemManagement.[User] u
		ON u.Id = v.UpdatedBy";
            migrationBuilder.Sql(sql);

            // Create store procedure dbo.sp_update_currency_sort
            sql = @"CREATE PROCEDURE [dbo].[sp_update_currency_sort]
	@beforeAfter int
AS
BEGIN

	DECLARE @orderBy varchar(5)
	DECLARE @query varchar(max) = ''

	--1 = after, 2 = before
	IF (@beforeAfter = 1)
	BEGIN
		SELECT @orderBy = 'asc'
	END
	ELSE
	BEGIN 
		SELECT @orderBy = 'desc'
	END

	EXECUTE('UPDATE c SET c.Sort = a.Num FROM (SELECT ROW_NUMBER() OVER(ORDER BY Sort ASC, UpdatedDate '+@orderBy+') AS Num, Code FROM General.Currency) a INNER JOIN General.Currency c ON a.Code = c.Code WHERE a.Num <> c.Sort')

END";
            migrationBuilder.Sql(sql);

            // Alter view General.vwCustomer
            sql = @"ALTER VIEW [General].[vwCustomer]
AS
	SELECT c.*,
		t.[Name] AS TypeName,
		u.Initial AS UpdatedInitial
	FROM General.Customer c
	LEFT JOIN General.CustomerType t
		ON t.Id = c.TypeId
	LEFT JOIN SystemManagement.[User] u
		ON u.Id = c.UpdatedBy";
            migrationBuilder.Sql(sql);

            // Alter view General.vwSupplier
            sql = @"ALTER VIEW [General].[vwSupplier]
AS
	SELECT c.*,
		t.[Name] AS TypeName,
		u.Initial AS UpdatedInitial
	FROM General.Supplier c
	LEFT JOIN General.SupplierType t
		ON t.Id = c.TypeId
	LEFT JOIN SystemManagement.[User] u
		ON u.Id = c.UpdatedBy";
            migrationBuilder.Sql(sql);

            // Alter view General.vwTax
            sql = @"ALTER VIEW [General].[vwTax]
AS
    SELECT c.*,
        x.[Name] AS CoaName,
		u.Initial AS UpdatedInitial
    FROM General.Tax c
    LEFT JOIN Accounting.COA x
        ON x.Code = c.CoaCode
	LEFT JOIN SystemManagement.[User] u
		ON u.Id = c.UpdatedBy";
            migrationBuilder.Sql(sql);

            // Alter view Inventory.vwItem
            sql = @"ALTER VIEW [Inventory].[vwItem]
AS
	SELECT i.*,
		CASE i.TypeId
			WHEN 0 THEN 'Raw Material'
			WHEN 1 THEN 'Work In Process'
			WHEN 2 THEN 'Finished Good'
			WHEN 3 THEN 'Service'
			WHEN 4 THEN 'Consignment'
			WHEN 5 THEN 'Kits'
			WHEN 6 THEN 'Resell'
			ELSE '' END AS TypeName,
		c.[Name] AS CategoryName,
		uom.Initial AS UomInitial,
		uom_c_s.UnitEquivalent AS UomSellName,
		uom_c_b.UnitEquivalent AS UomBuyName,
		u.Initial AS UpdatedInitial
	FROM Inventory.Item i
	LEFT JOIN Inventory.ItemCategory c
		ON c.Id = i.CategoryId
	LEFT JOIN Inventory.UoM uom
		ON uom.Id = i.UomId
	LEFT JOIN Inventory.UoMConversion uom_c_s
		ON uom_c_s.Id = i.UomSellId
	LEFT JOIN Inventory.UoMConversion uom_c_b
		ON uom_c_b.Id = i.UomBuyId
	LEFT JOIN SystemManagement.[User] u
		ON u.Id = c.UpdatedBy";
            migrationBuilder.Sql(sql);

            // Alter view Purchasing.vwPurchaseInvoiceHeader
            sql = @"ALTER VIEW [Purchasing].[vwPurchaseInvoiceHeader]
AS
	SELECT pi_h.*,
		s.[Name] AS SupName,
		e.Initial AS IssuedInitial,
		u.Initial AS UpdatedInitial,
		CASE pi_h.Mark
			WHEN 'A' THEN 'Active'
			WHEN 'V' THEN 'Void' END AS [Status]
	FROM Purchasing.PurchaseInvoiceHeader pi_h
	LEFT JOIN General.Supplier s
		ON s.Code = pi_h.SupCode
	LEFT JOIN General.Employee e
		ON e.Id = pi_h.IssuedBy
	LEFT JOIN SystemManagement.[User] u
		ON u.Id = pi_h.UpdatedBy";
            migrationBuilder.Sql(sql);

            // Alter view Purchasing.vwPurchaseOrderHeader
            sql = @"ALTER VIEW [Purchasing].[vwPurchaseOrderHeader]
AS
	SELECT po_h.*,
		s.[Name] AS SupName,
		e.Initial AS RequestInitial,
		u.Initial AS UpdatedInitial,
		CASE po_h.Mark
			WHEN 'A' THEN 'Active'
			WHEN 'V' THEN 'Void'
			WHEN 'PR' THEN 'Partial Received'
			WHEN 'CMP' THEN 'Completed'
			WHEN 'CLS' THEN 'Closed' END AS [Status]
	FROM Purchasing.PurchaseOrderHeader po_h
	LEFT JOIN General.Supplier s
		ON s.Code = po_h.SupCode
	LEFT JOIN General.Employee e
		ON e.Id = po_h.RequestBy
	LEFT JOIN SystemManagement.[User] u
		ON u.Id = po_h.UpdatedBy";
            migrationBuilder.Sql(sql);

            // Alter view Purchasing.vwPurchaseReceiveHeader
            sql = @"ALTER VIEW [Purchasing].[vwPurchaseReceiveHeader]
AS
	SELECT pr_h.*,
		s.[Name] AS SupName,
		e.Initial AS ReceiveInitial,
		u.Initial AS UpdatedInitial,
		CASE pr_h.Mark
			WHEN 'A' THEN 'Active'
			WHEN 'V' THEN 'Void'
			WHEN 'INV' THEN 'Invoiced' END AS [Status]
	FROM Purchasing.PurchaseReceiveHeader pr_h
	LEFT JOIN General.Supplier s
		ON s.Code = pr_h.SupCode
	LEFT JOIN General.Employee e
		ON e.Id = pr_h.ReceiveBy
	LEFT JOIN SystemManagement.[User] u
		ON u.Id = pr_h.UpdatedBy";
            migrationBuilder.Sql(sql);

            // Alter view Sales.vwSalesDeliveryHeader
            sql = @"ALTER VIEW [Sales].[vwSalesDeliveryHeader]
AS
	SELECT do_h.*,
		c.[Name] AS CustName,
		e.Initial AS ShippedInitial,
		u.Initial AS UpdatedInitial,
		CASE do_h.Mark
			WHEN 'A' THEN 'Active'
			WHEN 'V' THEN 'Void'
			WHEN 'INV' THEN 'Invoiced' END AS [Status]
	FROM Sales.SalesDeliveryHeader do_h
	LEFT JOIN General.Customer c
		ON c.Code = do_h.CustCode
	LEFT JOIN General.Employee e
		ON e.Id = do_h.ShippedBy
	LEFT JOIN SystemManagement.[User] u
		ON u.Id = do_h.UpdatedBy";
            migrationBuilder.Sql(sql);

            // Alter view Sales.vwSalesInvoiceHeader
            sql = @"ALTER VIEW [Sales].[vwSalesInvoiceHeader]
AS
	SELECT si_h.*,
		c.[Name] AS CustName,
		e.Initial AS IssuedInitial,
		u.Initial AS UpdatedInitial,
		CASE si_h.Mark
			WHEN 'A' THEN 'Active'
			WHEN 'V' THEN 'Void' END AS [Status]
	FROM Sales.SalesInvoiceHeader si_h
	LEFT JOIN General.Customer c
		ON c.Code = si_h.CustCode
	LEFT JOIN General.Employee e
		ON e.Id = si_h.IssuedBy
	LEFT JOIN SystemManagement.[User] u
		ON u.Id = si_h.UpdatedBy";
            migrationBuilder.Sql(sql);

            // Alter view Sales.vwSalesOrderHeader
            sql = @"ALTER VIEW [Sales].[vwSalesOrderHeader]
AS
	SELECT so_h.*,
		c.[Name] AS CustName,
		e.Initial AS SalesInitial,
		u.Initial AS UpdatedInitial,
		CASE so_h.Mark
			WHEN 'A' THEN 'Active'
			WHEN 'V' THEN 'Void'
			WHEN 'PS' THEN 'Partial Shipped'
			WHEN 'CMP' THEN 'Completed'
			WHEN 'CLS' THEN 'Closed' END AS [Status]
	FROM Sales.SalesOrderHeader so_h
	LEFT JOIN General.Customer c
		ON c.Code = so_h.CustCode
	LEFT JOIN General.Employee e
		ON e.Id = so_h.SalesBy
	LEFT JOIN SystemManagement.[User] u
		ON u.Id = so_h.UpdatedBy";
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

			SET @digitFormat = SUBSTRING(@format, PATINDEX('%{[0-9]}%', @format), 5)
			SET @digit = CONVERT(int, REPLACE(REPLACE(@digitFormat, '{', ''), '}', ''))
	
			/* Get sequence number */
			SELECT @lastRunNo = LastRunNo
			FROM msSequenceNumbers
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

			/* Update history in msSequenceNumbers */
			UPDATE msSequenceNumbers
			SET LastRunNo = @lastRunNo
			WHERE Code = @code
			AND [Format] = @format

			/* Insert history in msSequenceNumbers if no record affected */
			IF @@ROWCOUNT = 0
			BEGIN
				INSERT INTO msSequenceNumbers (Code, [Format], LastRunNo)
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SystemParameter",
                schema: "SystemManagement",
                table: "SystemParameter");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SequenceNumber",
                schema: "SystemManagement",
                table: "SequenceNumber");

            migrationBuilder.RenameTable(
                name: "SystemParameter",
                schema: "SystemManagement",
                newName: "msSystemParameters",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "SequenceNumber",
                schema: "SystemManagement",
                newName: "msSequenceNumbers",
                newSchema: "dbo");

            migrationBuilder.AddPrimaryKey(
                name: "PK_msSystemParameters",
                schema: "dbo",
                table: "msSystemParameters",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_msSequenceNumbers",
                schema: "dbo",
                table: "msSequenceNumbers",
                column: "Id");

            migrationBuilder.DropTable(
                name: "ItemGroup",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "ItemGroupSubGroup",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "PurchaseReturnDetail",
                schema: "Purchasing");

            migrationBuilder.DropTable(
                name: "PurchaseReturnHeader",
                schema: "Purchasing");

            migrationBuilder.DropTable(
                name: "SequenceNumber",
                schema: "SystemManagement");

            migrationBuilder.DropTable(
                name: "SystemParameter",
                schema: "SystemManagement");

            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "msSequenceNumbers",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Format = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    LastRunNo = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_msSequenceNumbers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "msSystemParameters",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Category = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Code = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Module = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Seq = table.Column<int>(type: "int", nullable: true),
                    Value = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_msSystemParameters", x => x.Id);
                });

            // Drop view General.vwVehicle
            var sql = @"DROP VIEW [General].[vwVehicle]";
            migrationBuilder.Sql(sql);

            // Drop store procedure dbo.sp_update_currency_sort
            sql = @"DROP PROCEDURE [dbo].[sp_update_currency_sort]";
            migrationBuilder.Sql(sql);
        }
    }
}
