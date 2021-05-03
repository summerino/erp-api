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

            // Create view General.Customer
            var sql = @"CREATE VIEW [Purchasing].[vwDebitMemo]
AS
	SELECT m.*,
		s.[Name] AS SupName,
		CASE m.SrcTrans
			WHEN 1 THEN 'Deposit'
			WHEN 2 THEN 'Return'
			WHEN 3 THEN 'Return (Same Item)' END AS SrcTransName,
		CASE m.Mark
			WHEN 'A' THEN 'Active'
			WHEN 'V' THEN 'Void'
			WHEN 'PU' THEN 'Partial Used'
			WHEN 'FU' THEN 'Full Used' END AS [Status]
	FROM Purchasing.DebitMemo m
	LEFT JOIN General.Supplier s
		ON s.Code = m.SupCode";
            migrationBuilder.Sql(sql);

			// Alter view General.vwCustomer
			sql = @"ALTER VIEW [General].[vwCustomer]
AS
	SELECT c.*, 
		ca.Initial AS InitialAddress, 
		ca.Address1, 
		ca.Address2, 
		ca.ContactPerson,
		ca.Phone,
		ca.Fax,
		ISNULL(ca.IsDefault, 0) AS IsDefault,
		t.[Name] AS TypeName,
		u.Initial AS UpdatedInitial
	FROM General.Customer c
	LEFT JOIN General.CustomerType t
		ON t.Id = c.TypeId
	LEFT JOIN General.CustomerAddress ca
		ON c.Code = ca.Code AND ca.IsDefault = 1
	LEFT JOIN SystemManagement.[User] u
		ON u.Id = c.UpdatedBy";
            migrationBuilder.Sql(sql);

            // Alter view Purchasing.vwPurchaseReceiveHeader
            sql = @"ALTER VIEW [Purchasing].[vwPurchaseReceiveHeader]
AS
	SELECT pr_h.*,
		s.[Name] AS SupName,
		e.Initial AS ReceiveInitial,
		u_c.Initial AS CreatedInitial,
		u_u.Initial AS UpdatedInitial,
		CASE pr_h.Mark
			WHEN 'A' THEN 'Active'
			WHEN 'V' THEN 'Void'
			WHEN 'INV' THEN 'Invoiced' END AS [Status]
	FROM Purchasing.PurchaseReceiveHeader pr_h
	LEFT JOIN General.Supplier s
		ON s.Code = pr_h.SupCode
	LEFT JOIN General.Employee e
		ON e.Id = pr_h.ReceiveBy
	LEFT JOIN SystemManagement.[User] u_c
		ON u_c.Id = pr_h.CreatedBy
	LEFT JOIN SystemManagement.[User] u_u
		ON u_u.Id = pr_h.UpdatedBy";
            migrationBuilder.Sql(sql);

            // Alter view SystemManagement.vwUser
            sql = @"ALTER VIEW [SystemManagement].[vwUser]
AS
    SELECT a.*,
        r.[Name] As RoleName,
        e.Initial As EmployeeInitial,
        u.Initial AS UpdatedInitial
    FROM SystemManagement.[User] a
    LEFT JOIN SystemManagement.[User] u
        ON u.Id = a.UpdatedBy
    LEFT JOIN General.Employee e
        ON u.EmployeeId = e.Id
    LEFT JOIN SystemManagement.[Role] r
        ON u.RoleId = r.Id";
            migrationBuilder.Sql(sql);

            // Reordering column in table General.Customer
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

            // DROP view General.Customer
            var sql = @"DROP VIEW [Purchasing].[vwDebitMemo]";
            migrationBuilder.Sql(sql);
        }
    }
}
