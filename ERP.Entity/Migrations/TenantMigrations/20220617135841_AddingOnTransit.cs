using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AddingOnTransit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "QtyOnTransit",
                schema: "Inventory",
                table: "WarehouseQuantity",
                type: "decimal(19,6)",
                precision: 19,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "CoaTransit",
                schema: "Inventory",
                table: "Item",
                type: "varchar(6)",
                unicode: false,
                maxLength: 6,
                nullable: true);

            // Reorder column Inventory.Item
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
ALTER TABLE Inventory.Item
	DROP CONSTRAINT FK_Item_UoMConversion_UomBuyId
GO
ALTER TABLE Inventory.Item
	DROP CONSTRAINT FK_Item_UoMConversion_UomSellId
GO
ALTER TABLE Inventory.UoMConversion SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.Item
	DROP CONSTRAINT FK_Item_UoM_UomId
GO
ALTER TABLE Inventory.UoM SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.Item
	DROP CONSTRAINT FK_Item_ItemCategory_CategoryId
GO
ALTER TABLE Inventory.ItemCategory SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.Item
	DROP CONSTRAINT FK_Item_Tax_PurchaseTaxId
GO
ALTER TABLE Inventory.Item
	DROP CONSTRAINT FK_Item_Tax_SalesTaxId
