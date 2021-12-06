using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AddingTableMobileCustomerOrder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "MobileCustomer");

            migrationBuilder.AddColumn<string>(
                name: "Content",
                schema: "Sales",
                table: "PromoHeader",
                type: "varchar(max)",
                unicode: false,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MobileOrderHeader",
                schema: "MobileCustomer",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    SalesOrderCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: true),
                    CustCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    CurrCode = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IncludeTax = table.Column<bool>(type: "bit", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DPP = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
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
                    table.PrimaryKey("PK_MobileCustomer_MobileOrderHeader", x => x.Code);
                    table.ForeignKey(
                        name: "FK_MobileOrderHeader_SalesOrderHeader_SalesOrderCode",
                        column: x => x.SalesOrderCode,
                        principalSchema: "Sales",
                        principalTable: "SalesOrderHeader",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "MobileOrderDetail",
                schema: "MobileCustomer",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    Disc = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    TaxId = table.Column<int>(type: "int", nullable: true),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NettPrice = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    DPP = table.Column<decimal>(type: "decimal(19,6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MobileCustomer_MobileOrderDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MobileOrderDetail_Item_ItemId",
                        column: x => x.ItemId,
                        principalSchema: "Inventory",
                        principalTable: "Item",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileOrderDetail_MobileOrderHeader_Code",
                        column: x => x.Code,
                        principalSchema: "MobileCustomer",
                        principalTable: "MobileOrderHeader",
                        principalColumn: "Code");
                    table.ForeignKey(
                        name: "FK_MobileOrderDetail_Tax_TaxId",
                        column: x => x.TaxId,
                        principalSchema: "General",
                        principalTable: "Tax",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileOrderDetail_UoM_UomId",
                        column: x => x.UomId,
                        principalSchema: "Inventory",
                        principalTable: "UoM",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileOrderDetail_UoMConversion_UnitId",
                        column: x => x.UnitId,
                        principalSchema: "Inventory",
                        principalTable: "UoMConversion",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MobileOrderDetailDiscount",
                schema: "MobileCustomer",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    OrderDetailId = table.Column<long>(type: "bigint", nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    PromoCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: true),
                    PromoDetailId = table.Column<long>(type: "bigint", nullable: true),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    IsPercentage = table.Column<bool>(type: "bit", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MobileCustomer_MobileOrderDetailDiscount", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MobileOrderDetailDiscount_MobileOrderDetail_OrderDetailId",
                        column: x => x.OrderDetailId,
                        principalSchema: "MobileCustomer",
                        principalTable: "MobileOrderDetail",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileOrderDetailDiscount_MobileOrderHeader_Code",
                        column: x => x.Code,
                        principalSchema: "MobileCustomer",
                        principalTable: "MobileOrderHeader",
                        principalColumn: "Code");
                    table.ForeignKey(
                        name: "FK_MobileOrderDetailDiscount_PromoDetail_PromoDetailId",
                        column: x => x.PromoDetailId,
                        principalSchema: "Sales",
                        principalTable: "PromoDetail",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileOrderDetailDiscount_PromoHeader_PromoCode",
                        column: x => x.PromoCode,
                        principalSchema: "Sales",
                        principalTable: "PromoHeader",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "MobileOrderDetailFreeGood",
                schema: "MobileCustomer",
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
                    Qty = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MobileCustomer_MobileOrderDetailFreeGood", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MobileOrderDetailFreeGood_Item_ItemId",
                        column: x => x.ItemId,
                        principalSchema: "Inventory",
                        principalTable: "Item",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileOrderDetailFreeGood_MobileOrderDetail_OrderDetailId",
                        column: x => x.OrderDetailId,
                        principalSchema: "MobileCustomer",
                        principalTable: "MobileOrderDetail",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileOrderDetailFreeGood_MobileOrderHeader_Code",
                        column: x => x.Code,
                        principalSchema: "MobileCustomer",
                        principalTable: "MobileOrderHeader",
                        principalColumn: "Code");
                    table.ForeignKey(
                        name: "FK_MobileOrderDetailFreeGood_PromoHeader_PromoCode",
                        column: x => x.PromoCode,
                        principalSchema: "Sales",
                        principalTable: "PromoHeader",
                        principalColumn: "Code");
                    table.ForeignKey(
                        name: "FK_MobileOrderDetailFreeGood_UoM_UomId",
                        column: x => x.UomId,
                        principalSchema: "Inventory",
                        principalTable: "UoM",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileOrderDetailFreeGood_UoMConversion_UnitId",
                        column: x => x.UnitId,
                        principalSchema: "Inventory",
                        principalTable: "UoMConversion",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MobileCustomer_MobileOrderDetail_Code",
                schema: "MobileCustomer",
                table: "MobileOrderDetail",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_MobileCustomer_MobileOrderDetail_ItemId",
                schema: "MobileCustomer",
                table: "MobileOrderDetail",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_MobileCustomer_MobileOrderDetail_TaxId ",
                schema: "MobileCustomer",
                table: "MobileOrderDetail",
                column: "TaxId");

            migrationBuilder.CreateIndex(
                name: "IX_MobileCustomer_MobileOrderDetail_UnitId",
                schema: "MobileCustomer",
                table: "MobileOrderDetail",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_MobileCustomer_MobileOrderDetail_UomId",
                schema: "MobileCustomer",
                table: "MobileOrderDetail",
                column: "UomId");

            migrationBuilder.CreateIndex(
                name: "IX_MobileCustomer_MobileOrderDetailDiscount_Code",
                schema: "MobileCustomer",
                table: "MobileOrderDetailDiscount",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_MobileCustomer_MobileOrderDetailDiscount_OrderDetailId",
                schema: "MobileCustomer",
                table: "MobileOrderDetailDiscount",
                column: "OrderDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_MobileCustomer_MobileOrderDetailDiscount_PromoCode",
                schema: "MobileCustomer",
                table: "MobileOrderDetailDiscount",
                column: "PromoCode");

            migrationBuilder.CreateIndex(
                name: "IX_MobileCustomer_MobileOrderDetailDiscount_PromoDetailId",
                schema: "MobileCustomer",
                table: "MobileOrderDetailDiscount",
                column: "PromoDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_MobileCustomer_MobileOrderDetailFreeGood_Code",
                schema: "MobileCustomer",
                table: "MobileOrderDetailFreeGood",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_MobileCustomer_MobileOrderDetailFreeGood_ItemId",
                schema: "MobileCustomer",
                table: "MobileOrderDetailFreeGood",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_MobileCustomer_MobileOrderDetailFreeGood_OrderDetailId",
                schema: "MobileCustomer",
                table: "MobileOrderDetailFreeGood",
                column: "OrderDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_MobileCustomer_MobileOrderDetailFreeGood_PromoCode",
                schema: "MobileCustomer",
                table: "MobileOrderDetailFreeGood",
                column: "PromoCode");

            migrationBuilder.CreateIndex(
                name: "IX_MobileCustomer_MobileOrderDetailFreeGood_UnitId",
                schema: "MobileCustomer",
                table: "MobileOrderDetailFreeGood",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_MobileCustomer_MobileOrderDetailFreeGood_UomId",
                schema: "MobileCustomer",
                table: "MobileOrderDetailFreeGood",
                column: "UomId");

            migrationBuilder.CreateIndex(
                name: "IX_MobileCustomer_MobileOrderHeader_CustCode",
                schema: "MobileCustomer",
                table: "MobileOrderHeader",
                column: "CustCode");

            migrationBuilder.CreateIndex(
                name: "IX_MobileCustomer_MobileOrderHeader_SalesOrderCode",
                schema: "MobileCustomer",
                table: "MobileOrderHeader",
                column: "SalesOrderCode");

            // Create view MobileCustomer.vwMobileOrderHeader
            var sql = @"CREATE VIEW [MobileCustomer].[vwMobileOrderHeader]
AS
    SELECT mo_h.*,
        c.[Name] AS CustName,
        u_c.Initial AS CreatedInitial,
        u_u.Initial AS UpdatedInitial,
        u_a.Initial AS ApprovedInitial,
        u_r.Initial AS RejectedInitial,
        CASE mo_h.Mark
            WHEN 'A' THEN 'Aktif'
            WHEN 'APR' THEN 'Disetujui'
            WHEN 'REJ' THEN 'Ditolak' END AS [Status]
    FROM MobileCustomer.MobileOrderHeader mo_h
    LEFT JOIN General.Customer c
        ON c.Code = mo_h.CustCode
    LEFT JOIN SystemManagement.[User] u_c
        ON u_c.Id = mo_h.CreatedBy
    LEFT JOIN SystemManagement.[User] u_u
        ON u_u.Id = mo_h.UpdatedBy
    LEFT JOIN SystemManagement.[User] u_a
        ON u_a.Id = mo_h.ApprovedBy
	LEFT JOIN SystemManagement.[User] u_r
        ON u_r.Id = mo_h.RejectedBy";
            migrationBuilder.Sql(sql);

            // Create view MobileCustomer.vwMobileOrderDetail
            sql = @"CREATE VIEW [MobileCustomer].[vwMobileOrderDetail]
AS
	SELECT mo_d.*,
		i.Initial AS ItemInitial,
		i.[Name] AS ItemName,
		i.UomSellId AS ItemUomSellId,
		uom_c_s.UnitEquivalent AS ItemUomSellName,
		i.SellPrice AS ItemSellPrice,
		uom.Initial AS UomInitial,
		uom_c.UnitEquivalent AS UnitName
	FROM MobileCustomer.MobileOrderDetail mo_d
	LEFT JOIN Inventory.Item i
		ON i.Id = mo_d.ItemId
	LEFT JOIN Inventory.UoMConversion uom_c_s
		ON uom_c_s.Id = i.UomSellId
	LEFT JOIN Inventory.UoM uom
		ON uom.Id = mo_d.UomId
	LEFT JOIN Inventory.UoMConversion uom_c
		ON uom_c.Id = mo_d.UnitId";
            migrationBuilder.Sql(sql);

            // Reorder table Sales.PromoHeader
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
CREATE TABLE Sales.Tmp_PromoHeader
	(
	Code varchar(17) NOT NULL,
	Name varchar(50) NOT NULL,
	StartDate date NOT NULL,
	EndDate date NOT NULL,
	ApplyTo smallint NOT NULL,
	CoaCost varchar(6) NOT NULL,
	[Content] varchar(MAX) NULL,
	Mark varchar(3) NOT NULL,
	CreatedBy int NOT NULL,
	CreatedDate datetime NOT NULL,
	UpdatedBy int NOT NULL,
	UpdatedDate datetime NOT NULL,
	ApprovedBy int NULL,
	ApprovedDate datetime NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE Sales.Tmp_PromoHeader SET (LOCK_ESCALATION = TABLE)
GO
IF EXISTS(SELECT * FROM Sales.PromoHeader)
	 EXEC('INSERT INTO Sales.Tmp_PromoHeader (Code, Name, StartDate, EndDate, ApplyTo, CoaCost, [Content], Mark, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, ApprovedBy, ApprovedDate)
		SELECT Code, Name, StartDate, EndDate, ApplyTo, CoaCost, [Content], Mark, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, ApprovedBy, ApprovedDate FROM Sales.PromoHeader WITH (HOLDLOCK TABLOCKX)')
GO
ALTER TABLE MobileCustomer.MobileOrderDetailDiscount
	DROP CONSTRAINT FK_MobileOrderDetailDiscount_PromoHeader_PromoCode
GO
ALTER TABLE MobileCustomer.MobileOrderDetailFreeGood
	DROP CONSTRAINT FK_MobileOrderDetailFreeGood_PromoHeader_PromoCode
GO
ALTER TABLE Sales.PromoSubject
	DROP CONSTRAINT FK_PromoSubject_PromoHeader_Code
GO
ALTER TABLE Sales.PromoDetail
	DROP CONSTRAINT FK_PromoDetail_PromoHeader_Code
GO
ALTER TABLE MobileSales.MobileOrderDetailDiscount
	DROP CONSTRAINT FK_MobileOrderDetailDiscount_PromoHeader_PromoCode
GO
ALTER TABLE MobileSales.MobileOrderDetailFreeGood
	DROP CONSTRAINT FK_MobileOrderDetailFreeGood_PromoHeader_PromoCode
GO
DROP TABLE Sales.PromoHeader
GO
EXECUTE sp_rename N'Sales.Tmp_PromoHeader', N'PromoHeader', 'OBJECT' 
GO
ALTER TABLE Sales.PromoHeader ADD CONSTRAINT
	PK_PromoHeader PRIMARY KEY CLUSTERED 
	(
	Code
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE MobileSales.MobileOrderDetailFreeGood ADD CONSTRAINT
	FK_MobileOrderDetailFreeGood_PromoHeader_PromoCode FOREIGN KEY
	(
	PromoCode
	) REFERENCES Sales.PromoHeader
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE MobileSales.MobileOrderDetailFreeGood SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE MobileSales.MobileOrderDetailDiscount ADD CONSTRAINT
	FK_MobileOrderDetailDiscount_PromoHeader_PromoCode FOREIGN KEY
	(
	PromoCode
	) REFERENCES Sales.PromoHeader
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE MobileSales.MobileOrderDetailDiscount SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Sales.PromoDetail ADD CONSTRAINT
	FK_PromoDetail_PromoHeader_Code FOREIGN KEY
	(
	Code
	) REFERENCES Sales.PromoHeader
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Sales.PromoDetail SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Sales.PromoSubject ADD CONSTRAINT
	FK_PromoSubject_PromoHeader_Code FOREIGN KEY
	(
	Code
	) REFERENCES Sales.PromoHeader
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
ALTER TABLE MobileCustomer.MobileOrderDetailFreeGood ADD CONSTRAINT
	FK_MobileOrderDetailFreeGood_PromoHeader_PromoCode FOREIGN KEY
	(
	PromoCode
	) REFERENCES Sales.PromoHeader
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE MobileCustomer.MobileOrderDetailFreeGood SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE MobileCustomer.MobileOrderDetailDiscount ADD CONSTRAINT
	FK_MobileOrderDetailDiscount_PromoHeader_PromoCode FOREIGN KEY
	(
	PromoCode
	) REFERENCES Sales.PromoHeader
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE MobileCustomer.MobileOrderDetailDiscount SET (LOCK_ESCALATION = TABLE)
GO
COMMIT";
            migrationBuilder.Sql(sql);

            // Refresh view Sales.vwPromoHeader
            sql = @"exec sp_refreshview 'Sales.vwPromoHeader'";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MobileOrderDetailDiscount",
                schema: "MobileCustomer");

            migrationBuilder.DropTable(
                name: "MobileOrderDetailFreeGood",
                schema: "MobileCustomer");

            migrationBuilder.DropTable(
                name: "MobileOrderDetail",
                schema: "MobileCustomer");

            migrationBuilder.DropTable(
                name: "MobileOrderHeader",
                schema: "MobileCustomer");

            migrationBuilder.DropColumn(
                name: "Content",
                schema: "Sales",
                table: "PromoHeader");

            // Drop view MobileCustomer.vwMobileOrderHeader
            var sql = @"DROP VIEW [MobileCustomer].[vwMobileOrderHeader]";
            migrationBuilder.Sql(sql);

            // Drop view MobileCustomer.vwMobileOrderDetail
            sql = @"DROP VIEW [MobileCustomer].[vwMobileOrderDetail]";
            migrationBuilder.Sql(sql);
        }
    }
}
