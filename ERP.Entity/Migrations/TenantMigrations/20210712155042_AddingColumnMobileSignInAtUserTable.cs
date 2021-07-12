using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AddingColumnMobileSignInAtUserTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "MobileSignIn",
                schema: "SystemManagement",
                table: "User",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // Reorder column in table 
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
ALTER TABLE SystemManagement.[User]
	DROP CONSTRAINT FK_User_Role_RoleId
GO
ALTER TABLE SystemManagement.Role SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE SystemManagement.[User]
	DROP CONSTRAINT FK_User_Employee_EmployeeId
GO
ALTER TABLE General.Employee SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE SystemManagement.Tmp_User
	(
	Id int NOT NULL IDENTITY (1, 1),
	CatalogUserId uniqueidentifier NOT NULL,
	Username nvarchar(50) NOT NULL,
	Initial varchar(20) NOT NULL,
	Name varchar(50) NOT NULL,
	RoleId int NOT NULL,
	EmployeeId bigint NULL,
	MobileSignIn bit NOT NULL,
	IsLoggedIn bit NOT NULL,
	LastLogin datetime NULL,
	SessionId varchar(50) NULL,
	TokenId varchar(MAX) NULL,
	IPAddress varchar(40) NULL,
	IsActive bit NOT NULL,
	CreatedBy int NOT NULL,
	CreatedDate datetime NOT NULL,
	UpdatedBy int NOT NULL,
	UpdatedDate datetime NOT NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE SystemManagement.Tmp_User SET (LOCK_ESCALATION = TABLE)
GO
SET IDENTITY_INSERT SystemManagement.Tmp_User ON
GO
IF EXISTS(SELECT * FROM SystemManagement.[User])
	 EXEC('INSERT INTO SystemManagement.Tmp_User (Id, CatalogUserId, Username, Initial, Name, RoleId, EmployeeId, MobileSignIn, IsLoggedIn, LastLogin, SessionId, TokenId, IPAddress, IsActive, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate)
		SELECT Id, CatalogUserId, Username, Initial, Name, RoleId, EmployeeId, MobileSignIn, IsLoggedIn, LastLogin, SessionId, TokenId, IPAddress, IsActive, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate FROM SystemManagement.[User] WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT SystemManagement.Tmp_User OFF
GO
DROP TABLE SystemManagement.[User]
GO
EXECUTE sp_rename N'SystemManagement.Tmp_User', N'User', 'OBJECT' 
GO
ALTER TABLE SystemManagement.[User] ADD CONSTRAINT
	PK_User PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX IX_User_EmployeeId ON SystemManagement.[User]
	(
	EmployeeId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_User_RoleId ON SystemManagement.[User]
	(
	RoleId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE SystemManagement.[User] ADD CONSTRAINT
	FK_User_Employee_EmployeeId FOREIGN KEY
	(
	EmployeeId
	) REFERENCES General.Employee
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE SystemManagement.[User] ADD CONSTRAINT
	FK_User_Role_RoleId FOREIGN KEY
	(
	RoleId
	) REFERENCES SystemManagement.Role
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
COMMIT";
			migrationBuilder.Sql(sql);

			// refresh view vwUser
			sql = @"execute sp_refreshview 'SystemManagement.vwUser'";
            migrationBuilder.Sql(sql);
		}

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MobileSignIn",
                schema: "SystemManagement",
                table: "User");
        }
    }
}
