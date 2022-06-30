using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AddingColumnForSalesDownPayment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalesInvoiceCreditMemo_CreditMemo_CreditMemoCode",
                schema: "Sales",
                table: "SalesInvoiceCreditMemo");

            migrationBuilder.AlterColumn<decimal>(
                name: "CreditMemoAmount",
                schema: "Sales",
                table: "SalesInvoiceCreditMemo",
                type: "decimal(19,6)",
                precision: 19,
                scale: 6,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AddColumn<string>(
                name: "Src",
                schema: "Sales",
                table: "SalesInvoiceCreditMemo",
                type: "varchar(5)",
                unicode: false,
                maxLength: 5,
                nullable: false,
                defaultValue: "CM");

            migrationBuilder.AddColumn<decimal>(
                name: "CreditMemoTaxAmount",
                schema: "Sales",
                table: "SalesInvoiceCreditMemo",
                type: "decimal(19,6)",
                precision: 19,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "Used",
                schema: "Sales",
                table: "CreditMemo",
                type: "decimal(19,6)",
                precision: 19,
                scale: 6,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                schema: "Sales",
                table: "CreditMemo",
                type: "decimal(19,6)",
                precision: 19,
                scale: 6,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AddColumn<bool>(
                name: "IncludeTax",
                schema: "Sales",
                table: "CreditMemo",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxAmount",
                schema: "Sales",
                table: "CreditMemo",
                type: "decimal(19,6)",
                precision: 19,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "TaxId",
                schema: "Sales",
                table: "CreditMemo",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Total",
                schema: "Sales",
                table: "CreditMemo",
                type: "decimal(19,6)",
                precision: 19,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_CreditMemo_TaxId",
                schema: "Sales",
                table: "CreditMemo",
                column: "TaxId");

            migrationBuilder.AddForeignKey(
                name: "FK_CreditMemo_Tax_TaxId",
                schema: "Sales",
                table: "CreditMemo",
                column: "TaxId",
                principalSchema: "General",
                principalTable: "Tax",
                principalColumn: "Id");

            // Update total Sales.CreditMemo
            var sql = @"UPDATE Sales.CreditMemo
SET Total = Amount";
            migrationBuilder.Sql(sql);

            // Reorder column Sales.CreditMemo
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
ALTER TABLE Sales.CreditMemo
	DROP CONSTRAINT FK_CreditMemo_Currency_CurrCode
GO
ALTER TABLE General.Currency SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Sales.CreditMemo
	DROP CONSTRAINT FK_CreditMemo_Tax_TaxId
GO
ALTER TABLE General.Tax SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Sales.CreditMemo
	DROP CONSTRAINT FK_CreditMemo_Customer_CustCode
GO
ALTER TABLE General.Customer SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE Sales.Tmp_CreditMemo
	(
	Code varchar(17) NOT NULL,
	Date date NOT NULL,
	SrcTrans smallint NOT NULL,
	CustCode varchar(8) NOT NULL,
	TransCode varchar(17) NULL,
	CurrCode varchar(3) NOT NULL,
	Rate decimal(18, 2) NOT NULL,
	Amount decimal(19, 6) NOT NULL,
	IncludeTax bit NOT NULL,
	TaxId int NULL,
	TaxAmount decimal(19, 6) NOT NULL,
	Total decimal(19, 6) NOT NULL,
	Used decimal(19, 6) NOT NULL,
	Notes varchar(256) NULL,
	Mark varchar(3) NOT NULL,
	CreatedBy int NOT NULL,
	CreatedDate datetime NOT NULL,
	UpdatedBy int NOT NULL,
	UpdatedDate datetime NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Sales.Tmp_CreditMemo SET (LOCK_ESCALATION = TABLE)
GO
IF EXISTS(SELECT * FROM Sales.CreditMemo)
	 EXEC('INSERT INTO Sales.Tmp_CreditMemo (Code, Date, SrcTrans, CustCode, TransCode, CurrCode, Rate, Amount, IncludeTax, TaxId, TaxAmount, Total, Used, Notes, Mark, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate)
		SELECT Code, Date, SrcTrans, CustCode, TransCode, CurrCode, Rate, Amount, IncludeTax, TaxId, TaxAmount, Total, Used, Notes, Mark, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate FROM Sales.CreditMemo WITH (HOLDLOCK TABLOCKX)')
GO
DROP TABLE Sales.CreditMemo
GO
EXECUTE sp_rename N'Sales.Tmp_CreditMemo', N'CreditMemo', 'OBJECT' 
GO
ALTER TABLE Sales.CreditMemo ADD CONSTRAINT
	PK_CreditMemo PRIMARY KEY CLUSTERED 
	(
	Code
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX IX_CreditMemo_CurrCode ON Sales.CreditMemo
	(
	CurrCode
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_CreditMemo_CustCode ON Sales.CreditMemo
	(
	CustCode
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_CreditMemo_TransCode ON Sales.CreditMemo
	(
	TransCode
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_CreditMemo_TaxId ON Sales.CreditMemo
	(
	TaxId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
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
ALTER TABLE Sales.CreditMemo ADD CONSTRAINT
	FK_CreditMemo_Tax_TaxId FOREIGN KEY
	(
	TaxId
	) REFERENCES General.Tax
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Sales.CreditMemo ADD CONSTRAINT
	FK_CreditMemo_Currency_CurrCode FOREIGN KEY
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

            // Reorder column Sales.SalesInvoiceCreditMemo
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
ALTER TABLE Sales.SalesInvoiceCreditMemo
	DROP CONSTRAINT FK_SalesInvoiceCreditMemo_SalesInvoiceHeader_InvCode
GO
ALTER TABLE Sales.SalesInvoiceHeader SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE Sales.Tmp_SalesInvoiceCreditMemo
	(
	Id bigint NOT NULL IDENTITY (1, 1),
	InvCode varchar(17) NOT NULL,
	CreditMemoCode varchar(17) NOT NULL,
	InvAmount decimal(18, 2) NOT NULL,
	CreditMemoAmount decimal(19, 6) NOT NULL,
	CreditMemoTaxAmount decimal(19, 6) NOT NULL,
	Src varchar(5) NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Sales.Tmp_SalesInvoiceCreditMemo SET (LOCK_ESCALATION = TABLE)
GO
SET IDENTITY_INSERT Sales.Tmp_SalesInvoiceCreditMemo ON
GO
IF EXISTS(SELECT * FROM Sales.SalesInvoiceCreditMemo)
	 EXEC('INSERT INTO Sales.Tmp_SalesInvoiceCreditMemo (Id, InvCode, CreditMemoCode, InvAmount, CreditMemoAmount, CreditMemoTaxAmount, Src)
		SELECT Id, InvCode, CreditMemoCode, InvAmount, CreditMemoAmount, CreditMemoTaxAmount, Src FROM Sales.SalesInvoiceCreditMemo WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT Sales.Tmp_SalesInvoiceCreditMemo OFF
GO
DROP TABLE Sales.SalesInvoiceCreditMemo
GO
EXECUTE sp_rename N'Sales.Tmp_SalesInvoiceCreditMemo', N'SalesInvoiceCreditMemo', 'OBJECT' 
GO
ALTER TABLE Sales.SalesInvoiceCreditMemo ADD CONSTRAINT
	PK_SalesInvoiceCreditMemo PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX IX_SalesInvoiceCreditMemo_CreditMemoCode ON Sales.SalesInvoiceCreditMemo
	(
	CreditMemoCode
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_SalesInvoiceCreditMemo_InvCode ON Sales.SalesInvoiceCreditMemo
	(
	InvCode
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE Sales.SalesInvoiceCreditMemo ADD CONSTRAINT
	FK_SalesInvoiceCreditMemo_SalesInvoiceHeader_InvCode FOREIGN KEY
	(
	InvCode
	) REFERENCES Sales.SalesInvoiceHeader
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
COMMIT";
            migrationBuilder.Sql(sql);

            // Alter view Sales.vwCreditMemo
            sql = @"ALTER VIEW [Sales].[vwCreditMemo]
AS
	SELECT m.*,
	m.Amount - m.Used AS Remaining,
	c.Initial AS CustInitial,
	c.[Name] AS CustName,
	u_c.Initial AS CreatedInitial,
    u_u.Initial AS UpdatedInitial,
	CASE m.SrcTrans
		WHEN 1 THEN 'Deposit'
		WHEN 2 THEN 'Retur'
		WHEN 3 THEN 'Uang Muka'
		WHEN 4 THEN 'Retur Uang Muka' END AS SrcTransName,
	CASE m.Mark
		WHEN 'A' THEN 'Active'
		WHEN 'V' THEN 'Void'
		WHEN 'PP' THEN 'Pending Payment'
		WHEN 'PU' THEN 'Partial Used'
		WHEN 'FU' THEN 'Full Used' END AS [Status]
FROM Sales.CreditMemo m
LEFT JOIN General.Customer c
	ON c.Code = m.CustCode
LEFT JOIN SystemManagement.[User] u_c
	ON u_c.Id = m.CreatedBy
LEFT JOIN SystemManagement.[User] u_u
    ON u_u.Id = m.UpdatedBy";
            migrationBuilder.Sql(sql);

            // Disabling constraints foreign key FK_CreditMemo_Tax_TaxId
            sql = @"ALTER TABLE [Sales].[CreditMemo] NOCHECK CONSTRAINT [FK_CreditMemo_Tax_TaxId]";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CreditMemo_Tax_TaxId",
                schema: "Sales",
                table: "CreditMemo");

            migrationBuilder.DropIndex(
                name: "IX_CreditMemo_TaxId",
                schema: "Sales",
                table: "CreditMemo");

            migrationBuilder.DropColumn(
                name: "Src",
                schema: "Sales",
                table: "SalesInvoiceCreditMemo");

            migrationBuilder.DropColumn(
                name: "CreditMemoTaxAmount",
                schema: "Sales",
                table: "SalesInvoiceCreditMemo");

            migrationBuilder.DropColumn(
                name: "IncludeTax",
                schema: "Sales",
                table: "CreditMemo");

            migrationBuilder.DropColumn(
                name: "TaxAmount",
                schema: "Sales",
                table: "CreditMemo");

            migrationBuilder.DropColumn(
                name: "TaxId",
                schema: "Sales",
                table: "CreditMemo");

            migrationBuilder.DropColumn(
                name: "Total",
                schema: "Sales",
                table: "CreditMemo");

            migrationBuilder.AlterColumn<decimal>(
                name: "CreditMemoAmount",
                schema: "Sales",
                table: "SalesInvoiceCreditMemo",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(19,6)",
                oldPrecision: 19,
                oldScale: 6);

            migrationBuilder.AlterColumn<decimal>(
                name: "Used",
                schema: "Sales",
                table: "CreditMemo",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(19,6)",
                oldPrecision: 19,
                oldScale: 6);

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                schema: "Sales",
                table: "CreditMemo",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(19,6)",
                oldPrecision: 19,
                oldScale: 6);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesInvoiceCreditMemo_CreditMemo_CreditMemoCode",
                schema: "Sales",
                table: "SalesInvoiceCreditMemo",
                column: "CreditMemoCode",
                principalSchema: "Sales",
                principalTable: "CreditMemo",
                principalColumn: "Code");
        }
    }
}
