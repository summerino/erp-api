using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Web.API.Migrations.TenantMigrations
{
    public partial class UpdateTableSystemParameter : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Delete record in SystemManagement.SystemParameter
            var sql = @"DELETE FROM [SystemManagement].[SystemParameter]";
            migrationBuilder.Sql(sql);

            migrationBuilder.DropColumn(
                name: "Category",
                schema: "SystemManagement",
                table: "SystemParameter");

            migrationBuilder.DropColumn(
                name: "Module",
                schema: "SystemManagement",
                table: "SystemParameter");

            migrationBuilder.AlterColumn<int>(
                name: "Seq",
                schema: "SystemManagement",
                table: "SystemParameter",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataType",
                schema: "SystemManagement",
                table: "SystemParameter",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "SystemManagement",
                table: "SystemParameter",
                type: "varchar(256)",
                unicode: false,
                maxLength: 256,
                nullable: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "SystemManagement",
                table: "SystemParameter",
                type: "bit",
                nullable: false);

            migrationBuilder.AddColumn<int>(
                name: "ModuleId",
                schema: "SystemManagement",
                table: "SystemParameter",
                type: "int",
                nullable: false);

            migrationBuilder.CreateTable(
                name: "SystemParameterModule",
                schema: "SystemManagement",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    Deep = table.Column<int>(type: "int", nullable: false),
                    Seq = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemParameterModule", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SystemParameter_ModuleId",
                schema: "SystemManagement",
                table: "SystemParameter",
                column: "ModuleId");

            migrationBuilder.AddForeignKey(
                name: "FK_SystemParameter_SystemParameterModule_ModuleId",
                schema: "SystemManagement",
                table: "SystemParameter",
                column: "ModuleId",
                principalSchema: "SystemManagement",
                principalTable: "SystemParameterModule",
                principalColumn: "Id");

            // Reorder column in table SystemManagement.SystemParameter
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
ALTER TABLE SystemManagement.SystemParameter
	DROP CONSTRAINT FK_SystemParameter_SystemParameterModule_ModuleId
GO
ALTER TABLE SystemManagement.SystemParameterModule SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE SystemManagement.Tmp_SystemParameter
	(
	Id int NOT NULL IDENTITY (1, 1),
	ModuleId int NOT NULL,
	Code varchar(50) NOT NULL,
	Value varchar(100) NOT NULL,
	DataType varchar(20) NOT NULL,
	Description varchar(256) NOT NULL,
	Seq int NOT NULL,
	IsActive bit NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE SystemManagement.Tmp_SystemParameter SET (LOCK_ESCALATION = TABLE)
GO
SET IDENTITY_INSERT SystemManagement.Tmp_SystemParameter ON
GO
IF EXISTS(SELECT * FROM SystemManagement.SystemParameter)
	 EXEC('INSERT INTO SystemManagement.Tmp_SystemParameter (Id, ModuleId, Code, Value, DataType, Description, Seq, IsActive)
		SELECT Id, ModuleId, Code, Value, DataType, Description, Seq, IsActive FROM SystemManagement.SystemParameter WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT SystemManagement.Tmp_SystemParameter OFF
GO
DROP TABLE SystemManagement.SystemParameter
GO
EXECUTE sp_rename N'SystemManagement.Tmp_SystemParameter', N'SystemParameter', 'OBJECT' 
GO
ALTER TABLE SystemManagement.SystemParameter ADD CONSTRAINT
	PK_SystemParameter PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE UNIQUE NONCLUSTERED INDEX IX_SystemParameter_Code ON SystemManagement.SystemParameter
	(
	Code
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_SystemParameter_ModuleId ON SystemManagement.SystemParameter
	(
	ModuleId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE SystemManagement.SystemParameter ADD CONSTRAINT
	FK_SystemParameter_SystemParameterModule_ModuleId FOREIGN KEY
	(
	ModuleId
	) REFERENCES SystemManagement.SystemParameterModule
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
            migrationBuilder.DropForeignKey(
                name: "FK_SystemParameter_SystemParameterModule_ModuleId",
                schema: "SystemManagement",
                table: "SystemParameter");

            migrationBuilder.DropTable(
                name: "SystemParameterModule",
                schema: "SystemManagement");

            migrationBuilder.DropIndex(
                name: "IX_SystemParameter_ModuleId",
                schema: "SystemManagement",
                table: "SystemParameter");

            migrationBuilder.DropColumn(
                name: "DataType",
                schema: "SystemManagement",
                table: "SystemParameter");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "SystemManagement",
                table: "SystemParameter");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "SystemManagement",
                table: "SystemParameter");

            migrationBuilder.DropColumn(
                name: "ModuleId",
                schema: "SystemManagement",
                table: "SystemParameter");

            migrationBuilder.AlterColumn<int>(
                name: "Seq",
                schema: "SystemManagement",
                table: "SystemParameter",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                schema: "SystemManagement",
                table: "SystemParameter",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Module",
                schema: "SystemManagement",
                table: "SystemParameter",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true);
        }
    }
}
