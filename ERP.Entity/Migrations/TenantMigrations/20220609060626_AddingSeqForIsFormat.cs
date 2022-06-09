using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AddingSeqForIsFormat : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IsDetSeq",
                schema: "Accounting",
                table: "COA",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IsSeq",
                schema: "Accounting",
                table: "COA",
                type: "int",
                nullable: true);

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
	IsSeq int NULL,
	IsDetSeq int NULL,
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
	 EXEC('INSERT INTO Accounting.Tmp_COA (Id, Code, Name, TypeId, ParentId, Deep, Description, CurrCode, CBType, VouCode, BsCode, IsCode, IsDetCode, IsSeq, IsDetSeq, ShowInMobile, IsActive, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate)
		SELECT Id, Code, Name, TypeId, ParentId, Deep, Description, CurrCode, CBType, VouCode, BsCode, IsCode, IsDetCode, IsSeq, IsDetSeq, ShowInMobile, IsActive, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate FROM Accounting.COA WITH (HOLDLOCK TABLOCKX)')
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

            // Refresh view Accounting.vwCOA
            sql = @"EXEC sp_refreshview 'Accounting.vwCOA'";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDetSeq",
                schema: "Accounting",
                table: "COA");

            migrationBuilder.DropColumn(
                name: "IsSeq",
                schema: "Accounting",
                table: "COA");
        }
    }
}
