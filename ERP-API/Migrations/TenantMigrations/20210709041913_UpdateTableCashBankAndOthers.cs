using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP_API.Migrations.TenantMigrations
{
    public partial class UpdateTableCashBankAndOthers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "PaidAmount",
                schema: "Sales",
                table: "SalesInvoiceHeader",
                type: "decimal(19,6)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "PaidAmount",
                schema: "Purchasing",
                table: "PurchaseInvoiceHeader",
                type: "decimal(19,6)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<bool>(
                name: "IsInterCashBank",
                schema: "Finance",
                table: "GeneralCashBankHeader",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "PaidAmount",
                schema: "Accounting",
                table: "BeginningBalanceAR",
                type: "decimal(19,6)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PaidAmount",
                schema: "Accounting",
                table: "BeginningBalanceAP",
                type: "decimal(19,6)",
                nullable: false,
                defaultValue: 0m);

            // Reorder column Accounting.BeginningBalanceAP
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
ALTER TABLE Accounting.BeginningBalanceAP
	DROP CONSTRAINT FK_BeginningBalanceAP_Supplier_SupCode
GO
ALTER TABLE General.Supplier SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Accounting.BeginningBalanceAP
	DROP CONSTRAINT FK_BeginningBalanceAP_Currency_CurrCode
