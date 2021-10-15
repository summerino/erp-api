using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class CreateTableMobileTransferStock : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Qty",
                schema: "MobileWarehouse",
                table: "MobileDeliveryItemDetail",
                newName: "RealizeQty");

            migrationBuilder.AddColumn<decimal>(
                name: "OriginalQty",
                schema: "MobileWarehouse",
                table: "MobileDeliveryItemDetail",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "MobileTransferStockHeader",
                schema: "MobileWarehouse",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    TransferCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    Mark = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ApprovedBy = table.Column<int>(type: "int", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    RejectedBy = table.Column<int>(type: "int", nullable: true),
                    RejectedDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MobileTransferStockHeader", x => x.Code);
                    table.ForeignKey(
                        name: "FK_MobileTransferStockHeader_TransferStockHeader_TransferCode",
                        column: x => x.TransferCode,
                        principalSchema: "Inventory",
                        principalTable: "TransferStockHeader",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "MobileTransferStockDetail",
                schema: "MobileWarehouse",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    OriginalQty = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    RealizeQty = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MobileTransferStockDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MobileTransferStockDetail_Item_ItemId",
                        column: x => x.ItemId,
                        principalSchema: "Inventory",
                        principalTable: "Item",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileTransferStockDetail_MobileTransferStockHeader_Code",
                        column: x => x.Code,
                        principalSchema: "MobileWarehouse",
                        principalTable: "MobileTransferStockHeader",
                        principalColumn: "Code");
                    table.ForeignKey(
                        name: "FK_MobileTransferStockDetail_UoM_UomId",
                        column: x => x.UomId,
                        principalSchema: "Inventory",
                        principalTable: "UoM",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileTransferStockDetail_UoMConversion_UnitId",
                        column: x => x.UnitId,
                        principalSchema: "Inventory",
                        principalTable: "UoMConversion",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MobileTransferStockDetail_Code",
                schema: "MobileWarehouse",
                table: "MobileTransferStockDetail",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_MobileTransferStockDetail_ItemId",
                schema: "MobileWarehouse",
                table: "MobileTransferStockDetail",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_MobileTransferStockDetail_UnitId",
                schema: "MobileWarehouse",
                table: "MobileTransferStockDetail",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_MobileTransferStockDetail_UomId",
                schema: "MobileWarehouse",
                table: "MobileTransferStockDetail",
                column: "UomId");

            migrationBuilder.CreateIndex(
                name: "IX_MobileTransferStockHeader_TransferCode",
                schema: "MobileWarehouse",
                table: "MobileTransferStockHeader",
                column: "TransferCode");

            // Reorder column MobileWarehouse.MobileDeliveryItemDetail
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
ALTER TABLE MobileWarehouse.MobileDeliveryItemDetail
	DROP CONSTRAINT FK_MobileDeliveryItemDetail_UoMConversion_UnitId
GO
ALTER TABLE Inventory.UoMConversion SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE MobileWarehouse.MobileDeliveryItemDetail
	DROP CONSTRAINT FK_MobileDeliveryItemDetail_UoM_UomId
GO
ALTER TABLE Inventory.UoM SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE MobileWarehouse.MobileDeliveryItemDetail
	DROP CONSTRAINT FK_MobileDeliveryItemDetail_MobileDeliveryItemHeader_Code
GO
ALTER TABLE MobileWarehouse.MobileDeliveryItemHeader SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE MobileWarehouse.MobileDeliveryItemDetail
	DROP CONSTRAINT FK_MobileDeliveryItemDetail_Item_ItemId
GO
ALTER TABLE Inventory.Item SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE MobileWarehouse.Tmp_MobileDeliveryItemDetail
	(
	Id bigint NOT NULL IDENTITY (1, 1),
	Code varchar(17) NOT NULL,
	[LineNo] smallint NOT NULL,
	ItemId int NOT NULL,
	OriginalQty decimal(18, 2) NOT NULL,
	RealizeQty decimal(18, 2) NOT NULL,
	UomId int NOT NULL,
	UnitId int NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE MobileWarehouse.Tmp_MobileDeliveryItemDetail SET (LOCK_ESCALATION = TABLE)
GO
SET IDENTITY_INSERT MobileWarehouse.Tmp_MobileDeliveryItemDetail ON
GO
IF EXISTS(SELECT * FROM MobileWarehouse.MobileDeliveryItemDetail)
	 EXEC('INSERT INTO MobileWarehouse.Tmp_MobileDeliveryItemDetail (Id, Code, [LineNo], ItemId, OriginalQty, RealizeQty, UomId, UnitId)
		SELECT Id, Code, [LineNo], ItemId, OriginalQty, RealizeQty, UomId, UnitId FROM MobileWarehouse.MobileDeliveryItemDetail WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT MobileWarehouse.Tmp_MobileDeliveryItemDetail OFF
GO
DROP TABLE MobileWarehouse.MobileDeliveryItemDetail
GO
EXECUTE sp_rename N'MobileWarehouse.Tmp_MobileDeliveryItemDetail', N'MobileDeliveryItemDetail', 'OBJECT' 
GO
ALTER TABLE MobileWarehouse.MobileDeliveryItemDetail ADD CONSTRAINT
	PK_MobileDeliveryItemDetail PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX IX_MobileDeliveryItemDetail_Code ON MobileWarehouse.MobileDeliveryItemDetail
	(
	Code
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_MobileDeliveryItemDetail_ItemId ON MobileWarehouse.MobileDeliveryItemDetail
	(
	ItemId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_MobileDeliveryItemDetail_UnitId ON MobileWarehouse.MobileDeliveryItemDetail
	(
	UnitId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_MobileDeliveryItemDetail_UomId ON MobileWarehouse.MobileDeliveryItemDetail
	(
	UomId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE MobileWarehouse.MobileDeliveryItemDetail ADD CONSTRAINT
	FK_MobileDeliveryItemDetail_Item_ItemId FOREIGN KEY
	(
	ItemId
	) REFERENCES Inventory.Item
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE MobileWarehouse.MobileDeliveryItemDetail ADD CONSTRAINT
	FK_MobileDeliveryItemDetail_MobileDeliveryItemHeader_Code FOREIGN KEY
	(
	Code
	) REFERENCES MobileWarehouse.MobileDeliveryItemHeader
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE MobileWarehouse.MobileDeliveryItemDetail ADD CONSTRAINT
	FK_MobileDeliveryItemDetail_UoM_UomId FOREIGN KEY
	(
	UomId
	) REFERENCES Inventory.UoM
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE MobileWarehouse.MobileDeliveryItemDetail ADD CONSTRAINT
	FK_MobileDeliveryItemDetail_UoMConversion_UnitId FOREIGN KEY
	(
	UnitId
	) REFERENCES Inventory.UoMConversion
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
COMMIT";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MobileTransferStockDetail",
                schema: "MobileWarehouse");

            migrationBuilder.DropTable(
                name: "MobileTransferStockHeader",
                schema: "MobileWarehouse");

            migrationBuilder.DropColumn(
                name: "OriginalQty",
                schema: "MobileWarehouse",
                table: "MobileDeliveryItemDetail");

            migrationBuilder.RenameColumn(
                name: "RealizeQty",
                schema: "MobileWarehouse",
                table: "MobileDeliveryItemDetail",
                newName: "Qty");
        }
    }
}
