using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AlterColumnCustomerAddress : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ShowInMobile",
                schema: "Inventory",
                table: "ItemGroupSubGroup",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "Lat",
                schema: "General",
                table: "CustomerAddress",
                type: "decimal(9,6)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Lng",
                schema: "General",
                table: "CustomerAddress",
                type: "decimal(9,6)",
                nullable: true);

			// Reorder column General.CustomerAddress
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
CREATE TABLE General.Tmp_CustomerAddress
	(
	Id int NOT NULL IDENTITY (1, 1),
	Code varchar(8) NOT NULL,
	Initial varchar(20) NOT NULL,
	Address1 varchar(100) NOT NULL,
	Address2 varchar(100) NULL,
	ContactPerson varchar(50) NULL,
	Phone varchar(30) NOT NULL,
	Fax varchar(15) NULL,
	Lat decimal(9, 6) NULL,
	Lng decimal(9, 6) NULL,
	IsDefault bit NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE General.Tmp_CustomerAddress SET (LOCK_ESCALATION = TABLE)
GO
SET IDENTITY_INSERT General.Tmp_CustomerAddress ON
GO
IF EXISTS(SELECT * FROM General.CustomerAddress)
	 EXEC('INSERT INTO General.Tmp_CustomerAddress (Id, Code, Initial, Address1, Address2, ContactPerson, Phone, Fax, Lat, Lng, IsDefault)
		SELECT Id, Code, Initial, Address1, Address2, ContactPerson, Phone, Fax, Lat, Lng, IsDefault FROM General.CustomerAddress WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT General.Tmp_CustomerAddress OFF
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
DROP TABLE General.CustomerAddress
GO
EXECUTE sp_rename N'General.Tmp_CustomerAddress', N'CustomerAddress', 'OBJECT' 
GO
ALTER TABLE General.CustomerAddress ADD CONSTRAINT
	PK_CustomerAddress PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX IX_CustomerAddress_Code ON General.CustomerAddress
	(
	Code
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
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
ALTER TABLE General.Customer SET (LOCK_ESCALATION = TABLE)
GO
COMMIT";
            migrationBuilder.Sql(sql);

			// Reorder column Inventory.ItemGroupSubGroup
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
ALTER TABLE Inventory.ItemGroupSubGroup
	DROP CONSTRAINT FK_ItemGroupSubGroup_ItemGroup_ItemGroupId
GO
ALTER TABLE Inventory.ItemGroup SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE Inventory.Tmp_ItemGroupSubGroup
	(
	Id int NOT NULL IDENTITY (1, 1),
	ItemGroupId int NOT NULL,
	Name varchar(50) NOT NULL,
	Value varchar(1000) NOT NULL,
	ShowInMobile bit NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Inventory.Tmp_ItemGroupSubGroup SET (LOCK_ESCALATION = TABLE)
GO
SET IDENTITY_INSERT Inventory.Tmp_ItemGroupSubGroup ON
GO
IF EXISTS(SELECT * FROM Inventory.ItemGroupSubGroup)
	 EXEC('INSERT INTO Inventory.Tmp_ItemGroupSubGroup (Id, ItemGroupId, Name, Value, ShowInMobile)
		SELECT Id, ItemGroupId, Name, Value, ShowInMobile FROM Inventory.ItemGroupSubGroup WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT Inventory.Tmp_ItemGroupSubGroup OFF
GO
DROP TABLE Inventory.ItemGroupSubGroup
GO
EXECUTE sp_rename N'Inventory.Tmp_ItemGroupSubGroup', N'ItemGroupSubGroup', 'OBJECT' 
GO
ALTER TABLE Inventory.ItemGroupSubGroup ADD CONSTRAINT
	PK_ItemGroupSubGroup PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX IX_ItemGroupSubGroup_ItemGroupId ON Inventory.ItemGroupSubGroup
	(
	ItemGroupId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE Inventory.ItemGroupSubGroup ADD CONSTRAINT
	FK_ItemGroupSubGroup_ItemGroup_ItemGroupId FOREIGN KEY
	(
	ItemGroupId
	) REFERENCES Inventory.ItemGroup
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
            migrationBuilder.DropColumn(
                name: "ShowInMobile",
                schema: "Inventory",
                table: "ItemGroupSubGroup");

            migrationBuilder.DropColumn(
                name: "Lat",
                schema: "General",
                table: "CustomerAddress");

            migrationBuilder.DropColumn(
                name: "Lng",
                schema: "General",
                table: "CustomerAddress");
        }
    }
}
