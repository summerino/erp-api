using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AddingColumnFinalDiscHeader : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "FinalDiscHeader",
                schema: "Sales",
                table: "SalesOrderDetail",
                type: "decimal(19,6)",
                precision: 19,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FinalDiscHeader",
                schema: "Sales",
                table: "SalesDeliveryDetail",
                type: "decimal(19,6)",
                precision: 19,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FinalDiscHeader",
                schema: "Purchasing",
                table: "PurchaseReceiveDetail",
                type: "decimal(19,6)",
                precision: 19,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FinalDiscHeader",
                schema: "Purchasing",
                table: "PurchaseOrderDetail",
                type: "decimal(19,6)",
                precision: 19,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            // Reorder column Sales.SalesOrderDetail
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
	 EXEC('INSERT INTO Sales.Tmp_SalesOrderDetail (Id, Code, [LineNo], ItemId, UomId, UnitId, Qty, Length, Width, Height, Weight, DimensionMeasurement, WeightMeasurement, QtyDlv, UnitPrice, Disc, FinalDiscHeader, TaxId, TaxAmount, NettPrice, Total, DPP, Notes, CoaInventory, CoaCOGS, CoaSls, CoaSlsDisc, CoaSlsReturn)
		SELECT Id, Code, [LineNo], ItemId, UomId, UnitId, Qty, Length, Width, Height, Weight, DimensionMeasurement, WeightMeasurement, QtyDlv, UnitPrice, Disc, FinalDiscHeader, TaxId, TaxAmount, NettPrice, Total, DPP, Notes, CoaInventory, CoaCOGS, CoaSls, CoaSlsDisc, CoaSlsReturn FROM Sales.SalesOrderDetail WITH (HOLDLOCK TABLOCKX)')
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
	 EXEC('INSERT INTO Sales.Tmp_SalesDeliveryDetail (Id, Code, [LineNo], SODetailId, ItemId, Qty, UomId, UnitId, Length, Width, Height, Weight, DimensionMeasurement, WeightMeasurement, UnitPrice, Disc, FinalDiscHeader, TaxId, TaxAmount, NettPrice, Total, DPP)
		SELECT Id, Code, [LineNo], SODetailId, ItemId, Qty, UomId, UnitId, Length, Width, Height, Weight, DimensionMeasurement, WeightMeasurement, UnitPrice, Disc, FinalDiscHeader, TaxId, TaxAmount, NettPrice, Total, DPP FROM Sales.SalesDeliveryDetail WITH (HOLDLOCK TABLOCKX)')
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
	 EXEC('INSERT INTO Purchasing.Tmp_PurchaseReceiveDetail (Id, Code, [LineNo], TransDetailId, ItemId, Qty, UomId, UnitId, Length, Width, Height, Weight, DimensionMeasurement, WeightMeasurement, UnitPrice, Disc, FinalDiscHeader, TaxId, TaxAmount, NettPrice, Total, DPP, WarehouseCode, Type)
		SELECT Id, Code, [LineNo], TransDetailId, ItemId, Qty, UomId, UnitId, Length, Width, Height, Weight, DimensionMeasurement, WeightMeasurement, UnitPrice, Disc, FinalDiscHeader, TaxId, TaxAmount, NettPrice, Total, DPP, WarehouseCode, Type FROM Purchasing.PurchaseReceiveDetail WITH (HOLDLOCK TABLOCKX)')
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
	 EXEC('INSERT INTO Purchasing.Tmp_PurchaseOrderDetail (Id, Code, [LineNo], ItemId, UomId, UnitId, Qty, Length, Width, Height, Weight, DimensionMeasurement, WeightMeasurement, QtyRcv, UnitPrice, Disc, FinalDiscHeader, TaxId, TaxAmount, NettPrice, Total, DPP, Notes, CoaInventory, CoaCOGS, CoaPurc, CoaPurcDisc, CoaPurcReturn, Type)
		SELECT Id, Code, [LineNo], ItemId, UomId, UnitId, Qty, Length, Width, Height, Weight, DimensionMeasurement, WeightMeasurement, QtyRcv, UnitPrice, Disc, FinalDiscHeader, TaxId, TaxAmount, NettPrice, Total, DPP, Notes, CoaInventory, CoaCOGS, CoaPurc, CoaPurcDisc, CoaPurcReturn, Type FROM Purchasing.PurchaseOrderDetail WITH (HOLDLOCK TABLOCKX)')
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

			// Refresh view Sales.vwSalesOrderDetail
			sql = @"exec sp_refreshview 'Sales.vwSalesOrderDetail'";
            migrationBuilder.Sql(sql);

			// Refresh view Sales.vwSalesDeliveryDetail
			sql = @"exec sp_refreshview 'Sales.vwSalesDeliveryDetail'";
            migrationBuilder.Sql(sql);

			// Refresh view Purchasing.vwPurchaseReceiveDetail
			sql = @"exec sp_refreshview 'Purchasing.vwPurchaseReceiveDetail'";
            migrationBuilder.Sql(sql);

			// Refresh view Purchasing.vwPurchaseOrderDetail
			sql = @"exec sp_refreshview 'Purchasing.vwPurchaseOrderDetail'";
            migrationBuilder.Sql(sql);

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FinalDiscHeader",
                schema: "Sales",
                table: "SalesOrderDetail");

            migrationBuilder.DropColumn(
                name: "FinalDiscHeader",
                schema: "Sales",
                table: "SalesDeliveryDetail");

            migrationBuilder.DropColumn(
                name: "FinalDiscHeader",
                schema: "Purchasing",
                table: "PurchaseReceiveDetail");

            migrationBuilder.DropColumn(
                name: "FinalDiscHeader",
                schema: "Purchasing",
                table: "PurchaseOrderDetail");
        }
    }
}
