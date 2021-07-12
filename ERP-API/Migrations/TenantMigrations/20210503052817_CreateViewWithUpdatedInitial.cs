using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Web.API.Migrations.TenantMigrations
{
    public partial class CreateViewWithUpdatedInitial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "Inventory",
                table: "UoMConversion");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                schema: "Inventory",
                table: "UoMConversion");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                schema: "Inventory",
                table: "UoMConversion");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                schema: "Inventory",
                table: "UoMConversion");

            migrationBuilder.AlterColumn<string>(
                name: "ContactPerson",
                schema: "General",
                table: "CustomerAddress",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldUnicode: false,
                oldMaxLength: 50);

            // Create view General.vwCustomerType
            var sql = @"CREATE VIEW [General].[vwCustomerType]
AS
	SELECT ct.*,
		u.Initial AS UpdatedInitial
	FROM General.CustomerType ct
	LEFT JOIN SystemManagement.[User] u
		ON u.Id = ct.UpdatedBy";
            migrationBuilder.Sql(sql);

            // Create view General.vwEmployee
            sql = @"CREATE VIEW [General].[vwEmployee]
AS
	SELECT e.*,
		u.Initial AS UpdatedInitial
	FROM General.Employee e
	LEFT JOIN SystemManagement.[User] u
		ON u.Id = e.UpdatedBy";
            migrationBuilder.Sql(sql);

            // Create view General.vwSupplierType
            sql = @"CREATE VIEW [General].[vwSupplierType]
AS
	SELECT st.*,
		u.Initial AS UpdatedInitial
	FROM General.SupplierType st
	LEFT JOIN SystemManagement.[User] u
		ON u.Id = st.UpdatedBy";
            migrationBuilder.Sql(sql);

            // Create view General.vwVehicleType
            sql = @"CREATE VIEW [General].[vwVehicleType]
AS
    SELECT vt.*,
		u.Initial AS UpdatedInitial
    FROM General.VehicleType vt
	LEFT JOIN SystemManagement.[User] u
		ON u.Id = vt.UpdatedBy";
            migrationBuilder.Sql(sql);

            // Create view Inventory.vwWarehouse
            sql = @"CREATE VIEW [Inventory].[vwWarehouse]
