using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class CreateTableClosingMonth : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClosingMonth",
                schema: "Accounting",
                columns: table => new
                {
                    Period = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: false),
                    IsClose = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClosingMonth", x => x.Period);
                });

            // Alter procedure Sales.vwSalesInvoiceHeader
            var sql = @"ALTER VIEW [Sales].[vwSalesInvoiceHeader]
AS
    SELECT si_h.*,
		si_h.Total - si_h.PaidAmount AS Remaining,
        c.[Name] AS CustName,
        e.Initial AS IssuedInitial,
        u_c.Initial AS CreatedInitial,
        u_u.Initial AS UpdatedInitial,
        u_a.Initial AS ApprovedInitial,
        CASE si_h.Mark
            WHEN 'A' THEN 'Active'
            WHEN 'V' THEN 'Void' END AS [Status]
    FROM Sales.SalesInvoiceHeader si_h
    LEFT JOIN General.Customer c
        ON c.Code = si_h.CustCode
    LEFT JOIN General.Employee e
        ON e.Id = si_h.IssuedBy
    LEFT JOIN SystemManagement.[User] u_c
        ON u_c.Id = si_h.CreatedBy
    LEFT JOIN SystemManagement.[User] u_u
        ON u_u.Id = si_h.UpdatedBy
    LEFT JOIN SystemManagement.[User] u_a
        ON u_a.Id = si_h.ApprovedBy";
            migrationBuilder.Sql(sql);

            // Alter procedure Purchasing.vwPurchaseInvoiceHeader
            sql = @"ALTER VIEW [Purchasing].[vwPurchaseInvoiceHeader]
AS
    SELECT pi_h.*,
		pi_h.Total - pi_h.PaidAmount as Remaining,
        s.[Name] AS SupName,
        e.Initial AS IssuedInitial,
        u_c.Initial AS CreatedInitial,
        u_u.Initial AS UpdatedInitial,
        u_a.Initial AS ApprovedInitial,
        CASE pi_h.Mark
            WHEN 'A' THEN 'Active'
            WHEN 'V' THEN 'Void' END AS [Status]
    FROM Purchasing.PurchaseInvoiceHeader pi_h
    LEFT JOIN General.Supplier s
        ON s.Code = pi_h.SupCode
    LEFT JOIN General.Employee e
        ON e.Id = pi_h.IssuedBy
    LEFT JOIN SystemManagement.[User] u_c
        ON u_c.Id = pi_h.CreatedBy
    LEFT JOIN SystemManagement.[User] u_u
        ON u_u.Id = pi_h.UpdatedBy
    LEFT JOIN SystemManagement.[User] u_a
        ON u_a.Id = pi_h.ApprovedBy";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClosingMonth",
                schema: "Accounting");
        }
    }
}
