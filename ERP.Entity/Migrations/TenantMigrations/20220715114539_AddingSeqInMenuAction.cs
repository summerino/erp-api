using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AddingSeqInMenuAction : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Seq",
                schema: "SystemManagement",
                table: "MenuAction",
                type: "int",
                nullable: false,
                defaultValue: 1);

			// Reorder column SystemManagement.MenuAction
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
ALTER TABLE SystemManagement.MenuAction
	DROP CONSTRAINT FK_MenuAction_Menu_MenuId
GO
ALTER TABLE SystemManagement.Menu SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE SystemManagement.MenuAction
	DROP CONSTRAINT FK_MenuAction_Action_ActionId
GO
ALTER TABLE SystemManagement.Action SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE SystemManagement.Tmp_MenuAction
	(
	Id bigint NOT NULL IDENTITY (1, 1),
	MenuId int NOT NULL,
	ActionId int NOT NULL,
	Seq int NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE SystemManagement.Tmp_MenuAction SET (LOCK_ESCALATION = TABLE)
GO
SET IDENTITY_INSERT SystemManagement.Tmp_MenuAction ON
GO
IF EXISTS(SELECT * FROM SystemManagement.MenuAction)
	 EXEC('INSERT INTO SystemManagement.Tmp_MenuAction (Id, MenuId, ActionId, Seq)
		SELECT Id, MenuId, ActionId, Seq FROM SystemManagement.MenuAction WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT SystemManagement.Tmp_MenuAction OFF
GO
DROP TABLE SystemManagement.MenuAction
GO
EXECUTE sp_rename N'SystemManagement.Tmp_MenuAction', N'MenuAction', 'OBJECT' 
GO
ALTER TABLE SystemManagement.MenuAction ADD CONSTRAINT
	PK_MenuAction PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE UNIQUE NONCLUSTERED INDEX IX_MenuAction_MenuId_ActionId ON SystemManagement.MenuAction
	(
	MenuId,
	ActionId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_MenuAction_ActionId ON SystemManagement.MenuAction
	(
	ActionId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE SystemManagement.MenuAction ADD CONSTRAINT
	FK_MenuAction_Action_ActionId FOREIGN KEY
	(
	ActionId
	) REFERENCES SystemManagement.Action
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE SystemManagement.MenuAction ADD CONSTRAINT
	FK_MenuAction_Menu_MenuId FOREIGN KEY
	(
	MenuId
	) REFERENCES SystemManagement.Menu
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
                name: "Seq",
                schema: "SystemManagement",
                table: "MenuAction");
        }
    }
}
