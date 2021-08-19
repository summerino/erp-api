using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AddColumnCreditUsedinCustomer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CreditUsed",
                schema: "General",
                table: "Customer",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            // Reorder column General.vwCustomer
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
	CreditUsed decimal(18, 2) NOT NULL,
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
	MobileSignIn bit NOT NULL,
	CatalogUserId uniqueidentifier NULL,
	MobileUsername nvarchar(50) NULL,
	IsMobileLoggedIn bit NOT NULL,
	MobileLastLogin datetime NULL,
	MobileSessionId varchar(50) NULL,
	MobileTokenId varchar(MAX) NULL,
	MobileIPAddress varchar(40) NULL,
	IsActive bit NOT NULL,
	CreatedBy int NOT NULL,
	CreatedDate datetime NOT NULL,
	UpdatedBy int NOT NULL,
	UpdatedDate datetime NOT NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE General.Tmp_Customer SET (LOCK_ESCALATION = TABLE)
GO
IF EXISTS(SELECT * FROM General.Customer)
	 EXEC('INSERT INTO General.Tmp_Customer (Code, Initial, Name, TypeId, Email, Website, PaymentTermId, CreditLimit, CreditUsed, RefNo, Notes, BillingAddressId, ShippingAddressId, AreaId1, AreaId2, AreaId3, AreaId4, AreaId5, IsConsignee, MobileSignIn, CatalogUserId, MobileUsername, IsMobileLoggedIn, MobileLastLogin, MobileSessionId, MobileTokenId, MobileIPAddress, IsActive, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate)
		SELECT Code, Initial, Name, TypeId, Email, Website, PaymentTermId, CreditLimit, CreditUsed, RefNo, Notes, BillingAddressId, ShippingAddressId, AreaId1, AreaId2, AreaId3, AreaId4, AreaId5, IsConsignee, MobileSignIn, CatalogUserId, MobileUsername, IsMobileLoggedIn, MobileLastLogin, MobileSessionId, MobileTokenId, MobileIPAddress, IsActive, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate FROM General.Customer WITH (HOLDLOCK TABLOCKX)')
GO
ALTER TABLE Sales.SalesOrderHeader
	DROP CONSTRAINT FK_SalesOrderHeader_Customer_CustCode
GO
ALTER TABLE Accounting.BeginningBalanceAR
	DROP CONSTRAINT FK_BeginningBalanceAR_Customer_CustCode
GO
ALTER TABLE MobileSales.MobileCustomer
	DROP CONSTRAINT FK_MobileCustomer_Customer_CustCode
GO
ALTER TABLE Sales.PromoSubject
	DROP CONSTRAINT FK_PromoSubject_Customer_CustCode
GO
ALTER TABLE Accounting.BeginningBalanceCreditMemo
	DROP CONSTRAINT FK_BeginningBalanceCreditMemo_Customer_CustCode
GO
ALTER TABLE Sales.SalesInvoiceHeader
	DROP CONSTRAINT FK_SalesInvoiceHeader_Customer_CustCode
GO
ALTER TABLE Inventory.Warehouse
	DROP CONSTRAINT FK_Warehouse_Customer_CustCode
GO
ALTER TABLE General.Customer
	DROP CONSTRAINT FK_Customer_CustomerAddress_ShippingAddressId
GO
ALTER TABLE General.CustomerAddress
	DROP CONSTRAINT FK_CustomerAddress_Customer_Code
GO
ALTER TABLE General.Customer
	DROP CONSTRAINT FK_Customer_CustomerAddress_BillingAddressId
GO
ALTER TABLE Sales.CreditMemo
	DROP CONSTRAINT FK_CreditMemo_Customer_CustCode
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
ALTER TABLE Sales.CreditMemo ADD CONSTRAINT
	FK_CreditMemo_Customer_CustCode FOREIGN KEY
	(
	CustCode
	) REFERENCES General.Customer
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Sales.CreditMemo SET (LOCK_ESCALATION = TABLE)
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
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Sales.SalesInvoiceHeader ADD CONSTRAINT
	FK_SalesInvoiceHeader_Customer_CustCode FOREIGN KEY
	(
	CustCode
	) REFERENCES General.Customer
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
ALTER TABLE Accounting.BeginningBalanceCreditMemo ADD CONSTRAINT
	FK_BeginningBalanceCreditMemo_Customer_CustCode FOREIGN KEY
	(
	CustCode
	) REFERENCES General.Customer
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Accounting.BeginningBalanceCreditMemo SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Sales.PromoSubject ADD CONSTRAINT
	FK_PromoSubject_Customer_CustCode FOREIGN KEY
	(
	CustCode
	) REFERENCES General.Customer
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Sales.PromoSubject SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE MobileSales.MobileCustomer ADD CONSTRAINT
	FK_MobileCustomer_Customer_CustCode FOREIGN KEY
	(
	CustCode
	) REFERENCES General.Customer
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE MobileSales.MobileCustomer SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Accounting.BeginningBalanceAR ADD CONSTRAINT
	FK_BeginningBalanceAR_Customer_CustCode FOREIGN KEY
	(
	CustCode
	) REFERENCES General.Customer
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Accounting.BeginningBalanceAR SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
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
ALTER TABLE Sales.SalesOrderHeader SET (LOCK_ESCALATION = TABLE)
GO
COMMIT";
            migrationBuilder.Sql(sql);

			// Refresh view General.vwCustomer
            sql = @"execute sp_refreshview 'General.vwCustomer'";
            migrationBuilder.Sql(sql);
		}

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreditUsed",
                schema: "General",
                table: "Customer");
        }
    }
}
