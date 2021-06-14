using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP_API.Migrations.TenantMigrations
{
    public partial class CreateTableSalesOrderFreeGoodAndOthers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category1",
                schema: "Inventory",
                table: "Item");

            migrationBuilder.DropColumn(
                name: "Category2",
                schema: "Inventory",
                table: "Item");

            migrationBuilder.DropColumn(
                name: "Category3",
                schema: "Inventory",
                table: "Item");

            migrationBuilder.DropColumn(
                name: "Category4",
                schema: "Inventory",
                table: "Item");

            migrationBuilder.DropColumn(
                name: "Category5",
                schema: "Inventory",
                table: "Item");

            migrationBuilder.DropColumn(
                name: "ValuationMethod",
                schema: "Inventory",
                table: "Item");

            migrationBuilder.DropColumn(
                name: "Username",
                schema: "General",
                table: "Employee");

            migrationBuilder.AddColumn<int>(
                name: "SalesGroupId",
                schema: "General",
                table: "Employee",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SalesDeliveryDetailFreeGood",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    DlvOrderDetailId = table.Column<long>(type: "bigint", nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    PromoCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    CoaCode = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesDeliveryDetailFreeGood", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SalesOrderDetailFreeGood",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    OrderDetailId = table.Column<long>(type: "bigint", nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    PromoCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    QtyClosed = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    CoaCode = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesOrderDetailFreeGood", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Employee_SalesGroupId",
                schema: "General",
                table: "Employee",
                column: "SalesGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employee_SalesmanGroup_SalesGroupId",
                schema: "General",
                table: "Employee",
                column: "SalesGroupId",
                principalSchema: "Sales",
                principalTable: "SalesmanGroup",
                principalColumn: "Id");

            // Reorder table General.Employee
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
	 EXEC('INSERT INTO General.Tmp_Employee (Id, Initial, Type, SalesGroupId, FirstName, LastName, Sex, BirthDate, BirthPlace, MaritalStatus, IdentityCardNo, Religion, Address1, Address2, Phone, IsActive, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate)
		SELECT Id, Initial, Type, SalesGroupId, FirstName, LastName, Sex, BirthDate, BirthPlace, MaritalStatus, IdentityCardNo, Religion, Address1, Address2, Phone, IsActive, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate FROM General.Employee WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT General.Tmp_Employee OFF
GO
ALTER TABLE Sales.DeliveryPlanHeader
	DROP CONSTRAINT FK_DeliveryPlanHeader_Employee_DriverId
GO
ALTER TABLE SystemManagement.[User]
	DROP CONSTRAINT FK_User_Employee_EmployeeId
GO
ALTER TABLE General.Vehicle
	DROP CONSTRAINT FK_Vehicle_Employee_DriverId
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
COMMIT";
            migrationBuilder.Sql(sql);

            // Execute sp_refreshview General.vwEmployee
            sql = @"execute sp_refreshview 'General.vwEmployee'";
            migrationBuilder.Sql(sql);

            // Execute sp_refreshview Inventory.vwItem
            sql = @"execute sp_refreshview 'Inventory.vwItem'";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employee_SalesmanGroup_SalesGroupId",
                schema: "General",
                table: "Employee");

            migrationBuilder.DropTable(
                name: "SalesDeliveryDetailFreeGood",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "SalesOrderDetailFreeGood",
                schema: "Sales");

            migrationBuilder.DropIndex(
                name: "IX_Employee_SalesGroupId",
                schema: "General",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "SalesGroupId",
                schema: "General",
                table: "Employee");

            migrationBuilder.AddColumn<string>(
                name: "Category1",
                schema: "Inventory",
                table: "Item",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Category2",
                schema: "Inventory",
                table: "Item",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Category3",
                schema: "Inventory",
                table: "Item",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Category4",
                schema: "Inventory",
                table: "Item",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Category5",
                schema: "Inventory",
                table: "Item",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "ValuationMethod",
                schema: "Inventory",
                table: "Item",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Username",
                schema: "General",
                table: "Employee",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }
    }
}
