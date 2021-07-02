using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP_API.Migrations.TenantMigrations
{
    public partial class AddingColumnCustomerInPromo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "ApplyTo",
                schema: "Sales",
                table: "PromoHeader",
                type: "smallint",
                nullable: false,
                defaultValue: (short)1);

            migrationBuilder.AddColumn<string>(
                name: "CustCode",
                schema: "Sales",
                table: "PromoHeader",
                type: "varchar(8)",
                unicode: false,
                maxLength: 8,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CustTypeId",
                schema: "Sales",
                table: "PromoHeader",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PromoHeader_CustCode",
                schema: "Sales",
                table: "PromoHeader",
                column: "CustCode");

            migrationBuilder.CreateIndex(
                name: "IX_PromoHeader_CustTypeId",
                schema: "Sales",
                table: "PromoHeader",
                column: "CustTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_PromoHeader_Customer_CustCode",
                schema: "Sales",
                table: "PromoHeader",
                column: "CustCode",
                principalSchema: "General",
                principalTable: "Customer",
                principalColumn: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_PromoHeader_CustomerType_CustTypeId",
                schema: "Sales",
                table: "PromoHeader",
                column: "CustTypeId",
                principalSchema: "General",
                principalTable: "CustomerType",
                principalColumn: "Id");

            // Reorder column in Sales.vwPromoHeader
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
ALTER TABLE Sales.PromoHeader
	DROP CONSTRAINT FK_PromoHeader_CustomerType_CustTypeId
GO
ALTER TABLE General.CustomerType SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Sales.PromoHeader
	DROP CONSTRAINT FK_PromoHeader_Customer_CustCode
GO
ALTER TABLE General.Customer SET (LOCK_ESCALATION = TABLE)
GO
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
	CustCode varchar(8) NULL,
	CustTypeId int NULL,
	CoaCost varchar(6) NOT NULL,
	Mark varchar(3) NOT NULL,
	CreatedBy int NOT NULL,
	CreatedDate datetime NOT NULL,
	UpdatedBy int NOT NULL,
	UpdatedDate datetime NOT NULL,
	ApprovedBy int NULL,
	ApprovedDate datetime NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Sales.Tmp_PromoHeader SET (LOCK_ESCALATION = TABLE)
GO
IF EXISTS(SELECT * FROM Sales.PromoHeader)
	 EXEC('INSERT INTO Sales.Tmp_PromoHeader (Code, Name, StartDate, EndDate, ApplyTo, CustCode, CustTypeId, CoaCost, Mark, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, ApprovedBy, ApprovedDate)
		SELECT Code, Name, StartDate, EndDate, ApplyTo, CustCode, CustTypeId, CoaCost, Mark, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, ApprovedBy, ApprovedDate FROM Sales.PromoHeader WITH (HOLDLOCK TABLOCKX)')
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
CREATE NONCLUSTERED INDEX IX_PromoHeader_CustCode ON Sales.PromoHeader
	(
	CustCode
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_PromoHeader_CustTypeId ON Sales.PromoHeader
	(
	CustTypeId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE Sales.PromoHeader ADD CONSTRAINT
	FK_PromoHeader_Customer_CustCode FOREIGN KEY
	(
	CustCode
	) REFERENCES General.Customer
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Sales.PromoHeader ADD CONSTRAINT
	FK_PromoHeader_CustomerType_CustTypeId FOREIGN KEY
	(
	CustTypeId
	) REFERENCES General.CustomerType
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
COMMIT";
            migrationBuilder.Sql(sql);

            // refresh view Sales.vwPromoHeader
            sql = @"execute sp_refreshview 'Sales.vwPromoHeader'";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApplyTo",
                schema: "Sales",
                table: "PromoHeader");

            migrationBuilder.DropColumn(
                name: "CustCode",
                schema: "Sales",
                table: "PromoHeader");

            migrationBuilder.DropColumn(
                name: "CustTypeId",
                schema: "Sales",
                table: "PromoHeader");

            migrationBuilder.DropForeignKey(
                name: "FK_PromoHeader_Customer_CustCode",
                schema: "Sales",
                table: "PromoHeader");

            migrationBuilder.DropForeignKey(
                name: "FK_PromoHeader_CustomerType_CustTypeId",
                schema: "Sales",
                table: "PromoHeader");

            migrationBuilder.DropIndex(
                name: "IX_PromoHeader_CustCode",
                schema: "Sales",
                table: "PromoHeader");

            migrationBuilder.DropIndex(
                name: "IX_PromoHeader_CustTypeId",
                schema: "Sales",
                table: "PromoHeader");
        }
    }
}
