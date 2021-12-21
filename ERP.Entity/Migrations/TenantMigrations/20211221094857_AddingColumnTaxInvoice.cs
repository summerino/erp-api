using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AddingColumnTaxInvoice : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "TaxInvoiceDate",
                schema: "Sales",
                table: "SalesDeliveryHeader",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxInvoiceNo",
                schema: "Sales",
                table: "SalesDeliveryHeader",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TaxInvoiceDate",
                schema: "Purchasing",
                table: "PurchaseReceiveHeader",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxInvoiceNo",
                schema: "Purchasing",
                table: "PurchaseReceiveHeader",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TaxInvoiceDate",
                schema: "Purchasing",
                table: "PurchaseInvoiceHeader",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxInvoiceNo",
                schema: "Purchasing",
                table: "PurchaseInvoiceHeader",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true);

            // Reorder column Purchasing.PurchaseReceiveHeader
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
	Total decimal(18, 2) NOT NULL,
	DPP decimal(18, 2) NOT NULL,
	PaidAmount decimal(19, 6) NOT NULL,
	TaxInvoiceDate date NULL,
	TaxInvoiceNo varchar(50) NULL,
	Mark varchar(3) NOT NULL,
	CreatedBy int NOT NULL,
	CreatedDate datetime NOT NULL,
	UpdatedBy int NOT NULL,
	UpdatedDate datetime NOT NULL,
	ApprovedBy int NULL,
	ApprovedDate datetime NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Purchasing.Tmp_PurchaseReceiveHeader SET (LOCK_ESCALATION = TABLE)
