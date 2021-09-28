using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class CreateTablePostingLog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Mark",
                schema: "MobileSales",
                table: "MobileCustomer",
                type: "varchar(3)",
                unicode: false,
                maxLength: 3,
                nullable: false,
                defaultValue: "A");

            migrationBuilder.AddColumn<int>(
                name: "RejectedBy",
                schema: "MobileSales",
                table: "MobileCustomer",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RejectedDate",
                schema: "MobileSales",
                table: "MobileCustomer",
                type: "datetime",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PostingLog",
                schema: "Accounting",
                columns: table => new
                {
                    Period = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: false),
                    IsPosted = table.Column<bool>(type: "bit", nullable: false),
                    PostedBy = table.Column<int>(type: "int", nullable: true),
                    PostedDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostingLog", x => x.Period);
                });

			// Reorder column MobileSales.MobileCustomer
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
ALTER TABLE MobileSales.MobileCustomer
	DROP CONSTRAINT FK_MobileCustomer_CustomerType_TypeId
GO
ALTER TABLE General.CustomerType SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE MobileSales.MobileCustomer
	DROP CONSTRAINT FK_MobileCustomer_Area_AreaId1
GO
ALTER TABLE MobileSales.MobileCustomer
	DROP CONSTRAINT FK_MobileCustomer_Area_AreaId2
GO
ALTER TABLE MobileSales.MobileCustomer
	DROP CONSTRAINT FK_MobileCustomer_Area_AreaId3
GO
ALTER TABLE MobileSales.MobileCustomer
	DROP CONSTRAINT FK_MobileCustomer_Area_AreaId4
GO
ALTER TABLE MobileSales.MobileCustomer
	DROP CONSTRAINT FK_MobileCustomer_Area_AreaId5
GO
ALTER TABLE Sales.Area SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE MobileSales.MobileCustomer
	DROP CONSTRAINT FK_MobileCustomer_Customer_CustCode
GO
ALTER TABLE General.Customer SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE MobileSales.Tmp_MobileCustomer
	(
	Code varchar(17) NOT NULL,
	CustCode varchar(8) NULL,
	Initial varchar(20) NOT NULL,
	Name varchar(50) NOT NULL,
	TypeId int NOT NULL,
	InitialAddress varchar(20) NOT NULL,
	Address1 varchar(100) NOT NULL,
	Address2 varchar(100) NULL,
	ContactPerson varchar(50) NULL,
	Phone varchar(30) NOT NULL,
	Fax varchar(15) NULL,
	AreaId1 int NULL,
	AreaId2 int NULL,
	AreaId3 int NULL,
	AreaId4 int NULL,
	AreaId5 int NULL,
	Mark varchar(3) NOT NULL,
	CreatedBy int NOT NULL,
	CreatedDate datetime NOT NULL,
	UpdatedBy int NOT NULL,
	UpdatedDate datetime NOT NULL,
	ApprovedBy int NULL,
	ApprovedDate datetime NULL,
	RejectedBy int NULL,
	RejectedDate datetime NULL
	)  ON [PRIMARY]
GO
ALTER TABLE MobileSales.Tmp_MobileCustomer SET (LOCK_ESCALATION = TABLE)
GO
IF EXISTS(SELECT * FROM MobileSales.MobileCustomer)
	 EXEC('INSERT INTO MobileSales.Tmp_MobileCustomer (Code, CustCode, Initial, Name, TypeId, InitialAddress, Address1, Address2, ContactPerson, Phone, Fax, AreaId1, AreaId2, AreaId3, AreaId4, AreaId5, Mark, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, ApprovedBy, ApprovedDate, RejectedBy, RejectedDate)
		SELECT Code, CustCode, Initial, Name, TypeId, InitialAddress, Address1, Address2, ContactPerson, Phone, Fax, AreaId1, AreaId2, AreaId3, AreaId4, AreaId5, Mark, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, ApprovedBy, ApprovedDate, RejectedBy, RejectedDate FROM MobileSales.MobileCustomer WITH (HOLDLOCK TABLOCKX)')
GO
DROP TABLE MobileSales.MobileCustomer
GO
EXECUTE sp_rename N'MobileSales.Tmp_MobileCustomer', N'MobileCustomer', 'OBJECT' 
GO
ALTER TABLE MobileSales.MobileCustomer ADD CONSTRAINT
	PK_MobileCustomer PRIMARY KEY CLUSTERED 
	(
	Code
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX IX_MobileCustomer_AreaId1 ON MobileSales.MobileCustomer
	(
	AreaId1
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_MobileCustomer_AreaId2 ON MobileSales.MobileCustomer
	(
	AreaId2
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_MobileCustomer_AreaId3 ON MobileSales.MobileCustomer
	(
	AreaId3
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_MobileCustomer_AreaId4 ON MobileSales.MobileCustomer
	(
	AreaId4
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_MobileCustomer_AreaId5 ON MobileSales.MobileCustomer
	(
	AreaId5
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_MobileCustomer_CustCode ON MobileSales.MobileCustomer
	(
	CustCode
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_MobileCustomer_TypeId ON MobileSales.MobileCustomer
	(
	TypeId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE MobileSales.MobileCustomer ADD CONSTRAINT
	FK_MobileCustomer_Customer_CustCode FOREIGN KEY
	(
	CustCode
	) REFERENCES General.Customer
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE MobileSales.MobileCustomer ADD CONSTRAINT
	FK_MobileCustomer_Area_AreaId1 FOREIGN KEY
	(
	AreaId1
	) REFERENCES Sales.Area
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE MobileSales.MobileCustomer ADD CONSTRAINT
	FK_MobileCustomer_Area_AreaId2 FOREIGN KEY
	(
	AreaId2
	) REFERENCES Sales.Area
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE MobileSales.MobileCustomer ADD CONSTRAINT
	FK_MobileCustomer_Area_AreaId3 FOREIGN KEY
	(
	AreaId3
	) REFERENCES Sales.Area
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE MobileSales.MobileCustomer ADD CONSTRAINT
	FK_MobileCustomer_Area_AreaId4 FOREIGN KEY
	(
	AreaId4
	) REFERENCES Sales.Area
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE MobileSales.MobileCustomer ADD CONSTRAINT
	FK_MobileCustomer_Area_AreaId5 FOREIGN KEY
	(
	AreaId5
	) REFERENCES Sales.Area
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE MobileSales.MobileCustomer ADD CONSTRAINT
	FK_MobileCustomer_CustomerType_TypeId FOREIGN KEY
	(
	TypeId
	) REFERENCES General.CustomerType
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
COMMIT";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Mark",
                schema: "MobileSales",
                table: "MobileCustomer");

            migrationBuilder.DropColumn(
                name: "RejectedBy",
                schema: "MobileSales",
                table: "MobileCustomer");

            migrationBuilder.DropColumn(
                name: "RejectedDate",
                schema: "MobileSales",
                table: "MobileCustomer");

            migrationBuilder.DropTable(
                name: "PostingLog",
                schema: "Accounting");
        }
    }
}
