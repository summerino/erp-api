using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AddingColumnFailedSendAll : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "FailedSendAll",
                schema: "Sales",
                table: "DeliveryPlanDetail",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // Reorder column Sales.DeliveryPlanDetail
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
ALTER TABLE Sales.DeliveryPlanDetail
	DROP CONSTRAINT FK_DeliveryPlanDetail_DeliveryPlanHeader_Code
GO
ALTER TABLE Sales.DeliveryPlanHeader SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE Sales.Tmp_DeliveryPlanDetail
	(
	Id bigint NOT NULL IDENTITY (1, 1),
	Code varchar(17) NOT NULL,
	[LineNo] smallint NOT NULL,
	TransCode varchar(17) NOT NULL,
	Volume decimal(19, 6) NOT NULL,
	Weight decimal(19, 6) NOT NULL,
	SrcTrans smallint NOT NULL,
	IsFailShipment bit NOT NULL,
	FailedSendAll bit NOT NULL,
	NotesFailShipment varchar(256) NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Sales.Tmp_DeliveryPlanDetail SET (LOCK_ESCALATION = TABLE)
GO
SET IDENTITY_INSERT Sales.Tmp_DeliveryPlanDetail ON
GO
IF EXISTS(SELECT * FROM Sales.DeliveryPlanDetail)
	 EXEC('INSERT INTO Sales.Tmp_DeliveryPlanDetail (Id, Code, [LineNo], TransCode, Volume, Weight, SrcTrans, IsFailShipment, FailedSendAll, NotesFailShipment)
		SELECT Id, Code, [LineNo], TransCode, Volume, Weight, SrcTrans, IsFailShipment, FailedSendAll, NotesFailShipment FROM Sales.DeliveryPlanDetail WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT Sales.Tmp_DeliveryPlanDetail OFF
GO
ALTER TABLE Sales.DeliveryPlanUndeliveredItem
	DROP CONSTRAINT FK_DeliveryPlanUndeliveredItem_DeliveryPlanDetail_DlvPlanDetailId
GO
DROP TABLE Sales.DeliveryPlanDetail
GO
EXECUTE sp_rename N'Sales.Tmp_DeliveryPlanDetail', N'DeliveryPlanDetail', 'OBJECT' 
GO
ALTER TABLE Sales.DeliveryPlanDetail ADD CONSTRAINT
	PK_DeliveryPlanDetail PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX IX_DeliveryPlanDetail_TransCode ON Sales.DeliveryPlanDetail
	(
	TransCode
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_DeliveryPlanDetail_Code ON Sales.DeliveryPlanDetail
	(
	Code
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE Sales.DeliveryPlanDetail ADD CONSTRAINT
	FK_DeliveryPlanDetail_DeliveryPlanHeader_Code FOREIGN KEY
	(
	Code
	) REFERENCES Sales.DeliveryPlanHeader
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Sales.DeliveryPlanUndeliveredItem ADD CONSTRAINT
	FK_DeliveryPlanUndeliveredItem_DeliveryPlanDetail_DlvPlanDetailId FOREIGN KEY
	(
	DlvPlanDetailId
	) REFERENCES Sales.DeliveryPlanDetail
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Sales.DeliveryPlanUndeliveredItem SET (LOCK_ESCALATION = TABLE)
GO
COMMIT";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FailedSendAll",
                schema: "Sales",
                table: "DeliveryPlanDetail");
        }
    }
}
