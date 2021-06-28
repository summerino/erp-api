using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP_API.Migrations.TenantMigrations
{
    public partial class AddingConsigneeInCustomerTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RcvStatus",
                schema: "Purchasing",
                table: "PurchaseOrderHeader");

            migrationBuilder.AddColumn<string>(
                name: "CustCode",
                schema: "Inventory",
                table: "Warehouse",
                type: "varchar(8)",
                unicode: false,
                maxLength: 8,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsConsignee",
                schema: "General",
                table: "Customer",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Warehouse_CustCode",
                schema: "Inventory",
                table: "Warehouse",
                column: "CustCode");

            migrationBuilder.AddForeignKey(
                name: "FK_Warehouse_Customer_CustCode",
                schema: "Inventory",
                table: "Warehouse",
                column: "CustCode",
                principalSchema: "General",
                principalTable: "Customer",
                principalColumn: "Code");

            // Create view Expedition.vwExpeditionInvoiceHeader
            var sql = @"CREATE VIEW [Expedition].[vwExpeditionInvoiceHeader]
AS
	SELECT e.*,
		s.Initial AS SupplierInitial,
		c.Initial AS CreatedInitial,
		u.Initial AS UpdatedInitial,
		a.Initial AS ApprovedInitial
	FROM Expedition.ExpeditionInvoiceHeader e
	LEFT JOIN General.Supplier s
		ON e.SupCode = s.Code
	LEFT JOIN SystemManagement.[User] c
		ON c.Id = e.CreatedBy
	LEFT JOIN SystemManagement.[User] u
		ON u.Id = e.UpdatedBy
	LEFT JOIN SystemManagement.[User] a
		ON a.Id = e.ApprovedBy";
            migrationBuilder.Sql(sql);

            // Reorder column in table General.Customer
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
ALTER TABLE General.Customer
	DROP CONSTRAINT FK_Customer_PaymentTerm_PaymentTermId
GO
ALTER TABLE General.PaymentTerm SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE General.Customer
	DROP CONSTRAINT FK_Customer_CustomerType_TypeId
GO
ALTER TABLE General.CustomerType SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE General.Customer
	DROP CONSTRAINT FK_Customer_Area_AreaId1
GO
ALTER TABLE General.Customer
	DROP CONSTRAINT FK_Customer_Area_AreaId2
GO
ALTER TABLE General.Customer
	DROP CONSTRAINT FK_Customer_Area_AreaId3
GO
ALTER TABLE General.Customer
	DROP CONSTRAINT FK_Customer_Area_AreaId4
GO
ALTER TABLE General.Customer
	DROP CONSTRAINT FK_Customer_Area_AreaId5