GO
IF EXISTS(SELECT * FROM Purchasing.PurchaseReceiveHeader)
	 EXEC('INSERT INTO Purchasing.Tmp_PurchaseReceiveHeader (Code, Date, TransCode, RefNo, SrcTrans, SupCode, ReceiveBy, CurrCode, Rate, ShipmentFee, HandlingFee, SubTotal, FinalDiscPercent, FinalDisc, IncludeTax, TaxAmount, Total, DPP, PaidAmount, TaxInvoiceDate, TaxInvoiceNo, Mark, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, ApprovedBy, ApprovedDate)
		SELECT Code, Date, TransCode, RefNo, SrcTrans, SupCode, ReceiveBy, CurrCode, Rate, ShipmentFee, HandlingFee, SubTotal, FinalDiscPercent, FinalDisc, IncludeTax, TaxAmount, Total, DPP, PaidAmount, TaxInvoiceDate, TaxInvoiceNo, Mark, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, ApprovedBy, ApprovedDate FROM Purchasing.PurchaseReceiveHeader WITH (HOLDLOCK TABLOCKX)')
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

            // Reorder column Purchasing.PurchaseInvoiceHeader
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
ALTER TABLE Purchasing.PurchaseInvoiceHeader
	DROP CONSTRAINT FK_PurchaseInvoiceHeader_Supplier_SupCode
GO
ALTER TABLE General.Supplier SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Purchasing.PurchaseInvoiceHeader
	DROP CONSTRAINT FK_PurchaseInvoiceHeader_PurchaseOrderHeader_POCode
GO
ALTER TABLE Purchasing.PurchaseOrderHeader SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Purchasing.PurchaseInvoiceHeader
	DROP CONSTRAINT FK_PurchaseInvoiceHeader_Employee_IssuedBy
GO
ALTER TABLE General.Employee SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Purchasing.PurchaseInvoiceHeader
	DROP CONSTRAINT FK_PurchaseInvoiceHeader_Currency_CurrCode
GO
ALTER TABLE General.Currency SET (LOCK_ESCALATION = TABLE)
GO
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
	Total decimal(18, 2) NOT NULL,
	DPP decimal(18, 2) NOT NULL,
	PaidAmount decimal(19, 6) NOT NULL,
	TaxInvoiceDate date NULL,
	TaxInvoiceNo varchar(50) NULL,
	Mark varchar(3) NOT NULL,
	CreatedBy int NOT NULL,
	CreatedDate datetime NOT NULL,
	UpdatedBy int NOT NULL,
	UpdatedDate datetime NOT NULL,
	ApprovedBy int NULL,
	ApprovedDate datetime NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Purchasing.Tmp_PurchaseReceiveHeader SET (LOCK_ESCALATION = TABLE)
GO
IF EXISTS(SELECT * FROM Purchasing.PurchaseReceiveHeader)
	 EXEC('INSERT INTO Purchasing.Tmp_PurchaseReceiveHeader (Code, Date, TransCode, RefNo, SrcTrans, SupCode, ReceiveBy, CurrCode, Rate, ShipmentFee, HandlingFee, SubTotal, FinalDiscPercent, FinalDisc, IncludeTax, TaxAmount, Total, DPP, PaidAmount, TaxInvoiceDate, TaxInvoiceNo, Mark, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, ApprovedBy, ApprovedDate)
		SELECT Code, Date, TransCode, RefNo, SrcTrans, SupCode, ReceiveBy, CurrCode, Rate, ShipmentFee, HandlingFee, SubTotal, FinalDiscPercent, FinalDisc, IncludeTax, TaxAmount, Total, DPP, PaidAmount, TaxInvoiceDate, TaxInvoiceNo, Mark, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, ApprovedBy, ApprovedDate FROM Purchasing.PurchaseReceiveHeader WITH (HOLDLOCK TABLOCKX)')
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
CREATE TABLE Purchasing.Tmp_PurchaseInvoiceHeader
	(
	Code varchar(17) NOT NULL,
	Date date NOT NULL,
	DueDate date NOT NULL,
	POCode varchar(17) NULL,
	RefNo varchar(30) NULL,
	SupCode varchar(8) NOT NULL,
	IssuedBy bigint NOT NULL,
	CurrCode varchar(3) NOT NULL,
	PaidAmount decimal(19, 6) NOT NULL,
	Total decimal(18, 2) NOT NULL,
	TaxInvoiceDate date NULL,
	TaxInvoiceNo varchar(50) NULL,
	Notes varchar(256) NULL,
	CreatedBy int NOT NULL,
	CreatedDate datetime NOT NULL,
	UpdatedBy int NOT NULL,
	UpdatedDate datetime NOT NULL,
	Mark varchar(3) NOT NULL,
	ApprovedBy int NULL,
	ApprovedDate datetime NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Purchasing.Tmp_PurchaseInvoiceHeader SET (LOCK_ESCALATION = TABLE)
GO
IF EXISTS(SELECT * FROM Purchasing.PurchaseInvoiceHeader)
	 EXEC('INSERT INTO Purchasing.Tmp_PurchaseInvoiceHeader (Code, Date, DueDate, POCode, RefNo, SupCode, IssuedBy, CurrCode, PaidAmount, Total, TaxInvoiceDate, TaxInvoiceNo, Notes, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, Mark, ApprovedBy, ApprovedDate)
		SELECT Code, Date, DueDate, POCode, RefNo, SupCode, IssuedBy, CurrCode, PaidAmount, Total, TaxInvoiceDate, TaxInvoiceNo, Notes, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, Mark, ApprovedBy, ApprovedDate FROM Purchasing.PurchaseInvoiceHeader WITH (HOLDLOCK TABLOCKX)')
GO
ALTER TABLE Purchasing.PurchaseInvoiceDebitMemo
	DROP CONSTRAINT FK_PurchaseInvoiceDebitMemo_PurchaseInvoiceHeader_InvCode
GO
ALTER TABLE Purchasing.PurchaseInvoiceDetail
	DROP CONSTRAINT FK_PurchaseInvoiceDetail_PurchaseInvoiceHeader_Code
GO
DROP TABLE Purchasing.PurchaseInvoiceHeader
GO
EXECUTE sp_rename N'Purchasing.Tmp_PurchaseInvoiceHeader', N'PurchaseInvoiceHeader', 'OBJECT' 
GO
ALTER TABLE Purchasing.PurchaseInvoiceHeader ADD CONSTRAINT
	PK_PurchaseInvoiceHeader PRIMARY KEY CLUSTERED 
	(
	Code
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX IX_PurchaseInvoiceHeader_CurrCode ON Purchasing.PurchaseInvoiceHeader
	(
	CurrCode
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_PurchaseInvoiceHeader_IssuedBy ON Purchasing.PurchaseInvoiceHeader
	(
	IssuedBy
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_PurchaseInvoiceHeader_POCode ON Purchasing.PurchaseInvoiceHeader
	(
	POCode
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_PurchaseInvoiceHeader_SupCode ON Purchasing.PurchaseInvoiceHeader
	(
	SupCode
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE Purchasing.PurchaseInvoiceHeader ADD CONSTRAINT
	FK_PurchaseInvoiceHeader_Currency_CurrCode FOREIGN KEY
	(
	CurrCode
	) REFERENCES General.Currency
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Purchasing.PurchaseInvoiceHeader ADD CONSTRAINT
	FK_PurchaseInvoiceHeader_Employee_IssuedBy FOREIGN KEY
	(
	IssuedBy
	) REFERENCES General.Employee
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
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
ALTER TABLE Purchasing.PurchaseInvoiceHeader ADD CONSTRAINT
	FK_PurchaseInvoiceHeader_Supplier_SupCode FOREIGN KEY
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
ALTER TABLE Purchasing.PurchaseInvoiceDetail SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Purchasing.PurchaseInvoiceDebitMemo ADD CONSTRAINT
	FK_PurchaseInvoiceDebitMemo_PurchaseInvoiceHeader_InvCode FOREIGN KEY
	(
	InvCode
	) REFERENCES Purchasing.PurchaseInvoiceHeader
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Purchasing.PurchaseInvoiceDebitMemo SET (LOCK_ESCALATION = TABLE)
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
	PaidAmount decimal(19, 6) NOT NULL,
	TaxInvoiceDate date NULL,
	TaxInvoiceNo varchar(50) NULL,
	Notes varchar(256) NULL,
	FromDirectInvoice bit NOT NULL,
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
	 EXEC('INSERT INTO Sales.Tmp_SalesDeliveryHeader (Code, Date, SrcTrans, TransCode, CustCode, WarehouseCode, ShippedBy, CurrCode, Rate, ShipmentFee, HandlingFee, SubTotal, FinalDiscPercent, FinalDisc, IncludeTax, TaxAmount, Total, DPP, PaidAmount, TaxInvoiceDate, TaxInvoiceNo, Notes, FromDirectInvoice, Mark, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, ApprovedBy, ApprovedDate)
		SELECT Code, Date, SrcTrans, TransCode, CustCode, WarehouseCode, ShippedBy, CurrCode, Rate, ShipmentFee, HandlingFee, SubTotal, FinalDiscPercent, FinalDisc, IncludeTax, TaxAmount, Total, DPP, PaidAmount, TaxInvoiceDate, TaxInvoiceNo, Notes, FromDirectInvoice, Mark, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, ApprovedBy, ApprovedDate FROM Sales.SalesDeliveryHeader WITH (HOLDLOCK TABLOCKX)')
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

			// Refresh view Purchasing.vwPurchaseReceiveHeader
			sql = @"exec sp_refreshview 'Purchasing.vwPurchaseReceiveHeader'";
            migrationBuilder.Sql(sql);

			// Refresh view Purchasing.vwPurchaseInvoiceHeader
			sql = @"exec sp_refreshview 'Purchasing.vwPurchaseInvoiceHeader'";
            migrationBuilder.Sql(sql);

			// Refresh view Sales.vwSalesDeliveryHeader
			sql = @"exec sp_refreshview 'Sales.vwSalesDeliveryHeader'";
            migrationBuilder.Sql(sql);
		}

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TaxInvoiceDate",
                schema: "Sales",
                table: "SalesDeliveryHeader");

            migrationBuilder.DropColumn(
                name: "TaxInvoiceNo",
                schema: "Sales",
                table: "SalesDeliveryHeader");

            migrationBuilder.DropColumn(
                name: "TaxInvoiceDate",
                schema: "Purchasing",
                table: "PurchaseReceiveHeader");

            migrationBuilder.DropColumn(
                name: "TaxInvoiceNo",
                schema: "Purchasing",
                table: "PurchaseReceiveHeader");

            migrationBuilder.DropColumn(
                name: "TaxInvoiceDate",
                schema: "Purchasing",
                table: "PurchaseInvoiceHeader");

            migrationBuilder.DropColumn(
                name: "TaxInvoiceNo",
                schema: "Purchasing",
                table: "PurchaseInvoiceHeader");
        }
    }
}
