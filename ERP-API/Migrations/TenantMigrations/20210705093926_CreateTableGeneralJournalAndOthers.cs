using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Web.API.Migrations.TenantMigrations
{
    public partial class CreateTableGeneralJournalAndOthers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PaidAmount",
                schema: "Sales",
                table: "SalesDeliveryHeader",
                type: "decimal(19,6)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PaidAmount",
                schema: "Purchasing",
                table: "PurchaseReceiveHeader",
                type: "decimal(19,6)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "GeneralJournalDetail",
                schema: "Accounting",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    CoaCode = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: false),
                    Notes = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    Type = table.Column<string>(type: "varchar(1)", unicode: false, maxLength: 1, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneralJournalDetail", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GeneralJournalHeader",
                schema: "Accounting",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    CurrCode = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
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
                    table.PrimaryKey("PK_GeneralJournalHeader", x => x.Code);
                    table.ForeignKey(
                        name: "FK_GeneralJournalHeader_Currency_CurrCode",
                        column: x => x.CurrCode,
                        principalSchema: "General",
                        principalTable: "Currency",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "Journal",
                schema: "Accounting",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    CoaCode = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: false),
                    TypeCode = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: false),
                    RefCode1 = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    RefCode2 = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    RefCode3 = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    RefCode4 = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    Group = table.Column<short>(type: "smallint", nullable: false),
                    CurrCode = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    Period = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    CustomRate = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    Type = table.Column<string>(type: "varchar(1)", unicode: false, maxLength: 1, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    SrcTrans = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Journal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Journal_Currency_CurrCode",
                        column: x => x.CurrCode,
                        principalSchema: "General",
                        principalTable: "Currency",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateIndex(
                name: "IX_GeneralJournalDetail_CoaCode",
                schema: "Accounting",
                table: "GeneralJournalDetail",
                column: "CoaCode");

            migrationBuilder.CreateIndex(
                name: "IX_GeneralJournalHeader_CurrCode",
                schema: "Accounting",
                table: "GeneralJournalHeader",
                column: "CurrCode");

            migrationBuilder.CreateIndex(
                name: "IX_Journal_CoaCode",
                schema: "Accounting",
                table: "Journal",
                column: "CoaCode");

            migrationBuilder.CreateIndex(
                name: "IX_Journal_Code",
                schema: "Accounting",
                table: "Journal",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_Journal_CurrCode",
                schema: "Accounting",
                table: "Journal",
                column: "CurrCode");

            migrationBuilder.CreateIndex(
                name: "IX_Journal_Period",
                schema: "Accounting",
                table: "Journal",
                column: "Period");

            migrationBuilder.CreateIndex(
                name: "IX_Journal_RefCode1",
                schema: "Accounting",
                table: "Journal",
                column: "RefCode1");

            migrationBuilder.CreateIndex(
                name: "IX_Journal_RefCode2",
                schema: "Accounting",
                table: "Journal",
                column: "RefCode2");

            migrationBuilder.CreateIndex(
                name: "IX_Journal_RefCode3",
                schema: "Accounting",
                table: "Journal",
                column: "RefCode3");

            migrationBuilder.CreateIndex(
                name: "IX_Journal_RefCode4",
                schema: "Accounting",
                table: "Journal",
                column: "RefCode4");

            migrationBuilder.CreateIndex(
                name: "IX_Journal_SrcTrans",
                schema: "Accounting",
                table: "Journal",
                column: "SrcTrans");

            migrationBuilder.CreateIndex(
                name: "IX_Journal_TypeCode",
                schema: "Accounting",
                table: "Journal",
                column: "TypeCode");

            // Reorder column in Purchasing.PurchaseReceiveHeader
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
CREATE TABLE Purchasing.Tmp_PurchaseReceiveHeader
	(
	Code varchar(17) NOT NULL,
	Date date NOT NULL,
	TransCode varchar(17) NOT NULL,
	RefNo varchar(30) NULL,
	SrcTrans smallint NOT NULL,
	SupCode varchar(8) NOT NULL,
	ReceiveBy bigint NOT NULL,
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
	PaidAmount decimal(19, 6) NOT NULL,
	Mark varchar(3) NOT NULL,
	CreatedBy int NOT NULL,
	CreatedDate datetime NOT NULL,
	UpdatedBy int NOT NULL,
	UpdatedDate datetime NOT NULL,
	ApprovedBy int NULL,
	ApprovedDate datetime NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Purchasing.Tmp_PurchaseReceiveHeader SET (LOCK_ESCALATION = TABLE)
GO
IF EXISTS(SELECT * FROM Purchasing.PurchaseReceiveHeader)
	 EXEC('INSERT INTO Purchasing.Tmp_PurchaseReceiveHeader (Code, Date, TransCode, RefNo, SrcTrans, SupCode, ReceiveBy, CurrCode, Rate, ShipmentFee, HandlingFee, SubTotal, FinalDiscPercent, FinalDisc, IncludeTax, TaxAmount, Total, DPP, PaidAmount, Mark, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, ApprovedBy, ApprovedDate)
		SELECT Code, Date, TransCode, RefNo, SrcTrans, SupCode, ReceiveBy, CurrCode, Rate, ShipmentFee, HandlingFee, SubTotal, FinalDiscPercent, FinalDisc, IncludeTax, TaxAmount, Total, DPP, PaidAmount, Mark, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, ApprovedBy, ApprovedDate FROM Purchasing.PurchaseReceiveHeader WITH (HOLDLOCK TABLOCKX)')
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
CREATE NONCLUSTERED INDEX IX_PurchaseReceiveHeader_TransCode ON Purchasing.PurchaseReceiveHeader
	(
	TransCode
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
COMMIT";
            migrationBuilder.Sql(sql);

            // Reorder column in Sales.SalesDeliveryHeader
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
CREATE TABLE Sales.Tmp_SalesDeliveryHeader
	(
	Code varchar(17) NOT NULL,
	Date date NOT NULL,
	SrcTrans smallint NOT NULL,
	TransCode varchar(17) NOT NULL,
	CustCode varchar(8) NOT NULL,
	WarehouseCode varchar(8) NOT NULL,
	ShippedBy bigint NOT NULL,
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
	PaidAmount decimal(19, 6) NOT NULL,
	Notes varchar(256) NULL,
	FromDirectInvoice bit NOT NULL,
	Mark varchar(3) NOT NULL,
	CreatedBy int NOT NULL,
	CreatedDate datetime NOT NULL,
	UpdatedBy int NOT NULL,
	UpdatedDate datetime NOT NULL,
	ApprovedBy int NULL,
	ApprovedDate datetime NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Sales.Tmp_SalesDeliveryHeader SET (LOCK_ESCALATION = TABLE)
GO
IF EXISTS(SELECT * FROM Sales.SalesDeliveryHeader)
	 EXEC('INSERT INTO Sales.Tmp_SalesDeliveryHeader (Code, Date, SrcTrans, TransCode, CustCode, WarehouseCode, ShippedBy, CurrCode, Rate, ShipmentFee, HandlingFee, SubTotal, FinalDiscPercent, FinalDisc, IncludeTax, TaxAmount, Total, DPP, PaidAmount, Notes, FromDirectInvoice, Mark, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, ApprovedBy, ApprovedDate)
		SELECT Code, Date, SrcTrans, TransCode, CustCode, WarehouseCode, ShippedBy, CurrCode, Rate, ShipmentFee, HandlingFee, SubTotal, FinalDiscPercent, FinalDisc, IncludeTax, TaxAmount, Total, DPP, PaidAmount, Notes, FromDirectInvoice, Mark, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, ApprovedBy, ApprovedDate FROM Sales.SalesDeliveryHeader WITH (HOLDLOCK TABLOCKX)')
GO
DROP TABLE Sales.SalesDeliveryHeader
GO
EXECUTE sp_rename N'Sales.Tmp_SalesDeliveryHeader', N'SalesDeliveryHeader', 'OBJECT' 
GO
ALTER TABLE Sales.SalesDeliveryHeader ADD CONSTRAINT
	PK_SalesDeliveryHeader PRIMARY KEY CLUSTERED 
	(
	Code
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX IX_SalesDeliveryHeader_TransCode ON Sales.SalesDeliveryHeader
	(
	TransCode
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
COMMIT";
            migrationBuilder.Sql(sql);

            // Refresh view Purchasing.vwPurchaseReceiveHeader
            sql = @"EXECUTE sp_refreshview 'Purchasing.vwPurchaseReceiveHeader'";
            migrationBuilder.Sql(sql);

            // Refresh view Sales.SalesDeliveryHeader
            sql = @"EXECUTE sp_refreshview 'Sales.vwSalesDeliveryHeader'";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GeneralJournalDetail",
                schema: "Accounting");

            migrationBuilder.DropTable(
                name: "GeneralJournalHeader",
                schema: "Accounting");

            migrationBuilder.DropTable(
                name: "Journal",
                schema: "Accounting");

            migrationBuilder.DropColumn(
                name: "PaidAmount",
                schema: "Sales",
                table: "SalesDeliveryHeader");

            migrationBuilder.DropColumn(
                name: "PaidAmount",
                schema: "Purchasing",
                table: "PurchaseReceiveHeader");
        }
    }
}
