using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Web.API.Migrations.TenantMigrations
{
    public partial class AddColumnQtyRcvInPurchaseReturnAndOthers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SOCode",
                schema: "Sales",
                table: "SalesDeliveryHeader",
                newName: "TransCode");

            migrationBuilder.AddColumn<decimal>(
                name: "QtyDlv",
                schema: "Sales",
                table: "SalesReturnDetailExchDiffItem",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<short>(
                name: "SrcTrans",
                schema: "Sales",
                table: "SalesDeliveryHeader",
                type: "smallint",
                nullable: false,
                defaultValue: (short)1);

            migrationBuilder.AddColumn<decimal>(
                name: "QtyRcv",
                schema: "Purchasing",
                table: "PurchaseReturnDetailExchDiffItem",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            // Reorder column Purchasing.PurchaseReturnDetailExchDiffItem
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
CREATE TABLE Purchasing.Tmp_PurchaseReturnDetailExchDiffItem
	(
	Id bigint NOT NULL IDENTITY (1, 1),
	Code varchar(17) NOT NULL,
	[LineNo] smallint NOT NULL,
	ReturnDetailId bigint NULL,
	ItemId int NOT NULL,
	UomId int NOT NULL,
	UnitId int NOT NULL,
	Qty decimal(18, 2) NOT NULL,
	QtyRcv decimal(18, 2) NOT NULL,
	WarehouseCode varchar(8) NULL,
	UnitPrice decimal(19, 6) NOT NULL,
	TaxId int NULL,
	TaxAmount decimal(18, 2) NOT NULL,
	NettPrice decimal(19, 6) NOT NULL,
	Total decimal(19, 6) NOT NULL,
	DPP decimal(19, 6) NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Purchasing.Tmp_PurchaseReturnDetailExchDiffItem SET (LOCK_ESCALATION = TABLE)
GO
SET IDENTITY_INSERT Purchasing.Tmp_PurchaseReturnDetailExchDiffItem ON
GO
IF EXISTS(SELECT * FROM Purchasing.PurchaseReturnDetailExchDiffItem)
	 EXEC('INSERT INTO Purchasing.Tmp_PurchaseReturnDetailExchDiffItem (Id, Code, [LineNo], ReturnDetailId, ItemId, UomId, UnitId, Qty, QtyRcv, WarehouseCode, UnitPrice, TaxId, TaxAmount, NettPrice, Total, DPP)
		SELECT Id, Code, [LineNo], ReturnDetailId, ItemId, UomId, UnitId, Qty, QtyRcv, WarehouseCode, UnitPrice, TaxId, TaxAmount, NettPrice, Total, DPP FROM Purchasing.PurchaseReturnDetailExchDiffItem WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT Purchasing.Tmp_PurchaseReturnDetailExchDiffItem OFF
GO
DROP TABLE Purchasing.PurchaseReturnDetailExchDiffItem
GO
EXECUTE sp_rename N'Purchasing.Tmp_PurchaseReturnDetailExchDiffItem', N'PurchaseReturnDetailExchDiffItem', 'OBJECT' 
GO
ALTER TABLE Purchasing.PurchaseReturnDetailExchDiffItem ADD CONSTRAINT
	PK_PurchaseReturnDetailExchDiffItem PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
COMMIT";
            migrationBuilder.Sql(sql);

			// Reorder column Sales.SalesDeliveryHeader
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
CREATE TABLE Sales.Tmp_SalesDeliveryHeader
	(
	Code varchar(17) NOT NULL,
	Date date NOT NULL,
	SrcTrans smallint NOT NULL,
	TransCode varchar(17) NOT NULL,
	CustCode varchar(8) NOT NULL,
	WarehouseCode varchar(8) NOT NULL,
	ShippedBy bigint NOT NULL,
	CurrCode varchar(3) NOT NULL,
	Rate decimal(18, 2) NOT NULL,
	ShipmentFee decimal(18, 2) NOT NULL,
	HandlingFee decimal(18, 2) NOT NULL,
	SubTotal decimal(18, 2) NOT NULL,
	FinalDiscPercent decimal(5, 2) NOT NULL,
	FinalDisc decimal(18, 2) NOT NULL,
	IncludeTax bit NOT NULL,
	TaxAmount decimal(18, 2) NOT NULL,
	Total decimal(18, 2) NOT NULL,
	DPP decimal(18, 2) NOT NULL,
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
ALTER TABLE Sales.Tmp_SalesDeliveryHeader SET (LOCK_ESCALATION = TABLE)
GO
IF EXISTS(SELECT * FROM Sales.SalesDeliveryHeader)
	 EXEC('INSERT INTO Sales.Tmp_SalesDeliveryHeader (Code, Date, SrcTrans, TransCode, CustCode, WarehouseCode, ShippedBy, CurrCode, Rate, ShipmentFee, HandlingFee, SubTotal, FinalDiscPercent, FinalDisc, IncludeTax, TaxAmount, Total, DPP, Notes, Mark, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, ApprovedBy, ApprovedDate)
		SELECT Code, Date, SrcTrans, TransCode, CustCode, WarehouseCode, ShippedBy, CurrCode, Rate, ShipmentFee, HandlingFee, SubTotal, FinalDiscPercent, FinalDisc, IncludeTax, TaxAmount, Total, DPP, Notes, Mark, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, ApprovedBy, ApprovedDate FROM Sales.SalesDeliveryHeader WITH (HOLDLOCK TABLOCKX)')
GO
DROP TABLE Sales.SalesDeliveryHeader
GO
EXECUTE sp_rename N'Sales.Tmp_SalesDeliveryHeader', N'SalesDeliveryHeader', 'OBJECT' 
GO
ALTER TABLE Sales.SalesDeliveryHeader ADD CONSTRAINT
	PK_SalesDeliveryHeader PRIMARY KEY CLUSTERED 
	(
	Code
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
COMMIT";
            migrationBuilder.Sql(sql);

            // Reorder column Sales.SalesReturnDetailExchDiffItem
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
CREATE TABLE Sales.Tmp_SalesReturnDetailExchDiffItem
	(
	Id bigint NOT NULL IDENTITY (1, 1),
	Code varchar(17) NOT NULL,
	[LineNo] smallint NOT NULL,
	ReturnDetailId bigint NULL,
	ItemId int NOT NULL,
	UomId int NOT NULL,
	UnitId int NOT NULL,
	Qty decimal(18, 2) NOT NULL,
	QtyDlv decimal(18, 2) NOT NULL,
	WarehouseCode varchar(8) NULL,
	UnitPrice decimal(19, 6) NOT NULL,
	TaxId int NULL,
	TaxAmount decimal(18, 2) NOT NULL,
	NettPrice decimal(19, 6) NOT NULL,
	Total decimal(19, 6) NOT NULL,
	DPP decimal(19, 6) NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Sales.Tmp_SalesReturnDetailExchDiffItem SET (LOCK_ESCALATION = TABLE)
GO
SET IDENTITY_INSERT Sales.Tmp_SalesReturnDetailExchDiffItem ON
GO
IF EXISTS(SELECT * FROM Sales.SalesReturnDetailExchDiffItem)
	 EXEC('INSERT INTO Sales.Tmp_SalesReturnDetailExchDiffItem (Id, Code, [LineNo], ReturnDetailId, ItemId, UomId, UnitId, Qty, QtyDlv, WarehouseCode, UnitPrice, TaxId, TaxAmount, NettPrice, Total, DPP)
		SELECT Id, Code, [LineNo], ReturnDetailId, ItemId, UomId, UnitId, Qty, QtyDlv, WarehouseCode, UnitPrice, TaxId, TaxAmount, NettPrice, Total, DPP FROM Sales.SalesReturnDetailExchDiffItem WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT Sales.Tmp_SalesReturnDetailExchDiffItem OFF
GO
DROP TABLE Sales.SalesReturnDetailExchDiffItem
GO
EXECUTE sp_rename N'Sales.Tmp_SalesReturnDetailExchDiffItem', N'SalesReturnDetailExchDiffItem', 'OBJECT' 
GO
ALTER TABLE Sales.SalesReturnDetailExchDiffItem ADD CONSTRAINT
	PK_SalesReturnDetailExchDiffItem PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
COMMIT";
            migrationBuilder.Sql(sql);

			// Execute sp_refreshview Sales.vwSalesDeliveryHeader
			sql = @"exec sp_refreshview 'Sales.vwSalesDeliveryHeader'";
            migrationBuilder.Sql(sql);

            // Execute sp_refreshview Sales.vwSalesReturnDetailExchDiffItem
            sql = @"exec sp_refreshview 'Sales.vwSalesReturnDetailExchDiffItem'";
            migrationBuilder.Sql(sql);

            // Alter view Inventory.vwTransferStockHeader
            sql = @"ALTER VIEW [Inventory].[vwTransferStockHeader]
AS
    SELECT ts_h.*,
        w_f.Initial AS WarehouseInitialFrom,
        w_t.Initial AS WarehouseInitialTo,
        u_c.Initial AS CreatedInitial,
        u_u.Initial AS UpdatedInitial,
        u_a.Initial AS ApprovedInitial,
        CASE ts_h.Mark
            WHEN 'A' THEN 'Active'
            WHEN 'V' THEN 'Void' END AS [Status],
        CASE ts_h.[Type]
            WHEN 1 THEN 'Barang Masuk'
            WHEN 2 THEN 'Barang Keluar'
            WHEN 3 THEN 'Transfer Langsung' END AS TypeInitial
    FROM Inventory.TransferStockHeader ts_h
    LEFT JOIN Inventory.Warehouse w_f
        ON w_f.Code = ts_h.WarehouseCodeFrom
    LEFT JOIN Inventory.Warehouse w_t
        ON w_t.Code = ts_h.WarehouseCodeTo
    LEFT JOIN SystemManagement.[User] u_c
        ON u_c.Id = ts_h.CreatedBy
    LEFT JOIN SystemManagement.[User] u_u
        ON u_u.Id = ts_h.UpdatedBy
    LEFT JOIN SystemManagement.[User] u_a
        ON u_a.Id = ts_h.ApprovedBy";
            migrationBuilder.Sql(sql);

            // Alter view Inventory.vwTransferStockDetail
            sql = @"ALTER VIEW [Inventory].[vwTransferStockDetail]
AS
    SELECT ts_d.*,
        i.[Name] AS ItemName,
        uom_c.UnitEquivalent AS UnitName
    FROM Inventory.TransferStockDetail ts_d
    LEFT JOIN Inventory.Item i
        ON i.Id = ts_d.ItemId
    LEFT JOIN Inventory.UoMConversion uom_c
        ON uom_c.Id = ts_d.UnitId";
            migrationBuilder.Sql(sql);

            // Drop view Purchasing.vwPurchaseReturnDetailExchDiffItem
            sql = @"DROP VIEW Purchasing.VwPurchaseReturnDetailExchDiffItem";
            migrationBuilder.Sql(sql);

            // Create view Purchasing.vwPurchaseReturnDetailExchDiffItem
            sql = @"CREATE VIEW [Purchasing].[vwPurchaseReturnDetailExchDiffItem]
AS
	SELECT pr_d.*,
		i.[Name] AS ItemName,
		i.Initial AS ItemInitial,
		i.UomBuyId AS ItemUomBuyId,
		uom_c_b.UnitEquivalent AS ItemUomBuyName,
		i.BuyPrice AS ItemBuyPrice,
		uom.Initial AS UomInitial,
		uom_c.UnitEquivalent AS UnitName
	FROM Purchasing.PurchaseReturnDetailExchDiffItem pr_d
	LEFT JOIN Inventory.Item i
		ON i.Id = pr_d.ItemId
	LEFT JOIN Inventory.UoMConversion uom_c_b
		ON uom_c_b.Id = i.UomBuyId
	LEFT JOIN Inventory.UoM uom
		ON uom.Id = pr_d.UomId
	LEFT JOIN Inventory.UoMConversion uom_c
		ON uom_c.Id = pr_d.UnitId";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QtyDlv",
                schema: "Sales",
                table: "SalesReturnDetailExchDiffItem");

            migrationBuilder.DropColumn(
                name: "SrcTrans",
                schema: "Sales",
                table: "SalesDeliveryHeader");

            migrationBuilder.DropColumn(
                name: "QtyRcv",
                schema: "Purchasing",
                table: "PurchaseReturnDetailExchDiffItem");

            migrationBuilder.RenameColumn(
                name: "TransCode",
                schema: "Sales",
                table: "SalesDeliveryHeader",
                newName: "SOCode");
        }
    }
}