GO
ALTER TABLE Sales.Area SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE General.Tmp_Customer
	(
	Code varchar(8) NOT NULL,
	Initial varchar(20) NOT NULL,
	Name varchar(50) NOT NULL,
	TypeId int NOT NULL,
	Email varchar(50) NULL,
	Website varchar(50) NULL,
	PaymentTermId int NOT NULL,
	CreditLimit decimal(18, 2) NOT NULL,
	RefNo varchar(30) NULL,
	Notes varchar(256) NULL,
	BillingAddressId int NULL,
	ShippingAddressId int NULL,
	AreaId1 int NULL,
	AreaId2 int NULL,
	AreaId3 int NULL,
	AreaId4 int NULL,
	AreaId5 int NULL,
	IsConsignee bit NOT NULL,
	IsActive bit NOT NULL,
	CreatedBy int NOT NULL,
	CreatedDate datetime NOT NULL,
	UpdatedBy int NOT NULL,
	UpdatedDate datetime NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE General.Tmp_Customer SET (LOCK_ESCALATION = TABLE)
GO
IF EXISTS(SELECT * FROM General.Customer)
	 EXEC('INSERT INTO General.Tmp_Customer (Code, Initial, Name, TypeId, Email, Website, PaymentTermId, CreditLimit, RefNo, Notes, BillingAddressId, ShippingAddressId, AreaId1, AreaId2, AreaId3, AreaId4, AreaId5, IsConsignee, IsActive, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate)
		SELECT Code, Initial, Name, TypeId, Email, Website, PaymentTermId, CreditLimit, RefNo, Notes, BillingAddressId, ShippingAddressId, AreaId1, AreaId2, AreaId3, AreaId4, AreaId5, IsConsignee, IsActive, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate FROM General.Customer WITH (HOLDLOCK TABLOCKX)')
GO
ALTER TABLE General.Customer
	DROP CONSTRAINT FK_Customer_CustomerAddress_ShippingAddressId
GO
ALTER TABLE Inventory.Warehouse
	DROP CONSTRAINT FK_Warehouse_Customer_CustCode
GO
ALTER TABLE General.CustomerAddress
	DROP CONSTRAINT FK_CustomerAddress_Customer_Code
GO
ALTER TABLE General.Customer
	DROP CONSTRAINT FK_Customer_CustomerAddress_BillingAddressId
GO
DROP TABLE General.Customer
GO
EXECUTE sp_rename N'General.Tmp_Customer', N'Customer', 'OBJECT' 
GO
ALTER TABLE General.Customer ADD CONSTRAINT
	PK_Customer PRIMARY KEY CLUSTERED 
	(
	Code
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX IX_Customer_AreaId1 ON General.Customer
	(
	AreaId1
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_Customer_AreaId2 ON General.Customer
	(
	AreaId2
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_Customer_AreaId3 ON General.Customer
	(
	AreaId3
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_Customer_AreaId4 ON General.Customer
	(
	AreaId4
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_Customer_AreaId5 ON General.Customer
	(
	AreaId5
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_Customer_BillingAddressId ON General.Customer
	(
	BillingAddressId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_Customer_ShippingAddressId ON General.Customer
	(
	ShippingAddressId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_Customer_TypeId ON General.Customer
	(
	TypeId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_Customer_PaymentTermId ON General.Customer
	(
	PaymentTermId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE General.Customer ADD CONSTRAINT
	FK_Customer_Area_AreaId1 FOREIGN KEY
	(
	AreaId1
	) REFERENCES Sales.Area
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE General.Customer ADD CONSTRAINT
	FK_Customer_Area_AreaId2 FOREIGN KEY
	(
	AreaId2
	) REFERENCES Sales.Area
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE General.Customer ADD CONSTRAINT
	FK_Customer_Area_AreaId3 FOREIGN KEY
	(
	AreaId3
	) REFERENCES Sales.Area
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE General.Customer ADD CONSTRAINT
	FK_Customer_Area_AreaId4 FOREIGN KEY
	(
	AreaId4
	) REFERENCES Sales.Area
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE General.Customer ADD CONSTRAINT
	FK_Customer_Area_AreaId5 FOREIGN KEY
	(
	AreaId5
	) REFERENCES Sales.Area
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE General.Customer ADD CONSTRAINT
	FK_Customer_CustomerType_TypeId FOREIGN KEY
	(
	TypeId
	) REFERENCES General.CustomerType
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE General.Customer ADD CONSTRAINT
	FK_Customer_PaymentTerm_PaymentTermId FOREIGN KEY
	(
	PaymentTermId
	) REFERENCES General.PaymentTerm
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE General.Customer WITH NOCHECK ADD CONSTRAINT
	FK_Customer_CustomerAddress_ShippingAddressId FOREIGN KEY
	(
	ShippingAddressId
	) REFERENCES General.CustomerAddress
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE General.Customer
	NOCHECK CONSTRAINT FK_Customer_CustomerAddress_ShippingAddressId
GO
ALTER TABLE General.CustomerAddress ADD CONSTRAINT
	FK_CustomerAddress_Customer_Code FOREIGN KEY
	(
	Code
	) REFERENCES General.Customer
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE General.Customer WITH NOCHECK ADD CONSTRAINT
	FK_Customer_CustomerAddress_BillingAddressId FOREIGN KEY
	(
	BillingAddressId
	) REFERENCES General.CustomerAddress
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE General.Customer
	NOCHECK CONSTRAINT FK_Customer_CustomerAddress_BillingAddressId
GO
ALTER TABLE General.CustomerAddress SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
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
ALTER TABLE Inventory.Warehouse SET (LOCK_ESCALATION = TABLE)
GO
COMMIT";
            migrationBuilder.Sql(sql);

            // Execute sp_refreshview General.VwCustomer
            sql = @"execute sp_refreshview '[General].[VwCustomer]'";
            migrationBuilder.Sql(sql);

            // Execute sp_refreshview Inventory.vwWarehouse
            sql = @"execute sp_refreshview '[Inventory].[vwWarehouse]'";
            migrationBuilder.Sql(sql);

            // Execute sp_refreshview Purchasing.vwPurchaseOrderHeader
            sql = @"execute sp_refreshview '[Purchasing].[vwPurchaseOrderHeader]'";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Warehouse_Customer_CustCode",
                schema: "Inventory",
                table: "Warehouse");

            migrationBuilder.DropIndex(
                name: "IX_Warehouse_CustCode",
                schema: "Inventory",
                table: "Warehouse");

            migrationBuilder.DropColumn(
                name: "CustCode",
                schema: "Inventory",
                table: "Warehouse");

            migrationBuilder.DropColumn(
                name: "IsConsignee",
                schema: "General",
                table: "Customer");

            migrationBuilder.AddColumn<string>(
                name: "RcvStatus",
                schema: "Purchasing",
                table: "PurchaseOrderHeader",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: true);

            // Drop view Expedition.vwExpeditionInvoiceHeader
            var sql = @"DROP VIEW [Expedition].[vwExpeditionInvoiceHeader]";
            migrationBuilder.Sql(sql);
        }
    }
}
