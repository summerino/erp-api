using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Web.API.Migrations.TenantMigrations
{
    public partial class AddColumnFromDirectInvoiceAndOthers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Type",
                schema: "Inventory",
                table: "StockMutation",
                type: "varchar(5)",
                unicode: false,
                maxLength: 5,
                nullable: false,
                defaultValue: "OH");

            migrationBuilder.AddColumn<bool>(
                name: "FromDirectInvoice",
                schema: "Sales",
                table: "SalesOrderHeader",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "FromDirectInvoice",
                schema: "Sales",
                table: "SalesInvoiceHeader",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "FromDirectInvoice",
                schema: "Sales",
                table: "SalesDeliveryHeader",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // Reorder column Inventory.StockMutation
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
CREATE TABLE Inventory.Tmp_StockMutation
	(
	Id bigint NOT NULL IDENTITY (1, 1),
	WarehouseCode varchar(8) NOT NULL,
	Date date NOT NULL,
	ItemId int NOT NULL,
	UomId int NOT NULL,
	UnitId int NOT NULL,
	Qty decimal(18, 6) NOT NULL,
	BaseUnit int NOT NULL,
	BaseQty decimal(18, 6) NOT NULL,
	RefCode1 varchar(17) NOT NULL,
	RefDetailId1 bigint NOT NULL,
	RefCode2 varchar(17) NULL,
	Type varchar(5) NOT NULL,
	Src varchar(10) NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Inventory.Tmp_StockMutation SET (LOCK_ESCALATION = TABLE)
GO
SET IDENTITY_INSERT Inventory.Tmp_StockMutation ON
GO
IF EXISTS(SELECT * FROM Inventory.StockMutation)
	 EXEC('INSERT INTO Inventory.Tmp_StockMutation (Id, WarehouseCode, Date, ItemId, UomId, UnitId, Qty, BaseUnit, BaseQty, RefCode1, RefDetailId1, RefCode2, Type, Src)
		SELECT Id, WarehouseCode, Date, ItemId, UomId, UnitId, Qty, BaseUnit, BaseQty, RefCode1, RefDetailId1, RefCode2, Type, Src FROM Inventory.StockMutation WITH (HOLDLOCK TABLOCKX)')
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
	 EXEC('INSERT INTO Sales.Tmp_SalesDeliveryHeader (Code, Date, SrcTrans, TransCode, CustCode, WarehouseCode, ShippedBy, CurrCode, Rate, ShipmentFee, HandlingFee, SubTotal, FinalDiscPercent, FinalDisc, IncludeTax, TaxAmount, Total, DPP, Notes, FromDirectInvoice, Mark, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, ApprovedBy, ApprovedDate)
		SELECT Code, Date, SrcTrans, TransCode, CustCode, WarehouseCode, ShippedBy, CurrCode, Rate, ShipmentFee, HandlingFee, SubTotal, FinalDiscPercent, FinalDisc, IncludeTax, TaxAmount, Total, DPP, Notes, FromDirectInvoice, Mark, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, ApprovedBy, ApprovedDate FROM Sales.SalesDeliveryHeader WITH (HOLDLOCK TABLOCKX)')
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

			// Reorder column Sales.SalesInvoiceHeader
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
CREATE TABLE Sales.Tmp_SalesInvoiceHeader
	(
	Code varchar(17) NOT NULL,
	Date date NOT NULL,
	DueDate date NOT NULL,
	SOCode varchar(17) NULL,
	CustCode varchar(8) NOT NULL,
	IssuedBy bigint NOT NULL,
	CurrCode varchar(3) NOT NULL,
	PaidAmount decimal(18, 2) NOT NULL,
	Total decimal(18, 2) NOT NULL,
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
ALTER TABLE Sales.Tmp_SalesInvoiceHeader SET (LOCK_ESCALATION = TABLE)
GO
IF EXISTS(SELECT * FROM Sales.SalesInvoiceHeader)
	 EXEC('INSERT INTO Sales.Tmp_SalesInvoiceHeader (Code, Date, DueDate, SOCode, CustCode, IssuedBy, CurrCode, PaidAmount, Total, Notes, FromDirectInvoice, Mark, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, ApprovedBy, ApprovedDate)
		SELECT Code, Date, DueDate, SOCode, CustCode, IssuedBy, CurrCode, PaidAmount, Total, Notes, FromDirectInvoice, Mark, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, ApprovedBy, ApprovedDate FROM Sales.SalesInvoiceHeader WITH (HOLDLOCK TABLOCKX)')
GO
DROP TABLE Sales.SalesInvoiceHeader
GO
EXECUTE sp_rename N'Sales.Tmp_SalesInvoiceHeader', N'SalesInvoiceHeader', 'OBJECT' 
GO
ALTER TABLE Sales.SalesInvoiceHeader ADD CONSTRAINT
	PK_SalesInvoiceHeader PRIMARY KEY CLUSTERED 
	(
	Code
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
CREATE TABLE Sales.Tmp_SalesOrderHeader
	(
	Code varchar(17) NOT NULL,
	Date date NOT NULL,
	CustCode varchar(8) NOT NULL,
	SalesBy bigint NOT NULL,
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
	ApprovedDate datetime NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Sales.Tmp_SalesOrderHeader SET (LOCK_ESCALATION = TABLE)
GO
IF EXISTS(SELECT * FROM Sales.SalesOrderHeader)
	 EXEC('INSERT INTO Sales.Tmp_SalesOrderHeader (Code, Date, CustCode, SalesBy, WarehouseCode, CurrCode, Rate, ShipmentFee, HandlingFee, SubTotal, FinalDiscPercent, FinalDisc, IncludeTax, TaxAmount, Total, DPP, Notes, FromDirectInvoice, Mark, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, ApprovedBy, ApprovedDate)
		SELECT Code, Date, CustCode, SalesBy, WarehouseCode, CurrCode, Rate, ShipmentFee, HandlingFee, SubTotal, FinalDiscPercent, FinalDisc, IncludeTax, TaxAmount, Total, DPP, Notes, FromDirectInvoice, Mark, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, ApprovedBy, ApprovedDate FROM Sales.SalesOrderHeader WITH (HOLDLOCK TABLOCKX)')
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
COMMIT";
            migrationBuilder.Sql(sql);

            // Execute sp_refreshview Sales.vwSalesDeliveryHeader
            sql = @"exec sp_refreshview 'Sales.vwSalesDeliveryHeader'";
            migrationBuilder.Sql(sql);

			// Execute sp_refreshview Sales.vwSalesInvoiceHeader
			sql = @"exec sp_refreshview 'Sales.vwSalesInvoiceHeader'";
            migrationBuilder.Sql(sql);

			// Execute sp_refreshview Sales.vwSalesOrderHeader
			sql = @"exec sp_refreshview 'Sales.vwSalesOrderHeader'";
            migrationBuilder.Sql(sql);
		}

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                schema: "Inventory",
                table: "StockMutation");

            migrationBuilder.DropColumn(
                name: "FromDirectInvoice",
                schema: "Sales",
                table: "SalesOrderHeader");

            migrationBuilder.DropColumn(
                name: "FromDirectInvoice",
                schema: "Sales",
                table: "SalesInvoiceHeader");

            migrationBuilder.DropColumn(
                name: "FromDirectInvoice",
                schema: "Sales",
                table: "SalesDeliveryHeader");
        }
    }
}