GO
ALTER TABLE General.Tax SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE Inventory.Tmp_Item
	(
	Id int NOT NULL IDENTITY (1, 1),
	Initial varchar(20) NOT NULL,
	Name varchar(50) NOT NULL,
	Description varchar(100) NULL,
	CategoryId int NOT NULL,
	CostOfGoodSold decimal(19, 6) NULL,
	UomId int NULL,
	UomSellId int NULL,
	SellPrice decimal(18, 2) NULL,
	UomBuyId int NULL,
	BuyPrice decimal(18, 2) NULL,
	SalesTaxId int NULL,
	PurchaseTaxId int NULL,
	SubGroup1 varchar(50) NULL,
	SubGroup2 varchar(50) NULL,
	SubGroup3 varchar(50) NULL,
	SubGroup4 varchar(50) NULL,
	SubGroup5 varchar(50) NULL,
	CoaTransit varchar(6) NULL,
	CoaInventory varchar(6) NULL,
	CoaCOGS varchar(6) NULL,
	CoaPurc varchar(6) NULL,
	CoaPurcDisc varchar(6) NULL,
	CoaPurcReturn varchar(6) NULL,
	CoaSls varchar(6) NULL,
	CoaSlsReturn varchar(6) NULL,
	CoaSlsDisc varchar(6) NULL,
	CoaOffSet varchar(6) NULL,
	CoaCost varchar(6) NULL,
	CoaExpense varchar(6) NULL,
	Length decimal(18, 2) NULL,
	Width decimal(18, 2) NULL,
	Height decimal(18, 2) NULL,
	DimensionMeasurement varchar(10) NULL,
	Weight decimal(18, 3) NULL,
	WeightMeasurement varchar(10) NULL,
	IsActive bit NOT NULL,
	CreatedBy int NOT NULL,
	CreatedDate datetime NOT NULL,
	UpdatedBy int NOT NULL,
	UpdatedDate datetime NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Inventory.Tmp_Item SET (LOCK_ESCALATION = TABLE)
GO
SET IDENTITY_INSERT Inventory.Tmp_Item ON
GO
IF EXISTS(SELECT * FROM Inventory.Item)
	 EXEC('INSERT INTO Inventory.Tmp_Item (Id, Initial, Name, Description, CategoryId, CostOfGoodSold, UomId, UomSellId, SellPrice, UomBuyId, BuyPrice, SalesTaxId, PurchaseTaxId, SubGroup1, SubGroup2, SubGroup3, SubGroup4, SubGroup5, CoaTransit, CoaInventory, CoaCOGS, CoaPurc, CoaPurcDisc, CoaPurcReturn, CoaSls, CoaSlsReturn, CoaSlsDisc, CoaOffSet, CoaCost, CoaExpense, Length, Width, Height, DimensionMeasurement, Weight, WeightMeasurement, IsActive, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate)
		SELECT Id, Initial, Name, Description, CategoryId, CostOfGoodSold, UomId, UomSellId, SellPrice, UomBuyId, BuyPrice, SalesTaxId, PurchaseTaxId, SubGroup1, SubGroup2, SubGroup3, SubGroup4, SubGroup5, CoaTransit, CoaInventory, CoaCOGS, CoaPurc, CoaPurcDisc, CoaPurcReturn, CoaSls, CoaSlsReturn, CoaSlsDisc, CoaOffSet, CoaCost, CoaExpense, Length, Width, Height, DimensionMeasurement, Weight, WeightMeasurement, IsActive, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate FROM Inventory.Item WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT Inventory.Tmp_Item OFF
GO
ALTER TABLE Inventory.AdjustmentDetail
	DROP CONSTRAINT FK_AdjustmentDetail_Item_ItemId
GO
ALTER TABLE MobileWarehouse.MobileReceiveItemDetail
	DROP CONSTRAINT FK_MobileReceiveItemDetail_Item_ItemId
GO
ALTER TABLE Inventory.BeginningBalanceDetail
	DROP CONSTRAINT FK_BeginningBalanceDetail_Item_ItemId
GO
ALTER TABLE MobileSales.MobileItemRequestDetail
	DROP CONSTRAINT FK_MobileItemRequestDetail_Item_ItemId
GO
ALTER TABLE MobileWarehouse.MobileTransferStockDetail
	DROP CONSTRAINT FK_MobileTransferStockDetail_Item_ItemId
GO
ALTER TABLE MobileCustomer.MobileOrderDetail
	DROP CONSTRAINT FK_MobileOrderDetail_Item_ItemId
GO
ALTER TABLE Inventory.StockMutation
	DROP CONSTRAINT FK_StockMutation_Item_ItemId
GO
ALTER TABLE Inventory.WarehouseQuantity
	DROP CONSTRAINT FK_WarehouseQuantity_Item_ItemId
GO
ALTER TABLE MobileCustomer.MobileOrderDetailFreeGood
	DROP CONSTRAINT FK_MobileOrderDetailFreeGood_Item_ItemId
GO
ALTER TABLE MobileSales.MobileOrderDetail
	DROP CONSTRAINT FK_MobileOrderDetail_Item_ItemId
GO
ALTER TABLE MobileSales.MobileOrderDetailFreeGood
	DROP CONSTRAINT FK_MobileOrderDetailFreeGood_Item_ItemId
GO
ALTER TABLE Sales.DeliveryPlanDetailItem
	DROP CONSTRAINT FK_DeliveryPlanDetailItem_Item_ItemId
GO
ALTER TABLE Sales.DeliveryPlanUndeliveredItem
	DROP CONSTRAINT FK_DeliveryPlanUndeliveredItem_Item_ItemId
GO
ALTER TABLE Inventory.TransferStockDetail
	DROP CONSTRAINT FK_TransferStockDetail_Item_ItemId
GO
ALTER TABLE Sales.PromoDetailMultipleItem
	DROP CONSTRAINT FK_PromoDetailMultipleItem_Item_ItemId
GO
ALTER TABLE MobileWarehouse.MobileDeliveryItemDetail
	DROP CONSTRAINT FK_MobileDeliveryItemDetail_Item_ItemId
GO
DROP TABLE Inventory.Item
GO
EXECUTE sp_rename N'Inventory.Tmp_Item', N'Item', 'OBJECT' 
GO
ALTER TABLE Inventory.Item ADD CONSTRAINT
	PK_Item PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX IX_Item_CategoryId ON Inventory.Item
	(
	CategoryId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_Item_PurchaseTaxId ON Inventory.Item
	(
	PurchaseTaxId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_Item_SalesTaxId ON Inventory.Item
	(
	SalesTaxId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_Item_UomBuyId ON Inventory.Item
	(
	UomBuyId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_Item_UomId ON Inventory.Item
	(
	UomId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_Item_UomSellId ON Inventory.Item
	(
	UomSellId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
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
ALTER TABLE Inventory.Item ADD CONSTRAINT
	FK_Item_ItemCategory_CategoryId FOREIGN KEY
	(
	CategoryId
	) REFERENCES Inventory.ItemCategory
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.Item ADD CONSTRAINT
	FK_Item_UoM_UomId FOREIGN KEY
	(
	UomId
	) REFERENCES Inventory.UoM
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.Item ADD CONSTRAINT
	FK_Item_UoMConversion_UomBuyId FOREIGN KEY
	(
	UomBuyId
	) REFERENCES Inventory.UoMConversion
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.Item ADD CONSTRAINT
	FK_Item_UoMConversion_UomSellId FOREIGN KEY
	(
	UomSellId
	) REFERENCES Inventory.UoMConversion
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
COMMIT
BEGIN TRANSACTION
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
ALTER TABLE MobileWarehouse.MobileDeliveryItemDetail SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Sales.PromoDetailMultipleItem ADD CONSTRAINT
	FK_PromoDetailMultipleItem_Item_ItemId FOREIGN KEY
	(
	ItemId
	) REFERENCES Inventory.Item
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Sales.PromoDetailMultipleItem SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.TransferStockDetail ADD CONSTRAINT
	FK_TransferStockDetail_Item_ItemId FOREIGN KEY
	(
	ItemId
	) REFERENCES Inventory.Item
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.TransferStockDetail SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Sales.DeliveryPlanUndeliveredItem ADD CONSTRAINT
	FK_DeliveryPlanUndeliveredItem_Item_ItemId FOREIGN KEY
	(
	ItemId
	) REFERENCES Inventory.Item
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Sales.DeliveryPlanUndeliveredItem SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Sales.DeliveryPlanDetailItem ADD CONSTRAINT
	FK_DeliveryPlanDetailItem_Item_ItemId FOREIGN KEY
	(
	ItemId
	) REFERENCES Inventory.Item
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Sales.DeliveryPlanDetailItem SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE MobileSales.MobileOrderDetailFreeGood ADD CONSTRAINT
	FK_MobileOrderDetailFreeGood_Item_ItemId FOREIGN KEY
	(
	ItemId
	) REFERENCES Inventory.Item
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE MobileSales.MobileOrderDetailFreeGood SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE MobileSales.MobileOrderDetail ADD CONSTRAINT
	FK_MobileOrderDetail_Item_ItemId FOREIGN KEY
	(
	ItemId
	) REFERENCES Inventory.Item
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
ALTER TABLE MobileCustomer.MobileOrderDetailFreeGood ADD CONSTRAINT
	FK_MobileOrderDetailFreeGood_Item_ItemId FOREIGN KEY
	(
	ItemId
	) REFERENCES Inventory.Item
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE MobileCustomer.MobileOrderDetailFreeGood SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.WarehouseQuantity ADD CONSTRAINT
	FK_WarehouseQuantity_Item_ItemId FOREIGN KEY
	(
	ItemId
	) REFERENCES Inventory.Item
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.WarehouseQuantity SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.StockMutation ADD CONSTRAINT
	FK_StockMutation_Item_ItemId FOREIGN KEY
	(
	ItemId
	) REFERENCES Inventory.Item
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.StockMutation SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE MobileCustomer.MobileOrderDetail ADD CONSTRAINT
	FK_MobileOrderDetail_Item_ItemId FOREIGN KEY
	(
	ItemId
	) REFERENCES Inventory.Item
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE MobileCustomer.MobileOrderDetail SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE MobileWarehouse.MobileTransferStockDetail ADD CONSTRAINT
	FK_MobileTransferStockDetail_Item_ItemId FOREIGN KEY
	(
	ItemId
	) REFERENCES Inventory.Item
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE MobileWarehouse.MobileTransferStockDetail SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE MobileSales.MobileItemRequestDetail ADD CONSTRAINT
	FK_MobileItemRequestDetail_Item_ItemId FOREIGN KEY
	(
	ItemId
	) REFERENCES Inventory.Item
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE MobileSales.MobileItemRequestDetail SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.BeginningBalanceDetail ADD CONSTRAINT
	FK_BeginningBalanceDetail_Item_ItemId FOREIGN KEY
	(
	ItemId
	) REFERENCES Inventory.Item
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.BeginningBalanceDetail SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE MobileWarehouse.MobileReceiveItemDetail ADD CONSTRAINT
	FK_MobileReceiveItemDetail_Item_ItemId FOREIGN KEY
	(
	ItemId
	) REFERENCES Inventory.Item
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE MobileWarehouse.MobileReceiveItemDetail SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.AdjustmentDetail ADD CONSTRAINT
	FK_AdjustmentDetail_Item_ItemId FOREIGN KEY
	(
	ItemId
	) REFERENCES Inventory.Item
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.AdjustmentDetail SET (LOCK_ESCALATION = TABLE)
GO
COMMIT";
            migrationBuilder.Sql(sql);

			// Reorder column Inventory.WarehouseQuantity
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
ALTER TABLE Inventory.WarehouseQuantity
	DROP CONSTRAINT FK_WarehouseQuantity_Warehouse_WarehouseCode
GO
ALTER TABLE Inventory.Warehouse SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.WarehouseQuantity
	DROP CONSTRAINT FK_WarehouseQuantity_Item_ItemId
GO
ALTER TABLE Inventory.Item SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE Inventory.Tmp_WarehouseQuantity
	(
	Id bigint NOT NULL IDENTITY (1, 1),
	WarehouseCode varchar(8) NOT NULL,
	ItemId int NOT NULL,
	QtyOnHand decimal(19, 6) NOT NULL,
	QtyOnOrder decimal(19, 6) NOT NULL,
	QtyOnIndent decimal(19, 6) NOT NULL,
	QtyReorderPoint decimal(19, 6) NOT NULL,
	QtyOnTransfer decimal(19, 6) NOT NULL,
	QtyOnTransit decimal(19, 6) NOT NULL,
	UpdatedDate datetime NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Inventory.Tmp_WarehouseQuantity SET (LOCK_ESCALATION = TABLE)
GO
SET IDENTITY_INSERT Inventory.Tmp_WarehouseQuantity ON
GO
IF EXISTS(SELECT * FROM Inventory.WarehouseQuantity)
	 EXEC('INSERT INTO Inventory.Tmp_WarehouseQuantity (Id, WarehouseCode, ItemId, QtyOnHand, QtyOnOrder, QtyOnIndent, QtyReorderPoint, QtyOnTransfer, QtyOnTransit, UpdatedDate)
		SELECT Id, WarehouseCode, ItemId, QtyOnHand, QtyOnOrder, QtyOnIndent, QtyReorderPoint, QtyOnTransfer, QtyOnTransit, UpdatedDate FROM Inventory.WarehouseQuantity WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT Inventory.Tmp_WarehouseQuantity OFF
GO
DROP TABLE Inventory.WarehouseQuantity
GO
EXECUTE sp_rename N'Inventory.Tmp_WarehouseQuantity', N'WarehouseQuantity', 'OBJECT' 
GO
ALTER TABLE Inventory.WarehouseQuantity ADD CONSTRAINT
	PK_WarehouseQuantity PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX IX_WarehouseQuantity_ItemId ON Inventory.WarehouseQuantity
	(
	ItemId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_WarehouseQuantity_WarehouseCode ON Inventory.WarehouseQuantity
	(
	WarehouseCode
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE Inventory.WarehouseQuantity ADD CONSTRAINT
	FK_WarehouseQuantity_Item_ItemId FOREIGN KEY
	(
	ItemId
	) REFERENCES Inventory.Item
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.WarehouseQuantity ADD CONSTRAINT
	FK_WarehouseQuantity_Warehouse_WarehouseCode FOREIGN KEY
	(
	WarehouseCode
	) REFERENCES Inventory.Warehouse
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
COMMIT";
            migrationBuilder.Sql(sql);

            // Refresh view Inventory.vwItem
            sql = @"EXEC sp_refreshview 'Inventory.vwItem'";
            migrationBuilder.Sql(sql);

			// Refresh view Inventory.vwWarehouseQuantity
			sql = @"EXEC sp_refreshview 'Inventory.vwWarehouseQuantity'";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QtyOnTransit",
                schema: "Inventory",
                table: "WarehouseQuantity");

            migrationBuilder.DropColumn(
                name: "CoaTransit",
                schema: "Inventory",
                table: "Item");
        }
    }
}
