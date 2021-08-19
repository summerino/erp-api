using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class CreateTableMobileSales2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MobileCostHeader",
                schema: "MobileSales",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    CashBankCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: true),
                    SalesmanId = table.Column<long>(type: "bigint", nullable: false),
                    CurrCode = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Mark = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ApprovedBy = table.Column<int>(type: "int", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MobileCostHeader", x => x.Code);
                    table.ForeignKey(
                        name: "FK_MobileCostHeader_Employee_SalesmanId",
                        column: x => x.SalesmanId,
                        principalSchema: "General",
                        principalTable: "Employee",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileCostHeader_GeneralCashBankHeader_CashBankCode",
                        column: x => x.CashBankCode,
                        principalSchema: "Finance",
                        principalTable: "GeneralCashBankHeader",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "MobileItemRequestHeader",
                schema: "MobileSales",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    TransferCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: true),
                    SalesmanId = table.Column<long>(type: "bigint", nullable: false),
                    AreaId1 = table.Column<int>(type: "int", nullable: true),
                    AreaId2 = table.Column<int>(type: "int", nullable: true),
                    AreaId3 = table.Column<int>(type: "int", nullable: true),
                    AreaId4 = table.Column<int>(type: "int", nullable: true),
                    AreaId5 = table.Column<int>(type: "int", nullable: true),
                    Mark = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ApprovedBy = table.Column<int>(type: "int", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MobileItemRequestHeader", x => x.Code);
                    table.ForeignKey(
                        name: "FK_MobileItemRequestHeader_Area_AreaId1",
                        column: x => x.AreaId1,
                        principalSchema: "Sales",
                        principalTable: "Area",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileItemRequestHeader_Area_AreaId2",
                        column: x => x.AreaId2,
                        principalSchema: "Sales",
                        principalTable: "Area",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileItemRequestHeader_Area_AreaId3",
                        column: x => x.AreaId3,
                        principalSchema: "Sales",
                        principalTable: "Area",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileItemRequestHeader_Area_AreaId4",
                        column: x => x.AreaId4,
                        principalSchema: "Sales",
                        principalTable: "Area",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileItemRequestHeader_Area_AreaId5",
                        column: x => x.AreaId5,
                        principalSchema: "Sales",
                        principalTable: "Area",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileItemRequestHeader_Employee_SalesmanId",
                        column: x => x.SalesmanId,
                        principalSchema: "General",
                        principalTable: "Employee",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileItemRequestHeader_TransferStockHeader_TransferCode",
                        column: x => x.TransferCode,
                        principalSchema: "Inventory",
                        principalTable: "TransferStockHeader",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "MobileCostDetail",
                schema: "MobileSales",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    CoaCode = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MobileCostDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MobileCostDetail_MobileItemRequestHeader_Code",
                        column: x => x.Code,
                        principalSchema: "MobileSales",
                        principalTable: "MobileItemRequestHeader",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "MobileCostImage",
                schema: "MobileSales",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    Image = table.Column<string>(type: "varchar(max)", unicode: false, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MobileCostImage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MobileCostImage_MobileItemRequestHeader_Code",
                        column: x => x.Code,
                        principalSchema: "MobileSales",
                        principalTable: "MobileItemRequestHeader",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "MobileItemRequestDetail",
                schema: "MobileSales",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MobileItemRequestDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MobileItemRequestDetail_Item_ItemId",
                        column: x => x.ItemId,
                        principalSchema: "Inventory",
                        principalTable: "Item",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileItemRequestDetail_MobileItemRequestHeader_Code",
                        column: x => x.Code,
                        principalSchema: "MobileSales",
                        principalTable: "MobileItemRequestHeader",
                        principalColumn: "Code");
                    table.ForeignKey(
                        name: "FK_MobileItemRequestDetail_UoMConversion_UnitId",
                        column: x => x.UnitId,
                        principalSchema: "Inventory",
                        principalTable: "UoMConversion",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MobileCostDetail_CoaCode",
                schema: "MobileSales",
                table: "MobileCostDetail",
                column: "CoaCode");

            migrationBuilder.CreateIndex(
                name: "IX_MobileCostDetail_Code",
                schema: "MobileSales",
                table: "MobileCostDetail",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_MobileCostHeader_CashBankCode",
                schema: "MobileSales",
                table: "MobileCostHeader",
                column: "CashBankCode");

            migrationBuilder.CreateIndex(
                name: "IX_MobileCostHeader_SalesmanId",
                schema: "MobileSales",
                table: "MobileCostHeader",
                column: "SalesmanId");

            migrationBuilder.CreateIndex(
                name: "IX_MobileCostImage_Code",
                schema: "MobileSales",
                table: "MobileCostImage",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_MobileItemRequestDetail_Code",
                schema: "MobileSales",
                table: "MobileItemRequestDetail",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_MobileItemRequestDetail_ItemId",
                schema: "MobileSales",
                table: "MobileItemRequestDetail",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_MobileItemRequestDetail_UnitId",
                schema: "MobileSales",
                table: "MobileItemRequestDetail",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_MobileItemRequestHeader_AreaId1",
                schema: "MobileSales",
                table: "MobileItemRequestHeader",
                column: "AreaId1");

            migrationBuilder.CreateIndex(
                name: "IX_MobileItemRequestHeader_AreaId2",
                schema: "MobileSales",
                table: "MobileItemRequestHeader",
                column: "AreaId2");

            migrationBuilder.CreateIndex(
                name: "IX_MobileItemRequestHeader_AreaId3",
                schema: "MobileSales",
                table: "MobileItemRequestHeader",
                column: "AreaId3");

            migrationBuilder.CreateIndex(
                name: "IX_MobileItemRequestHeader_AreaId4",
                schema: "MobileSales",
                table: "MobileItemRequestHeader",
                column: "AreaId4");

            migrationBuilder.CreateIndex(
                name: "IX_MobileItemRequestHeader_AreaId5",
                schema: "MobileSales",
                table: "MobileItemRequestHeader",
                column: "AreaId5");

            migrationBuilder.CreateIndex(
                name: "IX_MobileItemRequestHeader_SalesmanId",
                schema: "MobileSales",
                table: "MobileItemRequestHeader",
                column: "SalesmanId");

            migrationBuilder.CreateIndex(
                name: "IX_MobileItemRequestHeader_TransferCode",
                schema: "MobileSales",
                table: "MobileItemRequestHeader",
                column: "TransferCode");

            migrationBuilder.AddColumn<bool>(
                name: "ShowInMobile",
                schema: "Accounting",
                table: "COA",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // Reorder column Accounting.COA
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
ALTER TABLE Accounting.COA
	DROP CONSTRAINT FK_COA_Currency_CurrCode
GO
ALTER TABLE General.Currency SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Accounting.COA
	DROP CONSTRAINT FK_COA_COAType_TypeId
GO
ALTER TABLE Accounting.COAType SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE Accounting.Tmp_COA
	(
	Id int NOT NULL IDENTITY (1, 1),
	Code varchar(6) NOT NULL,
	Name varchar(50) NOT NULL,
	TypeId int NOT NULL,
	ParentId int NULL,
	Deep int NULL,
	Description varchar(100) NULL,
	CurrCode varchar(3) NULL,
	CBType varchar(1) NULL,
	VouCode varchar(4) NULL,
	BsCode varchar(4) NULL,
	IsCode varchar(4) NULL,
	IsDetCode varchar(4) NULL,
	ShowInMobile bit NOT NULL,
	IsActive bit NOT NULL,
	CreatedBy int NOT NULL,
	CreatedDate datetime NOT NULL,
	UpdatedBy int NOT NULL,
	UpdatedDate datetime NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Accounting.Tmp_COA SET (LOCK_ESCALATION = TABLE)
GO
SET IDENTITY_INSERT Accounting.Tmp_COA ON
GO
IF EXISTS(SELECT * FROM Accounting.COA)
	 EXEC('INSERT INTO Accounting.Tmp_COA (Id, Code, Name, TypeId, ParentId, Deep, Description, CurrCode, CBType, VouCode, BsCode, IsCode, IsDetCode, ShowInMobile, IsActive, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate)
		SELECT Id, Code, Name, TypeId, ParentId, Deep, Description, CurrCode, CBType, VouCode, BsCode, IsCode, IsDetCode, ShowInMobile, IsActive, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate FROM Accounting.COA WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT Accounting.Tmp_COA OFF
GO
DROP TABLE Accounting.COA
GO
EXECUTE sp_rename N'Accounting.Tmp_COA', N'COA', 'OBJECT' 
GO
ALTER TABLE Accounting.COA ADD CONSTRAINT
	PK_COA PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE UNIQUE NONCLUSTERED INDEX IX_COA_Code ON Accounting.COA
	(
	Code
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_COA_CurrCode ON Accounting.COA
	(
	CurrCode
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_COA_TypeId ON Accounting.COA
	(
	TypeId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE Accounting.COA ADD CONSTRAINT
	FK_COA_COAType_TypeId FOREIGN KEY
	(
	TypeId
	) REFERENCES Accounting.COAType
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Accounting.COA ADD CONSTRAINT
	FK_COA_Currency_CurrCode FOREIGN KEY
	(
	CurrCode
	) REFERENCES General.Currency
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
COMMIT";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MobileCostDetail",
                schema: "MobileSales");

            migrationBuilder.DropTable(
                name: "MobileCostHeader",
                schema: "MobileSales");

            migrationBuilder.DropTable(
                name: "MobileCostImage",
                schema: "MobileSales");

            migrationBuilder.DropTable(
                name: "MobileItemRequestDetail",
                schema: "MobileSales");

            migrationBuilder.DropTable(
                name: "MobileItemRequestHeader",
                schema: "MobileSales");

            migrationBuilder.DropColumn(
                name: "ShowInMobile",
                schema: "Accounting",
                table: "COA");
        }
    }
}
