using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP_API.Migrations.TenantMigrations
{
    public partial class CreateTableDebitMemo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DebitMemo",
                schema: "Purchasing",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    SrcTrans = table.Column<short>(type: "smallint", nullable: false),
                    SupCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    TransCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: true),
                    CurrCode = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Used = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    Mark = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DebitMemo", x => x.Code);
                });

            // Reordering column in table General.Customer
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
CREATE TABLE General.Tmp_Customer
	(
	Code varchar(8) NOT NULL,
	Initial varchar(20) NOT NULL,
	Name varchar(50) NOT NULL,
	TypeId int NOT NULL,
	Email varchar(50) NULL,
	Website varchar(50) NULL,
	CreditTerm smallint NOT NULL,
	CreditLimit decimal(18, 2) NOT NULL,
	RefNo varchar(30) NULL,
	Notes varchar(256) NULL,
	BillingAddressId int NULL,
	ShippingAddressId int NULL,
	IsActive bit NOT NULL,
	CreatedBy int NOT NULL,
	CreatedDate datetime NOT NULL,
	UpdatedBy int NOT NULL,
	UpdatedDate datetime NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE General.Tmp_Customer SET (LOCK_ESCALATION = TABLE)
GO
IF EXISTS(SELECT * FROM General.Customer)
	 EXEC('INSERT INTO General.Tmp_Customer (Code, Initial, Name, TypeId, Email, Website, CreditTerm, CreditLimit, RefNo, Notes, BillingAddressId, ShippingAddressId, IsActive, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate)
		SELECT Code, Initial, Name, TypeId, Email, Website, CreditTerm, CreditLimit, RefNo, Notes, BillingAddressId, ShippingAddressId, IsActive, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate FROM General.Customer WITH (HOLDLOCK TABLOCKX)')
GO
DROP TABLE General.Customer
GO
EXECUTE sp_rename N'General.Tmp_Customer', N'Customer', 'OBJECT' 
GO
ALTER TABLE General.Customer ADD CONSTRAINT
	PK_Customer PRIMARY KEY CLUSTERED 
	(
	Code
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
COMMIT";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DebitMemo",
                schema: "Purchasing");
        }
    }
}
