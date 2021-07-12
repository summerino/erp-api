using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Web.API.Migrations.TenantMigrations
{
    public partial class CreateForeignKey2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreditTerm",
                schema: "General",
                table: "Customer");

            migrationBuilder.AddColumn<string>(
                name: "WarehouseCode",
                schema: "General",
                table: "Employee",
                type: "varchar(8)",
                unicode: false,
                maxLength: 8,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentTermId",
                schema: "General",
                table: "Customer",
                type: "int",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.CreateIndex(
                name: "IX_Employee_WarehouseCode",
                schema: "General",
                table: "Employee",
                column: "WarehouseCode");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_PaymentTermId",
                schema: "General",
                table: "Customer",
                column: "PaymentTermId");

            migrationBuilder.AddForeignKey(
                name: "FK_Customer_PaymentTerm_PaymentTermId",
                schema: "General",
                table: "Customer",
                column: "PaymentTermId",
                principalSchema: "General",
                principalTable: "PaymentTerm",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Employee_Warehouse_WarehouseCode",
                schema: "General",
                table: "Employee",
                column: "WarehouseCode",
                principalSchema: "Inventory",
                principalTable: "Warehouse",
                principalColumn: "Code");

            // Reorder column General.Customer
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
	RefNo varchar(30) NULL,
	Notes varchar(256) NULL,
	BillingAddressId int NULL,
	ShippingAddressId int NULL,
	AreaId1 int NULL,
	AreaId2 int NULL,
	AreaId3 int NULL,
	AreaId4 int NULL,
	AreaId5 int NULL,
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
	 EXEC('INSERT INTO General.Tmp_Customer (Code, Initial, Name, TypeId, Email, Website, PaymentTermId, CreditLimit, RefNo, Notes, BillingAddressId, ShippingAddressId, AreaId1, AreaId2, AreaId3, AreaId4, AreaId5, IsActive, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate)
		SELECT Code, Initial, Name, TypeId, Email, Website, PaymentTermId, CreditLimit, RefNo, Notes, BillingAddressId, ShippingAddressId, AreaId1, AreaId2, AreaId3, AreaId4, AreaId5, IsActive, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate FROM General.Customer WITH (HOLDLOCK TABLOCKX)')
GO
ALTER TABLE General.CustomerAddress
	DROP CONSTRAINT FK_CustomerAddress_Customer_Code
GO
ALTER TABLE General.Customer
	DROP CONSTRAINT FK_Customer_CustomerAddress_BillingAddressId
GO
ALTER TABLE General.Customer
	DROP CONSTRAINT FK_Customer_CustomerAddress_ShippingAddressId
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
ALTER TABLE General.CustomerAddress SET (LOCK_ESCALATION = TABLE)
GO
COMMIT";
            migrationBuilder.Sql(sql);

			// Reorder column General.Employee
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
ALTER TABLE General.Employee
	DROP CONSTRAINT FK_Employee_Warehouse_WarehouseCode
GO
ALTER TABLE Inventory.Warehouse SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE General.Employee
	DROP CONSTRAINT FK_Employee_SalesmanGroup_SalesGroupId
GO
ALTER TABLE Sales.SalesmanGroup SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE General.Tmp_Employee
	(
	Id bigint NOT NULL IDENTITY (1, 1),
	Initial varchar(20) NOT NULL,
	Type smallint NOT NULL,
	SalesGroupId int NULL,
	FirstName varchar(50) NOT NULL,
	LastName varchar(50) NULL,
	Sex bit NOT NULL,
	BirthDate date NULL,
	BirthPlace varchar(50) NULL,
	MaritalStatus tinyint NULL,
	IdentityCardNo varchar(20) NULL,
	Religion tinyint NOT NULL,
	Address1 varchar(100) NULL,
	Address2 varchar(100) NULL,
	Phone varchar(30) NULL,
	WarehouseCode varchar(8) NULL,
	IsActive bit NOT NULL,
	CreatedBy int NOT NULL,
	CreatedDate datetime NOT NULL,
	UpdatedBy int NOT NULL,
	UpdatedDate datetime NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE General.Tmp_Employee SET (LOCK_ESCALATION = TABLE)
GO
SET IDENTITY_INSERT General.Tmp_Employee ON
GO
IF EXISTS(SELECT * FROM General.Employee)
	 EXEC('INSERT INTO General.Tmp_Employee (Id, Initial, Type, SalesGroupId, FirstName, LastName, Sex, BirthDate, BirthPlace, MaritalStatus, IdentityCardNo, Religion, Address1, Address2, Phone, WarehouseCode, IsActive, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate)
		SELECT Id, Initial, Type, SalesGroupId, FirstName, LastName, Sex, BirthDate, BirthPlace, MaritalStatus, IdentityCardNo, Religion, Address1, Address2, Phone, WarehouseCode, IsActive, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate FROM General.Employee WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT General.Tmp_Employee OFF
GO
ALTER TABLE General.Vehicle
	DROP CONSTRAINT FK_Vehicle_Employee_DriverId
GO
ALTER TABLE SystemManagement.[User]
	DROP CONSTRAINT FK_User_Employee_EmployeeId
GO
ALTER TABLE Sales.DeliveryPlanHeader
	DROP CONSTRAINT FK_DeliveryPlanHeader_Employee_DriverId
GO
DROP TABLE General.Employee
GO
EXECUTE sp_rename N'General.Tmp_Employee', N'Employee', 'OBJECT' 
GO
ALTER TABLE General.Employee ADD CONSTRAINT
	PK_Employee PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX IX_Employee_SalesGroupId ON General.Employee
	(
	SalesGroupId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_Employee_WarehouseCode ON General.Employee
	(
	WarehouseCode
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE General.Employee ADD CONSTRAINT
	FK_Employee_SalesmanGroup_SalesGroupId FOREIGN KEY
	(
	SalesGroupId
	) REFERENCES Sales.SalesmanGroup
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
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
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Sales.DeliveryPlanHeader ADD CONSTRAINT
	FK_DeliveryPlanHeader_Employee_DriverId FOREIGN KEY
	(
	DriverId
	) REFERENCES General.Employee
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Sales.DeliveryPlanHeader SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE SystemManagement.[User] ADD CONSTRAINT
	FK_User_Employee_EmployeeId FOREIGN KEY
	(
	EmployeeId
	) REFERENCES General.Employee
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE SystemManagement.[User] SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE General.Vehicle ADD CONSTRAINT
	FK_Vehicle_Employee_DriverId FOREIGN KEY
	(
	DriverId
	) REFERENCES General.Employee
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE General.Vehicle SET (LOCK_ESCALATION = TABLE)
GO
COMMIT";
            migrationBuilder.Sql(sql);

			// Execute sp_refreshview General.vwCustomer
			sql = "execute sp_refreshview 'General.vwCustomer'";
            migrationBuilder.Sql(sql);

			// Execute sp_refreshview General.vwEmployee
			sql = "execute sp_refreshview 'General.vwEmployee'";
            migrationBuilder.Sql(sql);
		}

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customer_PaymentTerm_PaymentTermId",
                schema: "General",
                table: "Customer");

            migrationBuilder.DropForeignKey(
                name: "FK_Employee_Warehouse_WarehouseCode",
                schema: "General",
                table: "Employee");

            migrationBuilder.DropIndex(
                name: "IX_Employee_WarehouseCode",
                schema: "General",
                table: "Employee");

            migrationBuilder.DropIndex(
                name: "IX_Customer_PaymentTermId",
                schema: "General",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "WarehouseCode",
                schema: "General",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "PaymentTermId",
                schema: "General",
                table: "Customer");

            migrationBuilder.AddColumn<short>(
                name: "CreditTerm",
                schema: "General",
                table: "Customer",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);
        }
    }
}
