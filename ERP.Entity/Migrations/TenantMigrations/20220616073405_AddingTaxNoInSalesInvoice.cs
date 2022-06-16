using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AddingTaxNoInSalesInvoice : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "TaxInvoiceDate",
                schema: "Sales",
                table: "SalesInvoiceHeader",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxInvoiceNo",
                schema: "Sales",
                table: "SalesInvoiceHeader",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true);

            // Reorder column Sales.SalesInvoiceHeader
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
ALTER TABLE Sales.SalesInvoiceHeader
	DROP CONSTRAINT FK_SalesInvoiceHeader_SalesOrderHeader_SOCode
GO
ALTER TABLE Sales.SalesOrderHeader SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Sales.SalesInvoiceHeader
	DROP CONSTRAINT FK_SalesInvoiceHeader_Customer_CustCode
GO
ALTER TABLE General.Customer SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Sales.SalesInvoiceHeader
	DROP CONSTRAINT FK_SalesInvoiceHeader_Currency_CurrCode
GO
ALTER TABLE General.Currency SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE Sales.Tmp_SalesInvoiceHeader
	(
	Code varchar(17) NOT NULL,
	Date date NOT NULL,
	DueDate date NOT NULL,
	SOCode varchar(17) NULL,
	CustCode varchar(8) NOT NULL,
	CurrCode varchar(3) NOT NULL,
	PaidAmount decimal(19, 6) NOT NULL,
	Total decimal(18, 2) NOT NULL,
	TaxInvoiceNo varchar(50) NULL,
	TaxInvoiceDate date NULL,
	Notes varchar(256) NULL,
	FromDirectInvoice bit NOT NULL,
	Mark varchar(3) NOT NULL,
	CreatedBy int NOT NULL,
	CreatedDate datetime NOT NULL,
	UpdatedBy int NOT NULL,
	UpdatedDate datetime NOT NULL,
	ApprovedBy int NULL,
	ApprovedDate datetime NULL,
	ViewedBy int NULL,
	ViewedDate datetime NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Sales.Tmp_SalesInvoiceHeader SET (LOCK_ESCALATION = TABLE)
GO
IF EXISTS(SELECT * FROM Sales.SalesInvoiceHeader)
	 EXEC('INSERT INTO Sales.Tmp_SalesInvoiceHeader (Code, Date, DueDate, SOCode, CustCode, CurrCode, PaidAmount, Total, TaxInvoiceNo, TaxInvoiceDate, Notes, FromDirectInvoice, Mark, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, ApprovedBy, ApprovedDate, ViewedBy, ViewedDate)
		SELECT Code, Date, DueDate, SOCode, CustCode, CurrCode, PaidAmount, Total, TaxInvoiceNo, TaxInvoiceDate, Notes, FromDirectInvoice, Mark, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, ApprovedBy, ApprovedDate, ViewedBy, ViewedDate FROM Sales.SalesInvoiceHeader WITH (HOLDLOCK TABLOCKX)')
GO
ALTER TABLE Sales.SalesInvoiceDetail
	DROP CONSTRAINT FK_SalesInvoiceDetail_SalesInvoiceHeader_Code
GO
ALTER TABLE Sales.SalesInvoiceCreditMemo
	DROP CONSTRAINT FK_SalesInvoiceCreditMemo_SalesInvoiceHeader_InvCode
GO
DROP TABLE Sales.SalesInvoiceHeader
GO
EXECUTE sp_rename N'Sales.Tmp_SalesInvoiceHeader', N'SalesInvoiceHeader', 'OBJECT' 
GO
ALTER TABLE Sales.SalesInvoiceHeader ADD CONSTRAINT
	PK_SalesInvoiceHeader PRIMARY KEY CLUSTERED 
	(
	Code
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX IX_SalesInvoiceHeader_CurrCode ON Sales.SalesInvoiceHeader
	(
	CurrCode
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_SalesInvoiceHeader_CustCode ON Sales.SalesInvoiceHeader
	(
	CustCode
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_SalesInvoiceHeader_SOCode ON Sales.SalesInvoiceHeader
	(
	SOCode
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE Sales.SalesInvoiceHeader ADD CONSTRAINT
	FK_SalesInvoiceHeader_Currency_CurrCode FOREIGN KEY
	(
	CurrCode
	) REFERENCES General.Currency
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
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
ALTER TABLE Sales.SalesInvoiceHeader ADD CONSTRAINT
	FK_SalesInvoiceHeader_SalesOrderHeader_SOCode FOREIGN KEY
	(
	SOCode
	) REFERENCES Sales.SalesOrderHeader
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
COMMIT
BEGIN TRANSACTION
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
ALTER TABLE Sales.SalesInvoiceCreditMemo SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Sales.SalesInvoiceDetail ADD CONSTRAINT
	FK_SalesInvoiceDetail_SalesInvoiceHeader_Code FOREIGN KEY
	(
	Code
	) REFERENCES Sales.SalesInvoiceHeader
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Sales.SalesInvoiceDetail SET (LOCK_ESCALATION = TABLE)
GO
COMMIT";
            migrationBuilder.Sql(sql);

            // Refresh view Sales.vwSalesInvoiceHeader
            sql = @"EXEC sp_refreshview 'Sales.vwSalesInvoiceHeader'";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TaxInvoiceDate",
                schema: "Sales",
                table: "SalesInvoiceHeader");

            migrationBuilder.DropColumn(
                name: "TaxInvoiceNo",
                schema: "Sales",
                table: "SalesInvoiceHeader");
        }
    }
}
