using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AddingOnTransitInSalesOrder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CoaTransit",
                schema: "Sales",
                table: "SalesOrderDetail",
                type: "varchar(6)",
                unicode: false,
                maxLength: 6,
                nullable: true);

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
	ExemptTaxAmount decimal(19, 6) NOT NULL,
	NettPrice decimal(19, 6) NOT NULL,
	Total decimal(19, 6) NOT NULL,
	DPP decimal(19, 6) NOT NULL,
	Notes varchar(256) NULL,
	CoaTransit varchar(6) NULL,
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
	 EXEC('INSERT INTO Sales.Tmp_SalesOrderDetail (Id, Code, [LineNo], ItemId, UomId, UnitId, Qty, Length, Width, Height, Weight, DimensionMeasurement, WeightMeasurement, QtyDlv, UnitPrice, Disc, FinalDiscHeader, TaxId, TaxAmount, ExemptTaxAmount, NettPrice, Total, DPP, Notes, CoaTransit, CoaInventory, CoaCOGS, CoaSls, CoaSlsDisc, CoaSlsReturn)
		SELECT Id, Code, [LineNo], ItemId, UomId, UnitId, Qty, Length, Width, Height, Weight, DimensionMeasurement, WeightMeasurement, QtyDlv, UnitPrice, Disc, FinalDiscHeader, TaxId, TaxAmount, ExemptTaxAmount, NettPrice, Total, DPP, Notes, CoaTransit, CoaInventory, CoaCOGS, CoaSls, CoaSlsDisc, CoaSlsReturn FROM Sales.SalesOrderDetail WITH (HOLDLOCK TABLOCKX)')
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

			// Refresh view Sales.vwSalesOrderDetail
			sql = @"EXEC sp_refreshview 'Sales.vwSalesOrderDetail'";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CoaTransit",
                schema: "Sales",
                table: "SalesOrderDetail");
        }
    }
}
