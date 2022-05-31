using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AddingColumnExemptTax : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExemptCoaCode",
                schema: "General",
                table: "Tax",
                type: "varchar(6)",
                unicode: false,
                maxLength: 6,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ExemptRate",
                schema: "General",
                table: "Tax",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ExemptTaxAmount",
                schema: "Sales",
                table: "SalesReturnHeader",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ExemptTaxAmount",
                schema: "Sales",
                table: "SalesReturnDetail",
                type: "decimal(19,6)",
                precision: 19,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "TaxAmount",
                schema: "Sales",
                table: "SalesReturnDetailExchDiffItem",
                type: "decimal(19,6)",
                precision: 19,
                scale: 6,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AddColumn<decimal>(
                name: "ExemptTaxAmount",
                schema: "Sales",
                table: "SalesReturnDetailExchDiffItem",
                type: "decimal(19,6)",
                precision: 19,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

			migrationBuilder.AddColumn<decimal>(
                name: "ExemptTaxAmount",
                schema: "Sales",
                table: "SalesOrderHeader",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ExemptTaxAmount",
                schema: "Sales",
                table: "SalesOrderDetail",
                type: "decimal(19,6)",
                precision: 19,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ExemptTaxAmount",
                schema: "Sales",
                table: "SalesInvoiceDetail",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ExemptTaxAmount",
                schema: "Sales",
                table: "SalesDeliveryHeader",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ExemptTaxAmount",
                schema: "Sales",
                table: "SalesDeliveryDetail",
                type: "decimal(19,6)",
                precision: 19,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ExemptTaxAmount",
                schema: "Purchasing",
                table: "PurchaseReturnHeader",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ExemptTaxAmount",
                schema: "Purchasing",
                table: "PurchaseReturnDetail",
                type: "decimal(19,6)",
                precision: 19,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "TaxAmount",
                schema: "Purchasing",
                table: "PurchaseReturnDetailExchDiffItem",
                type: "decimal(19,6)",
                precision: 19,
                scale: 6,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AddColumn<decimal>(
                name: "ExemptTaxAmount",
                schema: "Purchasing",
                table: "PurchaseReturnDetailExchDiffItem",
                type: "decimal(19,6)",
                precision: 19,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

			migrationBuilder.AddColumn<decimal>(
                name: "ExemptTaxAmount",
                schema: "Purchasing",
                table: "PurchaseReceiveHeader",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ExemptTaxAmount",
                schema: "Purchasing",
                table: "PurchaseReceiveDetail",
                type: "decimal(19,6)",
                precision: 19,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ExemptTaxAmount",
                schema: "Purchasing",
                table: "PurchaseOrderHeader",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ExemptTaxAmount",
                schema: "Purchasing",
                table: "PurchaseOrderDetail",
                type: "decimal(19,6)",
                precision: 19,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ExemptTaxAmount",
                schema: "Purchasing",
                table: "PurchaseInvoiceDetail",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ExemptTaxAmount",
                schema: "MobileSales",
                table: "MobileOrderHeader",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ExemptTaxAmount",
                schema: "MobileSales",
                table: "MobileOrderDetail",
                type: "decimal(19,6)",
                precision: 19,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            // Reorder column General.Tax
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
CREATE TABLE General.Tmp_Tax
	(
	Id int NOT NULL IDENTITY (1, 1),
	Initial varchar(20) NOT NULL,
	Name varchar(50) NOT NULL,
	TypeId smallint NOT NULL,
	Rate decimal(5, 2) NOT NULL,
	ExemptRate decimal(5, 2) NOT NULL,
	CoaCode varchar(6) NULL,
	ExemptCoaCode varchar(6) NULL,
	Seq smallint NOT NULL,
	IsActive bit NOT NULL,
	CreatedBy int NOT NULL,
	CreatedDate datetime NOT NULL,
	UpdatedBy int NOT NULL,
	UpdatedDate datetime NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE General.Tmp_Tax SET (LOCK_ESCALATION = TABLE)
GO
SET IDENTITY_INSERT General.Tmp_Tax ON
GO
IF EXISTS(SELECT * FROM General.Tax)
	 EXEC('INSERT INTO General.Tmp_Tax (Id, Initial, Name, TypeId, Rate, ExemptRate, CoaCode, ExemptCoaCode, Seq, IsActive, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate)
		SELECT Id, Initial, Name, TypeId, Rate, ExemptRate, CoaCode, ExemptCoaCode, Seq, IsActive, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate FROM General.Tax WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT General.Tmp_Tax OFF
GO
ALTER TABLE MobileCustomer.MobileOrderDetail
	DROP CONSTRAINT FK_MobileOrderDetail_Tax_TaxId
GO
ALTER TABLE MobileSales.MobileOrderDetail
	DROP CONSTRAINT FK_MobileOrderDetail_Tax_TaxId
GO
ALTER TABLE Inventory.Item
	DROP CONSTRAINT FK_Item_Tax_PurchaseTaxId
GO
ALTER TABLE Inventory.Item
	DROP CONSTRAINT FK_Item_Tax_SalesTaxId
GO
DROP TABLE General.Tax
GO
EXECUTE sp_rename N'General.Tmp_Tax', N'Tax', 'OBJECT' 
GO
ALTER TABLE General.Tax ADD CONSTRAINT
	PK_Tax PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX IX_Tax_CoaCode ON General.Tax
	(
	CoaCode
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.Item ADD CONSTRAINT
	FK_Item_Tax_PurchaseTaxId FOREIGN KEY
	(
	PurchaseTaxId
	) REFERENCES General.Tax
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.Item ADD CONSTRAINT
	FK_Item_Tax_SalesTaxId FOREIGN KEY
	(
	SalesTaxId
	) REFERENCES General.Tax
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.Item SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE MobileSales.MobileOrderDetail ADD CONSTRAINT
	FK_MobileOrderDetail_Tax_TaxId FOREIGN KEY
	(
	TaxId
	) REFERENCES General.Tax
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE MobileSales.MobileOrderDetail SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE MobileCustomer.MobileOrderDetail ADD CONSTRAINT
	FK_MobileOrderDetail_Tax_TaxId FOREIGN KEY
	(
	TaxId
	) REFERENCES General.Tax
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE MobileCustomer.MobileOrderDetail SET (LOCK_ESCALATION = TABLE)
GO
COMMIT";
            migrationBuilder.Sql(sql);

            // Reorder column Purchasing.PurchaseInvoiceDetail
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
ALTER TABLE Purchasing.PurchaseInvoiceDetail
	DROP CONSTRAINT FK_PurchaseInvoiceDetail_PurchaseReceiveHeader_RcvCode
GO
ALTER TABLE Purchasing.PurchaseReceiveHeader SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Purchasing.PurchaseInvoiceDetail
	DROP CONSTRAINT FK_PurchaseInvoiceDetail_PurchaseInvoiceHeader_Code
GO
ALTER TABLE Purchasing.PurchaseInvoiceHeader SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE Purchasing.Tmp_PurchaseInvoiceDetail
	(
	Id bigint NOT NULL IDENTITY (1, 1),
	Code varchar(17) NOT NULL,
	[LineNo] smallint NOT NULL,
	RcvCode varchar(17) NOT NULL,
	ShipmentFee decimal(18, 2) NOT NULL,
	HandlingFee decimal(18, 2) NOT NULL,
	SubTotal decimal(18, 2) NOT NULL,
	FinalDisc decimal(18, 2) NOT NULL,
	TaxAmount decimal(18, 2) NOT NULL,
	ExemptTaxAmount decimal(18, 2) NOT NULL,
	Total decimal(19, 6) NOT NULL,
	DPP decimal(19, 6) NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Purchasing.Tmp_PurchaseInvoiceDetail SET (LOCK_ESCALATION = TABLE)
GO
SET IDENTITY_INSERT Purchasing.Tmp_PurchaseInvoiceDetail ON
GO
IF EXISTS(SELECT * FROM Purchasing.PurchaseInvoiceDetail)
	 EXEC('INSERT INTO Purchasing.Tmp_PurchaseInvoiceDetail (Id, Code, [LineNo], RcvCode, ShipmentFee, HandlingFee, SubTotal, FinalDisc, TaxAmount, ExemptTaxAmount, Total, DPP)
		SELECT Id, Code, [LineNo], RcvCode, ShipmentFee, HandlingFee, SubTotal, FinalDisc, TaxAmount, ExemptTaxAmount, Total, DPP FROM Purchasing.PurchaseInvoiceDetail WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT Purchasing.Tmp_PurchaseInvoiceDetail OFF
GO
DROP TABLE Purchasing.PurchaseInvoiceDetail
GO
EXECUTE sp_rename N'Purchasing.Tmp_PurchaseInvoiceDetail', N'PurchaseInvoiceDetail', 'OBJECT' 
GO
ALTER TABLE Purchasing.PurchaseInvoiceDetail ADD CONSTRAINT
	PK_PurchaseInvoiceDetail PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX IX_PurchaseInvoiceDetail_Code ON Purchasing.PurchaseInvoiceDetail
	(
	Code
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_PurchaseInvoiceDetail_RcvCode ON Purchasing.PurchaseInvoiceDetail
	(
	RcvCode
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE Purchasing.PurchaseInvoiceDetail ADD CONSTRAINT
	FK_PurchaseInvoiceDetail_PurchaseInvoiceHeader_Code FOREIGN KEY
	(
	Code
	) REFERENCES Purchasing.PurchaseInvoiceHeader
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Purchasing.PurchaseInvoiceDetail ADD CONSTRAINT
	FK_PurchaseInvoiceDetail_PurchaseReceiveHeader_RcvCode FOREIGN KEY
	(
	RcvCode
	) REFERENCES Purchasing.PurchaseReceiveHeader
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
COMMIT";
            migrationBuilder.Sql(sql);

            // Reorder column Purchasing.PurchaseOrderDetail
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
CREATE TABLE Purchasing.Tmp_PurchaseOrderDetail
	(
	Id bigint NOT NULL IDENTITY (1, 1),
	Code varchar(17) NOT NULL,
	[LineNo] smallint NOT NULL,
	ItemId int NOT NULL,
	UomId int NOT NULL,
	UnitId int NOT NULL,
	Qty decimal(18, 2) NOT NULL,
	Length decimal(18, 2) NULL,
	Width decimal(18, 2) NULL,
	Height decimal(18, 2) NULL,
	Weight decimal(18, 3) NULL,
	DimensionMeasurement varchar(10) NULL,
	WeightMeasurement varchar(10) NULL,
	QtyRcv decimal(18, 2) NULL,
	UnitPrice decimal(19, 6) NOT NULL,
	Disc decimal(19, 6) NOT NULL,
	FinalDiscHeader decimal(19, 6) NOT NULL,
	TaxId int NULL,
	TaxAmount decimal(19, 6) NOT NULL,
	ExemptTaxAmount decimal(19, 6) NOT NULL,
	NettPrice decimal(19, 6) NOT NULL,
	Total decimal(19, 6) NOT NULL,
	DPP decimal(19, 6) NOT NULL,
	Notes varchar(256) NULL,
	CoaInventory varchar(6) NULL,
	CoaCOGS varchar(6) NULL,
	CoaPurc varchar(6) NULL,
	CoaPurcDisc varchar(6) NULL,
	CoaPurcReturn varchar(6) NULL,
	Type int NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Purchasing.Tmp_PurchaseOrderDetail SET (LOCK_ESCALATION = TABLE)
GO
SET IDENTITY_INSERT Purchasing.Tmp_PurchaseOrderDetail ON
GO
IF EXISTS(SELECT * FROM Purchasing.PurchaseOrderDetail)
	 EXEC('INSERT INTO Purchasing.Tmp_PurchaseOrderDetail (Id, Code, [LineNo], ItemId, UomId, UnitId, Qty, Length, Width, Height, Weight, DimensionMeasurement, WeightMeasurement, QtyRcv, UnitPrice, Disc, FinalDiscHeader, TaxId, TaxAmount, ExemptTaxAmount, NettPrice, Total, DPP, Notes, CoaInventory, CoaCOGS, CoaPurc, CoaPurcDisc, CoaPurcReturn, Type)
		SELECT Id, Code, [LineNo], ItemId, UomId, UnitId, Qty, Length, Width, Height, Weight, DimensionMeasurement, WeightMeasurement, QtyRcv, UnitPrice, Disc, FinalDiscHeader, TaxId, TaxAmount, ExemptTaxAmount, NettPrice, Total, DPP, Notes, CoaInventory, CoaCOGS, CoaPurc, CoaPurcDisc, CoaPurcReturn, Type FROM Purchasing.PurchaseOrderDetail WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT Purchasing.Tmp_PurchaseOrderDetail OFF
GO
DROP TABLE Purchasing.PurchaseOrderDetail
GO
EXECUTE sp_rename N'Purchasing.Tmp_PurchaseOrderDetail', N'PurchaseOrderDetail', 'OBJECT' 
GO
ALTER TABLE Purchasing.PurchaseOrderDetail ADD CONSTRAINT
	PK_PurchaseOrderDetail PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
COMMIT";
            migrationBuilder.Sql(sql);

            // Reorder column Purchasing.PurchaseOrderHeader
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
CREATE TABLE Purchasing.Tmp_PurchaseOrderHeader
	(
	Code varchar(17) NOT NULL,
	Date date NOT NULL,
	SupCode varchar(8) NOT NULL,
	RequestBy bigint NOT NULL,
	WarehouseCode varchar(8) NULL,
	CurrCode varchar(3) NOT NULL,
	Rate decimal(18, 2) NOT NULL,
	ShipmentFee decimal(18, 2) NOT NULL,
	HandlingFee decimal(18, 2) NOT NULL,
	SubTotal decimal(18, 2) NOT NULL,
	FinalDiscPercent decimal(5, 2) NOT NULL,
	FinalDisc decimal(18, 2) NOT NULL,
	IncludeTax bit NOT NULL,
	TaxAmount decimal(18, 2) NOT NULL,
	ExemptTaxAmount decimal(18, 2) NOT NULL,
	Total decimal(18, 2) NOT NULL,
	DPP decimal(18, 2) NOT NULL,
	Notes varchar(256) NULL,
	Mark varchar(3) NOT NULL,
	CreatedBy int NOT NULL,
	CreatedDate datetime NOT NULL,
	UpdatedBy int NOT NULL,
	UpdatedDate datetime NOT NULL,
	ApprovedBy int NULL,
	ApprovedDate datetime NULL,
	ViewedBy int NULL,
	ViewedDate datetime NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Purchasing.Tmp_PurchaseOrderHeader SET (LOCK_ESCALATION = TABLE)
GO
IF EXISTS(SELECT * FROM Purchasing.PurchaseOrderHeader)
	 EXEC('INSERT INTO Purchasing.Tmp_PurchaseOrderHeader (Code, Date, SupCode, RequestBy, WarehouseCode, CurrCode, Rate, ShipmentFee, HandlingFee, SubTotal, FinalDiscPercent, FinalDisc, IncludeTax, TaxAmount, ExemptTaxAmount, Total, DPP, Notes, Mark, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, ApprovedBy, ApprovedDate, ViewedBy, ViewedDate)
		SELECT Code, Date, SupCode, RequestBy, WarehouseCode, CurrCode, Rate, ShipmentFee, HandlingFee, SubTotal, FinalDiscPercent, FinalDisc, IncludeTax, TaxAmount, ExemptTaxAmount, Total, DPP, Notes, Mark, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, ApprovedBy, ApprovedDate, ViewedBy, ViewedDate FROM Purchasing.PurchaseOrderHeader WITH (HOLDLOCK TABLOCKX)')
GO
ALTER TABLE Purchasing.PurchaseInvoiceHeader
	DROP CONSTRAINT FK_PurchaseInvoiceHeader_PurchaseOrderHeader_POCode
GO
DROP TABLE Purchasing.PurchaseOrderHeader
GO
EXECUTE sp_rename N'Purchasing.Tmp_PurchaseOrderHeader', N'PurchaseOrderHeader', 'OBJECT' 
GO
ALTER TABLE Purchasing.PurchaseOrderHeader ADD CONSTRAINT
	PK_PurchaseOrderHeader PRIMARY KEY CLUSTERED 
	(
	Code
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Purchasing.PurchaseInvoiceHeader ADD CONSTRAINT
	FK_PurchaseInvoiceHeader_PurchaseOrderHeader_POCode FOREIGN KEY
	(
	POCode
	) REFERENCES Purchasing.PurchaseOrderHeader
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Purchasing.PurchaseInvoiceHeader SET (LOCK_ESCALATION = TABLE)
GO
COMMIT";
            migrationBuilder.Sql(sql);

            // Reorder column Purchasing.PurchaseReceiveDetail
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
CREATE TABLE Purchasing.Tmp_PurchaseReceiveDetail
	(
	Id bigint NOT NULL IDENTITY (1, 1),
	Code varchar(17) NOT NULL,
	[LineNo] smallint NOT NULL,
	TransDetailId bigint NULL,
	ItemId int NOT NULL,
	Qty decimal(18, 2) NOT NULL,
	UomId int NOT NULL,
	UnitId int NOT NULL,
	Length decimal(18, 2) NULL,
	Width decimal(18, 2) NULL,
	Height decimal(18, 2) NULL,
	Weight decimal(18, 3) NULL,
	DimensionMeasurement varchar(10) NULL,
	WeightMeasurement varchar(10) NULL,
	UnitPrice decimal(19, 6) NOT NULL,
	Disc decimal(19, 6) NOT NULL,
	FinalDiscHeader decimal(19, 6) NOT NULL,
	TaxId int NULL,
	TaxAmount decimal(19, 6) NOT NULL,
	ExemptTaxAmount decimal(19, 6) NOT NULL,
	NettPrice decimal(19, 6) NOT NULL,
	Total decimal(19, 6) NOT NULL,
	DPP decimal(19, 6) NOT NULL,
	WarehouseCode varchar(8) NOT NULL,
	Type int NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Purchasing.Tmp_PurchaseReceiveDetail SET (LOCK_ESCALATION = TABLE)
GO
SET IDENTITY_INSERT Purchasing.Tmp_PurchaseReceiveDetail ON
GO
IF EXISTS(SELECT * FROM Purchasing.PurchaseReceiveDetail)
	 EXEC('INSERT INTO Purchasing.Tmp_PurchaseReceiveDetail (Id, Code, [LineNo], TransDetailId, ItemId, Qty, UomId, UnitId, Length, Width, Height, Weight, DimensionMeasurement, WeightMeasurement, UnitPrice, Disc, FinalDiscHeader, TaxId, TaxAmount, ExemptTaxAmount, NettPrice, Total, DPP, WarehouseCode, Type)
		SELECT Id, Code, [LineNo], TransDetailId, ItemId, Qty, UomId, UnitId, Length, Width, Height, Weight, DimensionMeasurement, WeightMeasurement, UnitPrice, Disc, FinalDiscHeader, TaxId, TaxAmount, ExemptTaxAmount, NettPrice, Total, DPP, WarehouseCode, Type FROM Purchasing.PurchaseReceiveDetail WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT Purchasing.Tmp_PurchaseReceiveDetail OFF
GO
DROP TABLE Purchasing.PurchaseReceiveDetail
GO
EXECUTE sp_rename N'Purchasing.Tmp_PurchaseReceiveDetail', N'PurchaseReceiveDetail', 'OBJECT' 
GO
ALTER TABLE Purchasing.PurchaseReceiveDetail ADD CONSTRAINT
	PK_PurchaseReceiveDetail PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
COMMIT";
            migrationBuilder.Sql(sql);

            // Reorder column Purchasing.PurchaseReceiveHeader
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
CREATE TABLE Purchasing.Tmp_PurchaseReceiveHeader
	(
	Code varchar(17) NOT NULL,
	Date date NOT NULL,
	TransCode varchar(17) NOT NULL,
	RefNo varchar(30) NULL,
	SrcTrans smallint NOT NULL,
	SupCode varchar(8) NOT NULL,
	ReceiveBy bigint NOT NULL,
	CurrCode varchar(3) NOT NULL,
	Rate decimal(18, 2) NOT NULL,
	ShipmentFee decimal(18, 2) NOT NULL,
	HandlingFee decimal(18, 2) NOT NULL,
	SubTotal decimal(18, 2) NOT NULL,
	FinalDiscPercent decimal(5, 2) NOT NULL,
	FinalDisc decimal(18, 2) NOT NULL,
	IncludeTax bit NOT NULL,
	TaxAmount decimal(18, 2) NOT NULL,
	ExemptTaxAmount decimal(18, 2) NOT NULL,
	Total decimal(18, 2) NOT NULL,
	DPP decimal(18, 2) NOT NULL,
	PaidAmount decimal(19, 6) NOT NULL,
	TaxInvoiceNo varchar(50) NULL,
	TaxInvoiceDate date NULL,
	Mark varchar(3) NOT NULL,
	CreatedBy int NOT NULL,
	CreatedDate datetime NOT NULL,
	UpdatedBy int NOT NULL,
	UpdatedDate datetime NOT NULL,
	ApprovedBy int NULL,
	ApprovedDate datetime NULL,
	ViewedBy int NULL,
	ViewedDate datetime NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Purchasing.Tmp_PurchaseReceiveHeader SET (LOCK_ESCALATION = TABLE)
GO
IF EXISTS(SELECT * FROM Purchasing.PurchaseReceiveHeader)
	 EXEC('INSERT INTO Purchasing.Tmp_PurchaseReceiveHeader (Code, Date, TransCode, RefNo, SrcTrans, SupCode, ReceiveBy, CurrCode, Rate, ShipmentFee, HandlingFee, SubTotal, FinalDiscPercent, FinalDisc, IncludeTax, TaxAmount, ExemptTaxAmount, Total, DPP, PaidAmount, TaxInvoiceNo, TaxInvoiceDate, Mark, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, ApprovedBy, ApprovedDate, ViewedBy, ViewedDate)
		SELECT Code, Date, TransCode, RefNo, SrcTrans, SupCode, ReceiveBy, CurrCode, Rate, ShipmentFee, HandlingFee, SubTotal, FinalDiscPercent, FinalDisc, IncludeTax, TaxAmount, ExemptTaxAmount, Total, DPP, PaidAmount, TaxInvoiceNo, TaxInvoiceDate, Mark, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, ApprovedBy, ApprovedDate, ViewedBy, ViewedDate FROM Purchasing.PurchaseReceiveHeader WITH (HOLDLOCK TABLOCKX)')
GO
ALTER TABLE Purchasing.PurchaseInvoiceDetail
	DROP CONSTRAINT FK_PurchaseInvoiceDetail_PurchaseReceiveHeader_RcvCode
GO
ALTER TABLE MobileWarehouse.MobileReceiveItemHeader
	DROP CONSTRAINT FK_MobileReceiveItemHeader_PurchaseReceiveHeader_RcvCode
GO
DROP TABLE Purchasing.PurchaseReceiveHeader
GO
EXECUTE sp_rename N'Purchasing.Tmp_PurchaseReceiveHeader', N'PurchaseReceiveHeader', 'OBJECT' 
GO
ALTER TABLE Purchasing.PurchaseReceiveHeader ADD CONSTRAINT
	PK_PurchaseReceiveHeader PRIMARY KEY CLUSTERED 
	(
	Code
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX IX_PurchaseReceiveHeader_TransCode ON Purchasing.PurchaseReceiveHeader
	(
	TransCode
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE MobileWarehouse.MobileReceiveItemHeader ADD CONSTRAINT
	FK_MobileReceiveItemHeader_PurchaseReceiveHeader_RcvCode FOREIGN KEY
	(
	RcvCode
	) REFERENCES Purchasing.PurchaseReceiveHeader
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE MobileWarehouse.MobileReceiveItemHeader SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Purchasing.PurchaseInvoiceDetail ADD CONSTRAINT
	FK_PurchaseInvoiceDetail_PurchaseReceiveHeader_RcvCode FOREIGN KEY
	(
	RcvCode
	) REFERENCES Purchasing.PurchaseReceiveHeader
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Purchasing.PurchaseInvoiceDetail SET (LOCK_ESCALATION = TABLE)
GO
COMMIT";
            migrationBuilder.Sql(sql);

            // Reorder column Purchasing.PurchaseReturnDetail
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
CREATE TABLE Purchasing.Tmp_PurchaseReturnDetail
	(
	Id bigint NOT NULL IDENTITY (1, 1),
	Code varchar(17) NOT NULL,
	[LineNo] smallint NOT NULL,
	RcvDetailId bigint NULL,
	ItemId int NOT NULL,
	UomId int NOT NULL,
	UnitId int NOT NULL,
	Qty decimal(18, 2) NOT NULL,
	QtyRcv decimal(18, 2) NOT NULL,
	WarehouseCode varchar(8) NOT NULL,
	WarehouseCodeIn varchar(8) NULL,
	Length decimal(18, 2) NULL,
	Width decimal(18, 2) NULL,
	Height decimal(18, 2) NULL,
	Weight decimal(18, 3) NULL,
	DimensionMeasurement varchar(10) NULL,
	WeightMeasurement varchar(10) NULL,
	UnitPrice decimal(19, 6) NOT NULL,
	Disc decimal(19, 6) NOT NULL,
	TaxId int NULL,
	TaxAmount decimal(19, 6) NOT NULL,
	ExemptTaxAmount decimal(19, 6) NOT NULL,
	NettPrice decimal(19, 6) NOT NULL,
	Total decimal(19, 6) NOT NULL,
	DPP decimal(19, 6) NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Purchasing.Tmp_PurchaseReturnDetail SET (LOCK_ESCALATION = TABLE)
GO
SET IDENTITY_INSERT Purchasing.Tmp_PurchaseReturnDetail ON
GO
IF EXISTS(SELECT * FROM Purchasing.PurchaseReturnDetail)
	 EXEC('INSERT INTO Purchasing.Tmp_PurchaseReturnDetail (Id, Code, [LineNo], RcvDetailId, ItemId, UomId, UnitId, Qty, QtyRcv, WarehouseCode, WarehouseCodeIn, Length, Width, Height, Weight, DimensionMeasurement, WeightMeasurement, UnitPrice, Disc, TaxId, TaxAmount, ExemptTaxAmount, NettPrice, Total, DPP)
		SELECT Id, Code, [LineNo], RcvDetailId, ItemId, UomId, UnitId, Qty, QtyRcv, WarehouseCode, WarehouseCodeIn, Length, Width, Height, Weight, DimensionMeasurement, WeightMeasurement, UnitPrice, Disc, TaxId, TaxAmount, ExemptTaxAmount, NettPrice, Total, DPP FROM Purchasing.PurchaseReturnDetail WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT Purchasing.Tmp_PurchaseReturnDetail OFF
GO
DROP TABLE Purchasing.PurchaseReturnDetail
GO
EXECUTE sp_rename N'Purchasing.Tmp_PurchaseReturnDetail', N'PurchaseReturnDetail', 'OBJECT' 
GO
ALTER TABLE Purchasing.PurchaseReturnDetail ADD CONSTRAINT
	PK_PurchaseReturnDetail PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
COMMIT";
            migrationBuilder.Sql(sql);

            // Reorder column Purchasing.PurchaseReturnDetailExchDiffItem
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
	TaxAmount decimal(19, 6) NOT NULL,
	ExemptTaxAmount decimal(19, 6) NOT NULL,
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
	 EXEC('INSERT INTO Purchasing.Tmp_PurchaseReturnDetailExchDiffItem (Id, Code, [LineNo], ReturnDetailId, ItemId, UomId, UnitId, Qty, QtyRcv, WarehouseCode, UnitPrice, TaxId, TaxAmount, ExemptTaxAmount, NettPrice, Total, DPP)
		SELECT Id, Code, [LineNo], ReturnDetailId, ItemId, UomId, UnitId, Qty, QtyRcv, WarehouseCode, UnitPrice, TaxId, TaxAmount, ExemptTaxAmount, NettPrice, Total, DPP FROM Purchasing.PurchaseReturnDetailExchDiffItem WITH (HOLDLOCK TABLOCKX)')
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

            // Reorder column Purchasing.PurchaseReturnHeader
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
CREATE TABLE Purchasing.Tmp_PurchaseReturnHeader
	(
	Code varchar(17) NOT NULL,
	Date date NOT NULL,
	RcvCode varchar(17) NULL,
	RefNo varchar(30) NULL,
	Type smallint NOT NULL,
	SupCode varchar(8) NOT NULL,
	ShippedBy bigint NOT NULL,
	CurrCode varchar(3) NOT NULL,
	Rate decimal(18, 2) NOT NULL,
	ShipmentFee decimal(18, 2) NOT NULL,
	HandlingFee decimal(18, 2) NOT NULL,
	SubTotal decimal(18, 2) NOT NULL,
	FinalDisc decimal(18, 2) NOT NULL,
	NoTax bit NOT NULL,
	IncludeTax bit NOT NULL,
	TaxAmount decimal(18, 2) NOT NULL,
	ExemptTaxAmount decimal(18, 2) NOT NULL,
	Total decimal(18, 2) NOT NULL,
	DPP decimal(18, 2) NOT NULL,
	TaxInvoiceNo varchar(50) NULL,
	TaxInvoiceDate date NULL,
	Notes varchar(256) NOT NULL,
	Mark varchar(3) NOT NULL,
	CreatedBy int NOT NULL,
	CreatedDate datetime NOT NULL,
	UpdatedBy int NOT NULL,
	UpdatedDate datetime NOT NULL,
	ApprovedBy int NULL,
	ApprovedDate datetime NULL,
	ViewedBy int NULL,
	ViewedDate datetime NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Purchasing.Tmp_PurchaseReturnHeader SET (LOCK_ESCALATION = TABLE)
GO
IF EXISTS(SELECT * FROM Purchasing.PurchaseReturnHeader)
	 EXEC('INSERT INTO Purchasing.Tmp_PurchaseReturnHeader (Code, Date, RcvCode, RefNo, Type, SupCode, ShippedBy, CurrCode, Rate, ShipmentFee, HandlingFee, SubTotal, FinalDisc, NoTax, IncludeTax, TaxAmount, ExemptTaxAmount, Total, DPP, TaxInvoiceNo, TaxInvoiceDate, Notes, Mark, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, ApprovedBy, ApprovedDate, ViewedBy, ViewedDate)
		SELECT Code, Date, RcvCode, RefNo, Type, SupCode, ShippedBy, CurrCode, Rate, ShipmentFee, HandlingFee, SubTotal, FinalDisc, NoTax, IncludeTax, TaxAmount, ExemptTaxAmount, Total, DPP, TaxInvoiceNo, TaxInvoiceDate, Notes, Mark, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, ApprovedBy, ApprovedDate, ViewedBy, ViewedDate FROM Purchasing.PurchaseReturnHeader WITH (HOLDLOCK TABLOCKX)')
GO
DROP TABLE Purchasing.PurchaseReturnHeader
GO
EXECUTE sp_rename N'Purchasing.Tmp_PurchaseReturnHeader', N'PurchaseReturnHeader', 'OBJECT' 
GO
ALTER TABLE Purchasing.PurchaseReturnHeader ADD CONSTRAINT
	PK_PurchaseReturnHeader PRIMARY KEY CLUSTERED 
	(
	Code
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
COMMIT";
            migrationBuilder.Sql(sql);

			// Reorder column Sales.SalesDeliveryDetail
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
CREATE TABLE Sales.Tmp_SalesDeliveryDetail
	(
	Id bigint NOT NULL IDENTITY (1, 1),
	Code varchar(17) NOT NULL,
	[LineNo] smallint NOT NULL,
	SODetailId bigint NULL,
	ItemId int NOT NULL,
	Qty decimal(18, 2) NOT NULL,
	UomId int NOT NULL,
	UnitId int NOT NULL,
	Length decimal(18, 2) NULL,
	Width decimal(18, 2) NULL,
	Height decimal(18, 2) NULL,
	Weight decimal(18, 3) NULL,
	DimensionMeasurement varchar(10) NULL,
	WeightMeasurement varchar(10) NULL,
	UnitPrice decimal(19, 6) NOT NULL,
	Disc decimal(19, 6) NOT NULL,
	FinalDiscHeader decimal(19, 6) NOT NULL,
	TaxId int NULL,
	TaxAmount decimal(19, 6) NOT NULL,
	ExemptTaxAmount decimal(19, 6) NOT NULL,
	NettPrice decimal(19, 6) NOT NULL,
	Total decimal(19, 6) NOT NULL,
	DPP decimal(19, 6) NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Sales.Tmp_SalesDeliveryDetail SET (LOCK_ESCALATION = TABLE)
GO
SET IDENTITY_INSERT Sales.Tmp_SalesDeliveryDetail ON
GO
IF EXISTS(SELECT * FROM Sales.SalesDeliveryDetail)
	 EXEC('INSERT INTO Sales.Tmp_SalesDeliveryDetail (Id, Code, [LineNo], SODetailId, ItemId, Qty, UomId, UnitId, Length, Width, Height, Weight, DimensionMeasurement, WeightMeasurement, UnitPrice, Disc, FinalDiscHeader, TaxId, TaxAmount, ExemptTaxAmount, NettPrice, Total, DPP)
		SELECT Id, Code, [LineNo], SODetailId, ItemId, Qty, UomId, UnitId, Length, Width, Height, Weight, DimensionMeasurement, WeightMeasurement, UnitPrice, Disc, FinalDiscHeader, TaxId, TaxAmount, ExemptTaxAmount, NettPrice, Total, DPP FROM Sales.SalesDeliveryDetail WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT Sales.Tmp_SalesDeliveryDetail OFF
GO
DROP TABLE Sales.SalesDeliveryDetail
GO
EXECUTE sp_rename N'Sales.Tmp_SalesDeliveryDetail', N'SalesDeliveryDetail', 'OBJECT' 
GO
ALTER TABLE Sales.SalesDeliveryDetail ADD CONSTRAINT
	PK_SalesDeliveryDetail PRIMARY KEY CLUSTERED 
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
	ExemptTaxAmount decimal(18, 2) NOT NULL,
	Total decimal(18, 2) NOT NULL,
	DPP decimal(18, 2) NOT NULL,
	PaidAmount decimal(19, 6) NOT NULL,
	TaxInvoiceNo varchar(50) NULL,
	TaxInvoiceDate date NULL,
	Notes varchar(256) NULL,
	FromDirectInvoice bit NOT NULL,
	Mark varchar(3) NOT NULL,
	CreatedBy int NOT NULL,
	CreatedDate datetime NOT NULL,
	UpdatedBy int NOT NULL,
	UpdatedDate datetime NOT NULL,
	ApprovedBy int NULL,
	ApprovedDate datetime NULL,
	ViewedBy int NULL,
	ViewedDate datetime NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Sales.Tmp_SalesDeliveryHeader SET (LOCK_ESCALATION = TABLE)
GO
IF EXISTS(SELECT * FROM Sales.SalesDeliveryHeader)
	 EXEC('INSERT INTO Sales.Tmp_SalesDeliveryHeader (Code, Date, SrcTrans, TransCode, CustCode, WarehouseCode, ShippedBy, CurrCode, Rate, ShipmentFee, HandlingFee, SubTotal, FinalDiscPercent, FinalDisc, IncludeTax, TaxAmount, ExemptTaxAmount, Total, DPP, PaidAmount, TaxInvoiceNo, TaxInvoiceDate, Notes, FromDirectInvoice, Mark, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, ApprovedBy, ApprovedDate, ViewedBy, ViewedDate)
		SELECT Code, Date, SrcTrans, TransCode, CustCode, WarehouseCode, ShippedBy, CurrCode, Rate, ShipmentFee, HandlingFee, SubTotal, FinalDiscPercent, FinalDisc, IncludeTax, TaxAmount, ExemptTaxAmount, Total, DPP, PaidAmount, TaxInvoiceNo, TaxInvoiceDate, Notes, FromDirectInvoice, Mark, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, ApprovedBy, ApprovedDate, ViewedBy, ViewedDate FROM Sales.SalesDeliveryHeader WITH (HOLDLOCK TABLOCKX)')
GO
ALTER TABLE Sales.SalesInvoiceDetail
	DROP CONSTRAINT FK_SalesInvoiceDetail_SalesDeliveryHeader_DOCode
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
CREATE NONCLUSTERED INDEX IX_SalesDeliveryHeader_TransCode ON Sales.SalesDeliveryHeader
	(
	TransCode
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Sales.SalesInvoiceDetail ADD CONSTRAINT
	FK_SalesInvoiceDetail_SalesDeliveryHeader_DOCode FOREIGN KEY
	(
	DOCode
	) REFERENCES Sales.SalesDeliveryHeader
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Sales.SalesInvoiceDetail SET (LOCK_ESCALATION = TABLE)
GO
COMMIT";
            migrationBuilder.Sql(sql);

			// Reorder column Sales.SalesInvoiceDetail
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
ALTER TABLE Sales.SalesInvoiceDetail
	DROP CONSTRAINT FK_SalesInvoiceDetail_SalesInvoiceHeader_Code
GO
ALTER TABLE Sales.SalesInvoiceHeader SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Sales.SalesInvoiceDetail
	DROP CONSTRAINT FK_SalesInvoiceDetail_SalesDeliveryHeader_DOCode
GO
ALTER TABLE Sales.SalesDeliveryHeader SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE Sales.Tmp_SalesInvoiceDetail
	(
	Id bigint NOT NULL IDENTITY (1, 1),
	Code varchar(17) NOT NULL,
	[LineNo] smallint NOT NULL,
	DOCode varchar(17) NOT NULL,
	ShipmentFee decimal(18, 2) NOT NULL,
	HandlingFee decimal(18, 2) NOT NULL,
	SubTotal decimal(18, 2) NOT NULL,
	FinalDisc decimal(18, 2) NOT NULL,
	TaxAmount decimal(18, 2) NOT NULL,
	ExemptTaxAmount decimal(18, 2) NOT NULL,
	Total decimal(19, 6) NOT NULL,
	DPP decimal(19, 6) NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Sales.Tmp_SalesInvoiceDetail SET (LOCK_ESCALATION = TABLE)
GO
SET IDENTITY_INSERT Sales.Tmp_SalesInvoiceDetail ON
GO
IF EXISTS(SELECT * FROM Sales.SalesInvoiceDetail)
	 EXEC('INSERT INTO Sales.Tmp_SalesInvoiceDetail (Id, Code, [LineNo], DOCode, ShipmentFee, HandlingFee, SubTotal, FinalDisc, TaxAmount, ExemptTaxAmount, Total, DPP)
		SELECT Id, Code, [LineNo], DOCode, ShipmentFee, HandlingFee, SubTotal, FinalDisc, TaxAmount, ExemptTaxAmount, Total, DPP FROM Sales.SalesInvoiceDetail WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT Sales.Tmp_SalesInvoiceDetail OFF
GO
DROP TABLE Sales.SalesInvoiceDetail
GO
EXECUTE sp_rename N'Sales.Tmp_SalesInvoiceDetail', N'SalesInvoiceDetail', 'OBJECT' 
GO
ALTER TABLE Sales.SalesInvoiceDetail ADD CONSTRAINT
	PK_SalesInvoiceDetail PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX IX_SalesInvoiceDetail_Code ON Sales.SalesInvoiceDetail
	(
	Code
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_SalesInvoiceDetail_DOCode ON Sales.SalesInvoiceDetail
	(
	DOCode
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE Sales.SalesInvoiceDetail ADD CONSTRAINT
	FK_SalesInvoiceDetail_SalesDeliveryHeader_DOCode FOREIGN KEY
	(
	DOCode
	) REFERENCES Sales.SalesDeliveryHeader
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Sales.SalesInvoiceDetail ADD CONSTRAINT
	FK_SalesInvoiceDetail_SalesInvoiceHeader_Code FOREIGN KEY
	(
	Code
	) REFERENCES Sales.SalesInvoiceHeader
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
COMMIT";
            migrationBuilder.Sql(sql);

			// Reorder column Sales.SalesOrderDetail
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
CREATE TABLE Sales.Tmp_SalesOrderDetail
	(
	Id bigint NOT NULL IDENTITY (1, 1),
	Code varchar(17) NOT NULL,
	[LineNo] smallint NOT NULL,
	ItemId int NOT NULL,
	UomId int NOT NULL,
	UnitId int NOT NULL,
	Qty decimal(18, 2) NOT NULL,
	Length decimal(18, 2) NULL,
	Width decimal(18, 2) NULL,
	Height decimal(18, 2) NULL,
	Weight decimal(18, 3) NULL,
	DimensionMeasurement varchar(10) NULL,
	WeightMeasurement varchar(10) NULL,
	QtyDlv decimal(18, 2) NULL,
	UnitPrice decimal(19, 6) NOT NULL,
	Disc decimal(19, 6) NOT NULL,
	FinalDiscHeader decimal(19, 6) NOT NULL,
	TaxId int NULL,
	TaxAmount decimal(19, 6) NOT NULL,
	ExemptTaxAmount decimal(19, 6) NOT NULL,
	NettPrice decimal(19, 6) NOT NULL,
	Total decimal(19, 6) NOT NULL,
	DPP decimal(19, 6) NOT NULL,
	Notes varchar(256) NULL,
	CoaInventory varchar(6) NULL,
	CoaCOGS varchar(6) NULL,
	CoaSls varchar(6) NULL,
	CoaSlsDisc varchar(6) NULL,
	CoaSlsReturn varchar(6) NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Sales.Tmp_SalesOrderDetail SET (LOCK_ESCALATION = TABLE)
GO
SET IDENTITY_INSERT Sales.Tmp_SalesOrderDetail ON
GO
IF EXISTS(SELECT * FROM Sales.SalesOrderDetail)
	 EXEC('INSERT INTO Sales.Tmp_SalesOrderDetail (Id, Code, [LineNo], ItemId, UomId, UnitId, Qty, Length, Width, Height, Weight, DimensionMeasurement, WeightMeasurement, QtyDlv, UnitPrice, Disc, FinalDiscHeader, TaxId, TaxAmount, ExemptTaxAmount, NettPrice, Total, DPP, Notes, CoaInventory, CoaCOGS, CoaSls, CoaSlsDisc, CoaSlsReturn)
		SELECT Id, Code, [LineNo], ItemId, UomId, UnitId, Qty, Length, Width, Height, Weight, DimensionMeasurement, WeightMeasurement, QtyDlv, UnitPrice, Disc, FinalDiscHeader, TaxId, TaxAmount, ExemptTaxAmount, NettPrice, Total, DPP, Notes, CoaInventory, CoaCOGS, CoaSls, CoaSlsDisc, CoaSlsReturn FROM Sales.SalesOrderDetail WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT Sales.Tmp_SalesOrderDetail OFF
GO
DROP TABLE Sales.SalesOrderDetail
GO
EXECUTE sp_rename N'Sales.Tmp_SalesOrderDetail', N'SalesOrderDetail', 'OBJECT' 
GO
ALTER TABLE Sales.SalesOrderDetail ADD CONSTRAINT
	PK_SalesOrderDetail PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
COMMIT";
            migrationBuilder.Sql(sql);

			// Reorder column Sales.SalesOrderHeader
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
ALTER TABLE Sales.SalesOrderHeader
	DROP CONSTRAINT FK_SalesOrderHeader_PaymentTerm_PaymentTermId
GO
ALTER TABLE General.PaymentTerm SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Sales.SalesOrderHeader
	DROP CONSTRAINT FK_SalesOrderHeader_Employee_SalesBy
GO
ALTER TABLE General.Employee SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Sales.SalesOrderHeader
	DROP CONSTRAINT FK_SalesOrderHeader_Customer_CustCode
GO
ALTER TABLE General.Customer SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE Sales.Tmp_SalesOrderHeader
	(
	Code varchar(17) NOT NULL,
	Date date NOT NULL,
	CustCode varchar(8) NOT NULL,
	SalesBy bigint NOT NULL,
	BillingAddressId int NULL,
	PaymentTermId int NULL,
	WarehouseCode varchar(8) NULL,
	CurrCode varchar(3) NOT NULL,
	Rate decimal(18, 2) NOT NULL,
	ShipmentFee decimal(18, 2) NOT NULL,
	HandlingFee decimal(18, 2) NOT NULL,
	SubTotal decimal(18, 2) NOT NULL,
	FinalDiscPercent decimal(5, 2) NOT NULL,
	FinalDisc decimal(18, 2) NOT NULL,
	IncludeTax bit NOT NULL,
	TaxAmount decimal(18, 2) NOT NULL,
	ExemptTaxAmount decimal(18, 2) NOT NULL,
	Total decimal(18, 2) NOT NULL,
	DPP decimal(18, 2) NOT NULL,
	Notes varchar(256) NULL,
	FromDirectInvoice bit NOT NULL,
	Mark varchar(3) NOT NULL,
	CreatedBy int NOT NULL,
	CreatedDate datetime NOT NULL,
	UpdatedBy int NOT NULL,
	UpdatedDate datetime NOT NULL,
	ApprovedBy int NULL,
	ApprovedDate datetime NULL,
	ViewedBy int NULL,
	ViewedDate datetime NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Sales.Tmp_SalesOrderHeader SET (LOCK_ESCALATION = TABLE)
GO
IF EXISTS(SELECT * FROM Sales.SalesOrderHeader)
	 EXEC('INSERT INTO Sales.Tmp_SalesOrderHeader (Code, Date, CustCode, SalesBy, BillingAddressId, PaymentTermId, WarehouseCode, CurrCode, Rate, ShipmentFee, HandlingFee, SubTotal, FinalDiscPercent, FinalDisc, IncludeTax, TaxAmount, ExemptTaxAmount, Total, DPP, Notes, FromDirectInvoice, Mark, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, ApprovedBy, ApprovedDate, ViewedBy, ViewedDate)
		SELECT Code, Date, CustCode, SalesBy, BillingAddressId, PaymentTermId, WarehouseCode, CurrCode, Rate, ShipmentFee, HandlingFee, SubTotal, FinalDiscPercent, FinalDisc, IncludeTax, TaxAmount, ExemptTaxAmount, Total, DPP, Notes, FromDirectInvoice, Mark, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, ApprovedBy, ApprovedDate, ViewedBy, ViewedDate FROM Sales.SalesOrderHeader WITH (HOLDLOCK TABLOCKX)')
GO
ALTER TABLE MobileCustomer.MobileOrderHeader
	DROP CONSTRAINT FK_MobileOrderHeader_SalesOrderHeader_SalesOrderCode
GO
ALTER TABLE MobileSales.MobileOrderHeader
	DROP CONSTRAINT FK_MobileOrderHeader_SalesOrderHeader_SalesOrderCode
GO
ALTER TABLE Sales.SalesInvoiceHeader
	DROP CONSTRAINT FK_SalesInvoiceHeader_SalesOrderHeader_SOCode
GO
DROP TABLE Sales.SalesOrderHeader
GO
EXECUTE sp_rename N'Sales.Tmp_SalesOrderHeader', N'SalesOrderHeader', 'OBJECT' 
GO
ALTER TABLE Sales.SalesOrderHeader ADD CONSTRAINT
	PK_SalesOrderHeader PRIMARY KEY CLUSTERED 
	(
	Code
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX IX_SalesOrderHeader_CustCode ON Sales.SalesOrderHeader
	(
	CustCode
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_SalesOrderHeader_PaymentTermId ON Sales.SalesOrderHeader
	(
	PaymentTermId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_SalesOrderHeader_SalesBy ON Sales.SalesOrderHeader
	(
	SalesBy
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE Sales.SalesOrderHeader ADD CONSTRAINT
	FK_SalesOrderHeader_Customer_CustCode FOREIGN KEY
	(
	CustCode
	) REFERENCES General.Customer
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Sales.SalesOrderHeader ADD CONSTRAINT
	FK_SalesOrderHeader_Employee_SalesBy FOREIGN KEY
	(
	SalesBy
	) REFERENCES General.Employee
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Sales.SalesOrderHeader WITH NOCHECK ADD CONSTRAINT
	FK_SalesOrderHeader_PaymentTerm_PaymentTermId FOREIGN KEY
	(
	PaymentTermId
	) REFERENCES General.PaymentTerm
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Sales.SalesOrderHeader
	NOCHECK CONSTRAINT FK_SalesOrderHeader_PaymentTerm_PaymentTermId
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Sales.SalesInvoiceHeader ADD CONSTRAINT
	FK_SalesInvoiceHeader_SalesOrderHeader_SOCode FOREIGN KEY
	(
	SOCode
	) REFERENCES Sales.SalesOrderHeader
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Sales.SalesInvoiceHeader SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE MobileSales.MobileOrderHeader ADD CONSTRAINT
	FK_MobileOrderHeader_SalesOrderHeader_SalesOrderCode FOREIGN KEY
	(
	SalesOrderCode
	) REFERENCES Sales.SalesOrderHeader
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE MobileSales.MobileOrderHeader SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE MobileCustomer.MobileOrderHeader ADD CONSTRAINT
	FK_MobileOrderHeader_SalesOrderHeader_SalesOrderCode FOREIGN KEY
	(
	SalesOrderCode
	) REFERENCES Sales.SalesOrderHeader
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE MobileCustomer.MobileOrderHeader SET (LOCK_ESCALATION = TABLE)
GO
COMMIT";
            migrationBuilder.Sql(sql);

			// Reorder column Sales.SalesReturnDetail
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
CREATE TABLE Sales.Tmp_SalesReturnDetail
	(
	Id bigint NOT NULL IDENTITY (1, 1),
	Code varchar(17) NOT NULL,
	[LineNo] smallint NOT NULL,
	TransDetailId bigint NULL,
	ItemId int NOT NULL,
	UomId int NOT NULL,
	UnitId int NOT NULL,
	Qty decimal(18, 2) NOT NULL,
	QtyDlv decimal(18, 2) NOT NULL,
	Length decimal(18, 2) NULL,
	Width decimal(18, 2) NULL,
	Height decimal(18, 2) NULL,
	Weight decimal(18, 3) NULL,
	DimensionMeasurement varchar(10) NULL,
	WeightMeasurement varchar(10) NULL,
	UnitPrice decimal(19, 6) NOT NULL,
	Disc decimal(19, 6) NOT NULL,
	TaxId int NULL,
	TaxAmount decimal(19, 6) NOT NULL,
	ExemptTaxAmount decimal(19, 6) NOT NULL,
	NettPrice decimal(19, 6) NOT NULL,
	Total decimal(19, 6) NOT NULL,
	DPP decimal(19, 6) NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Sales.Tmp_SalesReturnDetail SET (LOCK_ESCALATION = TABLE)
GO
SET IDENTITY_INSERT Sales.Tmp_SalesReturnDetail ON
GO
IF EXISTS(SELECT * FROM Sales.SalesReturnDetail)
	 EXEC('INSERT INTO Sales.Tmp_SalesReturnDetail (Id, Code, [LineNo], TransDetailId, ItemId, UomId, UnitId, Qty, QtyDlv, Length, Width, Height, Weight, DimensionMeasurement, WeightMeasurement, UnitPrice, Disc, TaxId, TaxAmount, ExemptTaxAmount, NettPrice, Total, DPP)
		SELECT Id, Code, [LineNo], TransDetailId, ItemId, UomId, UnitId, Qty, QtyDlv, Length, Width, Height, Weight, DimensionMeasurement, WeightMeasurement, UnitPrice, Disc, TaxId, TaxAmount, ExemptTaxAmount, NettPrice, Total, DPP FROM Sales.SalesReturnDetail WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT Sales.Tmp_SalesReturnDetail OFF
GO
DROP TABLE Sales.SalesReturnDetail
GO
EXECUTE sp_rename N'Sales.Tmp_SalesReturnDetail', N'SalesReturnDetail', 'OBJECT' 
GO
ALTER TABLE Sales.SalesReturnDetail ADD CONSTRAINT
	PK_SalesReturnDetail PRIMARY KEY CLUSTERED 
	(
	Id
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
	TaxAmount decimal(19, 6) NOT NULL,
	ExemptTaxAmount decimal(19, 6) NOT NULL,
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
	 EXEC('INSERT INTO Sales.Tmp_SalesReturnDetailExchDiffItem (Id, Code, [LineNo], ReturnDetailId, ItemId, UomId, UnitId, Qty, QtyDlv, WarehouseCode, UnitPrice, TaxId, TaxAmount, ExemptTaxAmount, NettPrice, Total, DPP)
		SELECT Id, Code, [LineNo], ReturnDetailId, ItemId, UomId, UnitId, Qty, QtyDlv, WarehouseCode, UnitPrice, TaxId, TaxAmount, ExemptTaxAmount, NettPrice, Total, DPP FROM Sales.SalesReturnDetailExchDiffItem WITH (HOLDLOCK TABLOCKX)')
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

            // Reorder column Sales.SalesReturnHeader
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
CREATE TABLE Sales.Tmp_SalesReturnHeader
	(
	Code varchar(17) NOT NULL,
	Date date NOT NULL,
	SrcTrans smallint NOT NULL,
	TransCode varchar(17) NULL,
	Type smallint NOT NULL,
	CustCode varchar(8) NOT NULL,
	WarehouseCode varchar(8) NOT NULL,
	SalesBy bigint NOT NULL,
	CurrCode varchar(3) NOT NULL,
	Rate decimal(18, 2) NOT NULL,
	ShipmentFee decimal(18, 2) NOT NULL,
	HandlingFee decimal(18, 2) NOT NULL,
	SubTotal decimal(18, 2) NOT NULL,
	FinalDisc decimal(18, 2) NOT NULL,
	NoTax bit NOT NULL,
	IncludeTax bit NOT NULL,
	TaxAmount decimal(18, 2) NOT NULL,
	ExemptTaxAmount decimal(18, 2) NOT NULL,
	Total decimal(18, 2) NOT NULL,
	DPP decimal(18, 2) NOT NULL,
	TaxInvoiceNo varchar(50) NULL,
	TaxInvoiceDate date NULL,
	Notes varchar(256) NOT NULL,
	Mark varchar(3) NOT NULL,
	CreatedBy int NOT NULL,
	CreatedDate datetime NOT NULL,
	UpdatedBy int NOT NULL,
	UpdatedDate datetime NOT NULL,
	ApprovedBy int NULL,
	ApprovedDate datetime NULL,
	ViewedBy int NULL,
	ViewedDate datetime NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Sales.Tmp_SalesReturnHeader SET (LOCK_ESCALATION = TABLE)
GO
IF EXISTS(SELECT * FROM Sales.SalesReturnHeader)
	 EXEC('INSERT INTO Sales.Tmp_SalesReturnHeader (Code, Date, SrcTrans, TransCode, Type, CustCode, WarehouseCode, SalesBy, CurrCode, Rate, ShipmentFee, HandlingFee, SubTotal, FinalDisc, NoTax, IncludeTax, TaxAmount, ExemptTaxAmount, Total, DPP, TaxInvoiceNo, TaxInvoiceDate, Notes, Mark, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, ApprovedBy, ApprovedDate, ViewedBy, ViewedDate)
		SELECT Code, Date, SrcTrans, TransCode, Type, CustCode, WarehouseCode, SalesBy, CurrCode, Rate, ShipmentFee, HandlingFee, SubTotal, FinalDisc, NoTax, IncludeTax, TaxAmount, ExemptTaxAmount, Total, DPP, TaxInvoiceNo, TaxInvoiceDate, Notes, Mark, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, ApprovedBy, ApprovedDate, ViewedBy, ViewedDate FROM Sales.SalesReturnHeader WITH (HOLDLOCK TABLOCKX)')
GO
DROP TABLE Sales.SalesReturnHeader
GO
EXECUTE sp_rename N'Sales.Tmp_SalesReturnHeader', N'SalesReturnHeader', 'OBJECT' 
GO
ALTER TABLE Sales.SalesReturnHeader ADD CONSTRAINT
	PK_SalesReturnHeader PRIMARY KEY CLUSTERED 
	(
	Code
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
COMMIT";
            migrationBuilder.Sql(sql);
			
			// Refresh view General.vwTax
			sql = @"EXEC sp_refreshview 'General.vwTax'";
            migrationBuilder.Sql(sql);

            // Refresh view Purchasing.vwPurchaseOrderDetail
            sql = @"EXEC sp_refreshview 'Purchasing.vwPurchaseOrderDetail'";
            migrationBuilder.Sql(sql);

            // Refresh view Purchasing.vwPurchaseOrderHeader
            sql = @"EXEC sp_refreshview 'Purchasing.vwPurchaseOrderHeader'";
            migrationBuilder.Sql(sql);

            // Refresh view Purchasing.vwPurchaseReceiveDetail
            sql = @"EXEC sp_refreshview 'Purchasing.vwPurchaseReceiveDetail'";
            migrationBuilder.Sql(sql);

            // Refresh view Purchasing.vwPurchaseReceiveHeader
            sql = @"EXEC sp_refreshview 'Purchasing.vwPurchaseReceiveHeader'";
            migrationBuilder.Sql(sql);

            // Refresh view Purchasing.vwPurchaseReturnDetail
            sql = @"EXEC sp_refreshview 'Purchasing.vwPurchaseReturnDetail'";
			migrationBuilder.Sql(sql);

			// Refresh view Purchasing.vwPurchaseReturnDetailExchDiffItem
			sql = @"EXEC sp_refreshview 'Purchasing.vwPurchaseReturnDetailExchDiffItem'";
            migrationBuilder.Sql(sql);

			// Refresh view Purchasing.vwPurchaseReturnHeader
			sql = @"EXEC sp_refreshview 'Purchasing.vwPurchaseOrderDetail'";
            migrationBuilder.Sql(sql);

			// Refresh view Sales.vwSalesDeliveryDetail
			sql = @"EXEC sp_refreshview 'Sales.vwSalesDeliveryDetail'";
            migrationBuilder.Sql(sql);

			// Refresh view Sales.vwSalesDeliveryHeader
			sql = @"EXEC sp_refreshview 'Sales.vwSalesDeliveryHeader'";
            migrationBuilder.Sql(sql);

			// Refresh view Sales.vwSalesOrderDetail
			sql = @"EXEC sp_refreshview 'Sales.vwSalesOrderDetail'";
            migrationBuilder.Sql(sql);

			// Refresh view Sales.vwSalesOrderHeader
			sql = @"EXEC sp_refreshview 'Sales.vwSalesOrderHeader'";
            migrationBuilder.Sql(sql);

			// Refresh view Sales.vwSalesReturnDetail
			sql = @"EXEC sp_refreshview 'Sales.vwSalesReturnDetail'";
            migrationBuilder.Sql(sql);

			// Refresh view Sales.vwSalesReturnDetailExchDiffItem
			sql = @"EXEC sp_refreshview 'Sales.vwSalesReturnDetailExchDiffItem'";
            migrationBuilder.Sql(sql);

			// Refresh view Sales.vwSalesReturnHeader
			sql = @"EXEC sp_refreshview 'Sales.vwSalesReturnHeader'";
            migrationBuilder.Sql(sql);
		}

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExemptCoaCode",
                schema: "General",
                table: "Tax");

            migrationBuilder.DropColumn(
                name: "ExemptRate",
                schema: "General",
                table: "Tax");

            migrationBuilder.DropColumn(
                name: "ExemptTaxAmount",
                schema: "Sales",
                table: "SalesReturnHeader");

            migrationBuilder.DropColumn(
                name: "ExemptTaxAmount",
                schema: "Sales",
                table: "SalesReturnDetail");

            migrationBuilder.DropColumn(
                name: "ExemptTaxAmount",
                schema: "Sales",
                table: "SalesReturnDetailExchDiffItem");

			migrationBuilder.DropColumn(
                name: "ExemptTaxAmount",
                schema: "Sales",
                table: "SalesOrderHeader");

            migrationBuilder.DropColumn(
                name: "ExemptTaxAmount",
                schema: "Sales",
                table: "SalesOrderDetail");

            migrationBuilder.DropColumn(
                name: "ExemptTaxAmount",
                schema: "Sales",
                table: "SalesInvoiceDetail");

            migrationBuilder.DropColumn(
                name: "ExemptTaxAmount",
                schema: "Sales",
                table: "SalesDeliveryHeader");

            migrationBuilder.DropColumn(
                name: "ExemptTaxAmount",
                schema: "Sales",
                table: "SalesDeliveryDetail");

            migrationBuilder.DropColumn(
                name: "ExemptTaxAmount",
                schema: "Purchasing",
                table: "PurchaseReturnHeader");

            migrationBuilder.DropColumn(
                name: "ExemptTaxAmount",
                schema: "Purchasing",
                table: "PurchaseReturnDetail");

            migrationBuilder.DropColumn(
                name: "ExemptTaxAmount",
                schema: "Purchasing",
                table: "PurchaseReturnDetailExchDiffItem");

			migrationBuilder.DropColumn(
                name: "ExemptTaxAmount",
                schema: "Purchasing",
                table: "PurchaseReceiveHeader");

            migrationBuilder.DropColumn(
                name: "ExemptTaxAmount",
                schema: "Purchasing",
                table: "PurchaseReceiveDetail");

            migrationBuilder.DropColumn(
                name: "ExemptTaxAmount",
                schema: "Purchasing",
                table: "PurchaseOrderHeader");

            migrationBuilder.DropColumn(
                name: "ExemptTaxAmount",
                schema: "Purchasing",
                table: "PurchaseOrderDetail");

            migrationBuilder.DropColumn(
                name: "ExemptTaxAmount",
                schema: "Purchasing",
                table: "PurchaseInvoiceDetail");

            migrationBuilder.DropColumn(
                name: "ExemptTaxAmount",
                schema: "MobileSales",
                table: "MobileOrderHeader");

            migrationBuilder.DropColumn(
                name: "ExemptTaxAmount",
                schema: "MobileSales",
                table: "MobileOrderDetail");

            migrationBuilder.AlterColumn<decimal>(
                name: "TaxAmount",
                schema: "Sales",
                table: "SalesReturnDetailExchDiffItem",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(19,6)",
                oldPrecision: 19,
                oldScale: 6);

            migrationBuilder.AlterColumn<decimal>(
                name: "TaxAmount",
                schema: "Purchasing",
                table: "PurchaseReturnDetailExchDiffItem",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(19,6)",
                oldPrecision: 19,
                oldScale: 6);
		}
    }
}