AS
    SELECT w.*,
		u.Initial AS UpdatedInitial
    FROM Inventory.Warehouse w
	LEFT JOIN SystemManagement.[User] u
		ON u.Id = w.UpdatedBy";
            migrationBuilder.Sql(sql);

            // Reordering column in table Purchasing.PurchaseReceiveHeader
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
CREATE TABLE Purchasing.Tmp_PurchaseReceiveHeader
	(
	Code varchar(17) NOT NULL,
	Date date NOT NULL,
	TransCode varchar(17) NOT NULL,
	RefNo varchar(30) NULL,
	SrcTrans smallint NOT NULL,
	SupCode varchar(8) NOT NULL,
	ReceiveBy bigint NOT NULL,
	ApproveBy bigint NULL,
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
	Mark varchar(3) NOT NULL,
	CreatedBy int NOT NULL,
	CreatedDate datetime NOT NULL,
	UpdatedBy int NOT NULL,
	UpdatedDate datetime NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Purchasing.Tmp_PurchaseReceiveHeader SET (LOCK_ESCALATION = TABLE)
GO
IF EXISTS(SELECT * FROM Purchasing.PurchaseReceiveHeader)
	 EXEC('INSERT INTO Purchasing.Tmp_PurchaseReceiveHeader (Code, Date, TransCode, RefNo, SrcTrans, SupCode, ReceiveBy, ApproveBy, CurrCode, Rate, ShipmentFee, HandlingFee, SubTotal, FinalDiscPercent, FinalDisc, IncludeTax, TaxAmount, Total, DPP, Mark, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate)
		SELECT Code, Date, TransCode, RefNo, SrcTrans, SupCode, ReceiveBy, ApproveBy, CurrCode, Rate, ShipmentFee, HandlingFee, SubTotal, FinalDiscPercent, FinalDisc, IncludeTax, TaxAmount, Total, DPP, Mark, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate FROM Purchasing.PurchaseReceiveHeader WITH (HOLDLOCK TABLOCKX)')
GO
DROP TABLE Purchasing.PurchaseReceiveHeader
GO
EXECUTE sp_rename N'Purchasing.Tmp_PurchaseReceiveHeader', N'PurchaseReceiveHeader', 'OBJECT' 
GO
ALTER TABLE Purchasing.PurchaseReceiveHeader ADD CONSTRAINT
	PK_PurchaseReceiveHeader PRIMARY KEY CLUSTERED 
	(
	Code
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
COMMIT";
            migrationBuilder.Sql(sql);

            // Reordering column in table Purchasing.StockMutation
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
CREATE TABLE Purchasing.Tmp_PurchaseReturnHeader
	(
	Code varchar(17) NOT NULL,
	Date date NOT NULL,
	RcvCode varchar(17) NULL,
	RefNo varchar(30) NULL,
	Type smallint NOT NULL,
	SupCode varchar(8) NOT NULL,
	ShippedBy bigint NOT NULL,
	ApproveBy bigint NULL,
	CurrCode varchar(3) NOT NULL,
	Rate decimal(18, 2) NOT NULL,
	ShipmentFee decimal(18, 2) NOT NULL,
	HandlingFee decimal(18, 2) NOT NULL,
	SubTotal decimal(18, 2) NOT NULL,
	FinalDisc decimal(18, 2) NOT NULL,
	IncludeTax bit NOT NULL,
	TaxAmount decimal(18, 2) NOT NULL,
	Total decimal(18, 2) NOT NULL,
	DPP decimal(18, 2) NOT NULL,
	Notes varchar(256) NULL,
	Mark varchar(3) NOT NULL,
	CreatedBy int NOT NULL,
	CreatedDate datetime NOT NULL,
	UpdatedBy int NOT NULL,
	UpdatedDate datetime NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Purchasing.Tmp_PurchaseReturnHeader SET (LOCK_ESCALATION = TABLE)
GO
IF EXISTS(SELECT * FROM Purchasing.PurchaseReturnHeader)
	 EXEC('INSERT INTO Purchasing.Tmp_PurchaseReturnHeader (Code, Date, RcvCode, RefNo, Type, SupCode, ShippedBy, ApproveBy, CurrCode, Rate, ShipmentFee, HandlingFee, SubTotal, FinalDisc, IncludeTax, TaxAmount, Total, DPP, Notes, Mark, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate)
		SELECT Code, Date, RcvCode, RefNo, Type, SupCode, ShippedBy, ApproveBy, CurrCode, Rate, ShipmentFee, HandlingFee, SubTotal, FinalDisc, IncludeTax, TaxAmount, Total, DPP, Notes, Mark, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate FROM Purchasing.PurchaseReturnHeader WITH (HOLDLOCK TABLOCKX)')
GO
DROP TABLE Purchasing.PurchaseReturnHeader
GO
EXECUTE sp_rename N'Purchasing.Tmp_PurchaseReturnHeader', N'PurchaseReturnHeader', 'OBJECT' 
GO
ALTER TABLE Purchasing.PurchaseReturnHeader ADD CONSTRAINT
	PK_PurchaseReturnHeader PRIMARY KEY CLUSTERED 
	(
	Code
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
COMMIT";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
           migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                schema: "Inventory",
                table: "UoMConversion",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                schema: "Inventory",
                table: "UoMConversion",
                type: "datetime",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy",
                schema: "Inventory",
                table: "UoMConversion",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                schema: "Inventory",
                table: "UoMConversion",
                type: "datetime",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<string>(
                name: "ContactPerson",
                schema: "General",
                table: "CustomerAddress",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldUnicode: false,
                oldMaxLength: 50,
                oldNullable: true);

            // Drop view General.vwCustomerType
            var sql = @"DROP VIEW [General].[vwCustomerType]";
            migrationBuilder.Sql(sql);

            // Drop view General.vwEmployee
            sql = @"DROP VIEW [General].[vwEmployee]";
            migrationBuilder.Sql(sql);

            // Drop view General.vwSupplierType
            sql = @"DROP VIEW [General].[vwSupplierType]";
            migrationBuilder.Sql(sql);

            // Drop view General.vwVehicleType
            sql = @"DROP VIEW [General].[vwVehicleType]";
            migrationBuilder.Sql(sql);

            // Drop view Inventory.vwWarehouse
            sql = @"DROP VIEW [Inventory].[vwWarehouse]";
            migrationBuilder.Sql(sql);
        }
    }
}