GO
ALTER TABLE General.Currency SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE Accounting.Tmp_BeginningBalanceAP
	(
	Id bigint NOT NULL IDENTITY (1, 1),
	Code varchar(17) NOT NULL,
	Date date NOT NULL,
	DueDate date NOT NULL,
	SupCode varchar(8) NOT NULL,
	CurrCode varchar(3) NOT NULL,
	CurrRate decimal(19, 6) NOT NULL,
	Amount decimal(18, 2) NOT NULL,
	PaidAmount decimal(19, 6) NOT NULL,
	Notes varchar(256) NULL,
	IsActive bit NOT NULL,
	CreatedBy int NOT NULL,
	CreatedDate datetime NOT NULL,
	UpdatedBy int NOT NULL,
	UpdatedDate datetime NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Accounting.Tmp_BeginningBalanceAP SET (LOCK_ESCALATION = TABLE)
GO
SET IDENTITY_INSERT Accounting.Tmp_BeginningBalanceAP ON
GO
IF EXISTS(SELECT * FROM Accounting.BeginningBalanceAP)
	 EXEC('INSERT INTO Accounting.Tmp_BeginningBalanceAP (Id, Code, Date, DueDate, SupCode, CurrCode, CurrRate, Amount, PaidAmount, Notes, IsActive, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate)
		SELECT Id, Code, Date, DueDate, SupCode, CurrCode, CurrRate, Amount, PaidAmount, Notes, IsActive, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate FROM Accounting.BeginningBalanceAP WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT Accounting.Tmp_BeginningBalanceAP OFF
GO
DROP TABLE Accounting.BeginningBalanceAP
GO
EXECUTE sp_rename N'Accounting.Tmp_BeginningBalanceAP', N'BeginningBalanceAP', 'OBJECT' 
GO
ALTER TABLE Accounting.BeginningBalanceAP ADD CONSTRAINT
	PK_BeginningBalanceAP PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE UNIQUE NONCLUSTERED INDEX IX_BeginningBalanceAP_Code ON Accounting.BeginningBalanceAP
	(
	Code
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_BeginningBalanceAP_CurrCode ON Accounting.BeginningBalanceAP
	(
	CurrCode
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_BeginningBalanceAP_SupCode ON Accounting.BeginningBalanceAP
	(
	SupCode
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE Accounting.BeginningBalanceAP ADD CONSTRAINT
	FK_BeginningBalanceAP_Currency_CurrCode FOREIGN KEY
	(
	CurrCode
	) REFERENCES General.Currency
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Accounting.BeginningBalanceAP ADD CONSTRAINT
	FK_BeginningBalanceAP_Supplier_SupCode FOREIGN KEY
	(
	SupCode
	) REFERENCES General.Supplier
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
COMMIT";
            migrationBuilder.Sql(sql);

            // Reorder column Accounting.BeginningBalanceAR
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
ALTER TABLE Accounting.BeginningBalanceAR
	DROP CONSTRAINT FK_BeginningBalanceAR_Customer_CustCode
GO
ALTER TABLE General.Customer SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Accounting.BeginningBalanceAR
	DROP CONSTRAINT FK_BeginningBalanceAR_Currency_CurrCode
GO
ALTER TABLE General.Currency SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE Accounting.Tmp_BeginningBalanceAR
	(
	Id bigint NOT NULL IDENTITY (1, 1),
	Code varchar(17) NOT NULL,
	Date date NOT NULL,
	DueDate date NOT NULL,
	CustCode varchar(8) NOT NULL,
	CurrCode varchar(3) NOT NULL,
	CurrRate decimal(19, 6) NOT NULL,
	Amount decimal(18, 2) NOT NULL,
	PaidAmount decimal(19, 6) NOT NULL,
	Notes varchar(256) NULL,
	IsActive bit NOT NULL,
	CreatedBy int NOT NULL,
	CreatedDate datetime NOT NULL,
	UpdatedBy int NOT NULL,
	UpdatedDate datetime NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Accounting.Tmp_BeginningBalanceAR SET (LOCK_ESCALATION = TABLE)
GO
SET IDENTITY_INSERT Accounting.Tmp_BeginningBalanceAR ON
GO
IF EXISTS(SELECT * FROM Accounting.BeginningBalanceAR)
	 EXEC('INSERT INTO Accounting.Tmp_BeginningBalanceAR (Id, Code, Date, DueDate, CustCode, CurrCode, CurrRate, Amount, PaidAmount, Notes, IsActive, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate)
		SELECT Id, Code, Date, DueDate, CustCode, CurrCode, CurrRate, Amount, PaidAmount, Notes, IsActive, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate FROM Accounting.BeginningBalanceAR WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT Accounting.Tmp_BeginningBalanceAR OFF
GO
DROP TABLE Accounting.BeginningBalanceAR
GO
EXECUTE sp_rename N'Accounting.Tmp_BeginningBalanceAR', N'BeginningBalanceAR', 'OBJECT' 
GO
ALTER TABLE Accounting.BeginningBalanceAR ADD CONSTRAINT
	PK_BeginningBalanceAR PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE UNIQUE NONCLUSTERED INDEX IX_BeginningBalanceAR_Code ON Accounting.BeginningBalanceAR
	(
	Code
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_BeginningBalanceAR_CurrCode ON Accounting.BeginningBalanceAR
	(
	CurrCode
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_BeginningBalanceAR_CustCode ON Accounting.BeginningBalanceAR
	(
	CustCode
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE Accounting.BeginningBalanceAR ADD CONSTRAINT
	FK_BeginningBalanceAR_Currency_CurrCode FOREIGN KEY
	(
	CurrCode
	) REFERENCES General.Currency
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
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
COMMIT";
            migrationBuilder.Sql(sql);

			// Reorder column Finance.GeneralCashBankHeader
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
ALTER TABLE Finance.GeneralCashBankHeader
	DROP CONSTRAINT FK_GeneralCashBankHeader_Currency_CurrCode
GO
ALTER TABLE General.Currency SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE Finance.Tmp_GeneralCashBankHeader
	(
	Code varchar(17) NOT NULL,
	VouCode varchar(17) NULL,
	Type varchar(1) NOT NULL,
	Date date NOT NULL,
	CoaCode varchar(6) NOT NULL,
	CurrCode varchar(3) NOT NULL,
	Rate decimal(19, 6) NOT NULL,
	Amount decimal(18, 2) NOT NULL,
	ChequeNo varchar(25) NULL,
	ChequeDate date NULL,
	Notes varchar(256) NULL,
	IsInterCashBank bit NOT NULL,
	Mark varchar(3) NOT NULL,
	CreatedBy int NOT NULL,
	CreatedDate datetime NOT NULL,
	UpdatedBy int NOT NULL,
	UpdatedDate datetime NOT NULL,
	ApprovedBy int NULL,
	ApprovedDate datetime NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Finance.Tmp_GeneralCashBankHeader SET (LOCK_ESCALATION = TABLE)
GO
IF EXISTS(SELECT * FROM Finance.GeneralCashBankHeader)
	 EXEC('INSERT INTO Finance.Tmp_GeneralCashBankHeader (Code, VouCode, Type, Date, CoaCode, CurrCode, Rate, Amount, ChequeNo, ChequeDate, Notes, IsInterCashBank, Mark, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, ApprovedBy, ApprovedDate)
		SELECT Code, VouCode, Type, Date, CoaCode, CurrCode, Rate, Amount, ChequeNo, ChequeDate, Notes, IsInterCashBank, Mark, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, ApprovedBy, ApprovedDate FROM Finance.GeneralCashBankHeader WITH (HOLDLOCK TABLOCKX)')
GO
ALTER TABLE Finance.GeneralCashBankDetail
	DROP CONSTRAINT FK_GeneralCashBankDetail_GeneralCashBankHeader_Code
GO
DROP TABLE Finance.GeneralCashBankHeader
GO
EXECUTE sp_rename N'Finance.Tmp_GeneralCashBankHeader', N'GeneralCashBankHeader', 'OBJECT' 
GO
ALTER TABLE Finance.GeneralCashBankHeader ADD CONSTRAINT
	PK_GeneralCashBankHeader PRIMARY KEY CLUSTERED 
	(
	Code
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX IX_GeneralCashBankHeader_CoaCode ON Finance.GeneralCashBankHeader
	(
	CoaCode
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_GeneralCashBankHeader_CurrCode ON Finance.GeneralCashBankHeader
	(
	CurrCode
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE Finance.GeneralCashBankHeader ADD CONSTRAINT
	FK_GeneralCashBankHeader_Currency_CurrCode FOREIGN KEY
	(
	CurrCode
	) REFERENCES General.Currency
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Finance.GeneralCashBankDetail ADD CONSTRAINT
	FK_GeneralCashBankDetail_GeneralCashBankHeader_Code FOREIGN KEY
	(
	Code
	) REFERENCES Finance.GeneralCashBankHeader
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Finance.GeneralCashBankDetail SET (LOCK_ESCALATION = TABLE)
GO
COMMIT";
            migrationBuilder.Sql(sql);
            
            // Refresh view
            sql = @"execute sp_refreshview 'Purchasing.vwPurchaseInvoiceHeader';
execute sp_refreshview 'Sales.vwSalesInvoiceHeader';
execute sp_refreshview 'Finance.vwGeneralCashBankHeader';
execute sp_refreshview 'Accounting.vwBeginningBalanceAP';
execute sp_refreshview 'Accounting.vwBeginningBalanceAR';";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsInterCashBank",
                schema: "Finance",
                table: "GeneralCashBankHeader");

            migrationBuilder.DropColumn(
                name: "PaidAmount",
                schema: "Accounting",
                table: "BeginningBalanceAR");

            migrationBuilder.DropColumn(
                name: "PaidAmount",
                schema: "Accounting",
                table: "BeginningBalanceAP");

            migrationBuilder.AlterColumn<decimal>(
                name: "PaidAmount",
                schema: "Sales",
                table: "SalesInvoiceHeader",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(19,6)");

            migrationBuilder.AlterColumn<decimal>(
                name: "PaidAmount",
                schema: "Purchasing",
                table: "PurchaseInvoiceHeader",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(19,6)");
        }
    }
}
