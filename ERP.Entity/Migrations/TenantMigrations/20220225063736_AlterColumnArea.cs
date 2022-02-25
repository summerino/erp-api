using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AlterColumnArea : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Initial",
                schema: "Inventory",
                table: "UoM",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldUnicode: false,
                oldMaxLength: 20);

            // Refresh view Sales.vwArea
            var sql = @"exec sp_refreshview 'Sales.vwArea'";
            migrationBuilder.Sql(sql);

            // Reorder column Inventory.Warehouse
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
ALTER TABLE Inventory.Warehouse
	DROP CONSTRAINT FK_Warehouse_Customer_CustCode
GO
ALTER TABLE General.Customer SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE Inventory.Tmp_Warehouse
	(
	Code varchar(8) NOT NULL,
	Initial varchar(20) NOT NULL,
	Name varchar(50) NOT NULL,
	Address varchar(100) NULL,
	Phone varchar(30) NULL,
	IsDefault bit NOT NULL,
	CustCode varchar(8) NULL,
	IsActive bit NOT NULL,
	CreatedBy int NOT NULL,
	CreatedDate datetime NOT NULL,
	UpdatedBy int NOT NULL,
	UpdatedDate datetime NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Inventory.Tmp_Warehouse SET (LOCK_ESCALATION = TABLE)
GO
IF EXISTS(SELECT * FROM Inventory.Warehouse)
	 EXEC('INSERT INTO Inventory.Tmp_Warehouse (Code, Initial, Name, Address, Phone, IsDefault, CustCode, IsActive, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate)
		SELECT Code, Initial, Name, Address, Phone, IsDefault, CustCode, IsActive, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate FROM Inventory.Warehouse WITH (HOLDLOCK TABLOCKX)')
GO
ALTER TABLE Inventory.StockMutation
	DROP CONSTRAINT FK_StockMutation_Warehouse_WarehouseCode
GO
ALTER TABLE General.Employee
	DROP CONSTRAINT FK_Employee_Warehouse_WarehouseCode
GO
ALTER TABLE Inventory.WarehouseQuantity
	DROP CONSTRAINT FK_WarehouseQuantity_Warehouse_WarehouseCode
GO
ALTER TABLE Inventory.TransferStockHeader
	DROP CONSTRAINT FK_TransferStockHeader_Warehouse_WarehouseCodeFrom
GO
ALTER TABLE Inventory.TransferStockHeader
	DROP CONSTRAINT FK_TransferStockHeader_Warehouse_WarehouseCodeTo
GO
ALTER TABLE Sales.DeliveryPlanUndeliveredItem
	DROP CONSTRAINT FK_DeliveryPlanUndeliveredItem_Warehouse_WarehouseCode
GO
ALTER TABLE Sales.DeliveryPlanHeader
	DROP CONSTRAINT FK_DeliveryPlanHeader_Warehouse_WarehouseCode
GO
ALTER TABLE Inventory.AdjustmentHeader
	DROP CONSTRAINT FK_AdjustmentHeader_Warehouse_WarehouseCode
GO
ALTER TABLE MobileWarehouse.MobileReceiveItemDetail
	DROP CONSTRAINT FK_MobileReceiveItemDetail_Warehouse_WarehouseCode
GO
ALTER TABLE Inventory.BeginningBalanceHeader
	DROP CONSTRAINT FK_BeginningBalanceHeader_Warehouse_WarehouseCode
GO
DROP TABLE Inventory.Warehouse
GO
EXECUTE sp_rename N'Inventory.Tmp_Warehouse', N'Warehouse', 'OBJECT' 
GO
ALTER TABLE Inventory.Warehouse ADD CONSTRAINT
	PK_Warehouse PRIMARY KEY CLUSTERED 
	(
	Code
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX IX_Warehouse_CustCode ON Inventory.Warehouse
	(
	CustCode
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE Inventory.Warehouse ADD CONSTRAINT
	FK_Warehouse_Customer_CustCode FOREIGN KEY
	(
	CustCode
	) REFERENCES General.Customer
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.BeginningBalanceHeader ADD CONSTRAINT
	FK_BeginningBalanceHeader_Warehouse_WarehouseCode FOREIGN KEY
	(
	WarehouseCode
	) REFERENCES Inventory.Warehouse
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.BeginningBalanceHeader SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE MobileWarehouse.MobileReceiveItemDetail ADD CONSTRAINT
	FK_MobileReceiveItemDetail_Warehouse_WarehouseCode FOREIGN KEY
	(
	WarehouseCode
	) REFERENCES Inventory.Warehouse
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE MobileWarehouse.MobileReceiveItemDetail SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.AdjustmentHeader ADD CONSTRAINT
	FK_AdjustmentHeader_Warehouse_WarehouseCode FOREIGN KEY
	(
	WarehouseCode
	) REFERENCES Inventory.Warehouse
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.AdjustmentHeader SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Sales.DeliveryPlanHeader ADD CONSTRAINT
	FK_DeliveryPlanHeader_Warehouse_WarehouseCode FOREIGN KEY
	(
	WarehouseCode
	) REFERENCES Inventory.Warehouse
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Sales.DeliveryPlanHeader SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Sales.DeliveryPlanUndeliveredItem ADD CONSTRAINT
	FK_DeliveryPlanUndeliveredItem_Warehouse_WarehouseCode FOREIGN KEY
	(
	WarehouseCode
	) REFERENCES Inventory.Warehouse
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Sales.DeliveryPlanUndeliveredItem SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.TransferStockHeader ADD CONSTRAINT
	FK_TransferStockHeader_Warehouse_WarehouseCodeFrom FOREIGN KEY
	(
	WarehouseCodeFrom
	) REFERENCES Inventory.Warehouse
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.TransferStockHeader ADD CONSTRAINT
	FK_TransferStockHeader_Warehouse_WarehouseCodeTo FOREIGN KEY
	(
	WarehouseCodeTo
	) REFERENCES Inventory.Warehouse
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.TransferStockHeader SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
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
ALTER TABLE Inventory.WarehouseQuantity SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE General.Employee ADD CONSTRAINT
	FK_Employee_Warehouse_WarehouseCode FOREIGN KEY
	(
	WarehouseCode
	) REFERENCES Inventory.Warehouse
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE General.Employee SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.StockMutation ADD CONSTRAINT
	FK_StockMutation_Warehouse_WarehouseCode FOREIGN KEY
	(
	WarehouseCode
	) REFERENCES Inventory.Warehouse
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.StockMutation SET (LOCK_ESCALATION = TABLE)
GO
COMMIT";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Initial",
                schema: "Inventory",
                table: "UoM",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldUnicode: false,
                oldMaxLength: 50);
        }
    }
}
