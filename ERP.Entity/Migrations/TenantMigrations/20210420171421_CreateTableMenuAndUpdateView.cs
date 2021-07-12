using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class CreateTableMenuAndUpdateView : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Menu",
                schema: "SystemManagement",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    Deep = table.Column<int>(type: "int", nullable: false),
                    Seq = table.Column<int>(type: "int", nullable: false),
                    Link = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Icon = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Menu", x => x.Id);
                });

            // Alter view Purchasing.vwPurchaseOrderHeader
            var sql = @"ALTER VIEW [Purchasing].[vwPurchaseOrderHeader]
AS
	SELECT po_h.*,
		s.[Name] AS SupName,
		e.Initial AS RequestInitial,
		CASE po_h.Mark
			WHEN 'A' THEN 'Active'
			WHEN 'V' THEN 'Void'
			WHEN 'PR' THEN 'Partial Received'
			WHEN 'CMP' THEN 'Completed'
			WHEN 'CLS' THEN 'Closed' END AS [Status]
	FROM Purchasing.PurchaseOrderHeader po_h
	LEFT JOIN General.Supplier s
		ON s.Code = po_h.SupCode
	LEFT JOIN General.Employee e
		ON e.Id = po_h.RequestBy";
            migrationBuilder.Sql(sql);

            // Alter view Purchasing.vwPurchaseReceiveHeader
            sql = @"ALTER VIEW [Purchasing].[vwPurchaseReceiveHeader]
AS
	SELECT pr_h.*,
		s.[Name] AS SupName,
		e.Initial AS ReceiveInitial,
		CASE pr_h.Mark
			WHEN 'A' THEN 'Active'
			WHEN 'V' THEN 'Void'
			WHEN 'INV' THEN 'Invoiced' END AS [Status]
	FROM Purchasing.PurchaseReceiveHeader pr_h
	LEFT JOIN General.Supplier s
		ON s.Code = pr_h.SupCode
	LEFT JOIN General.Employee e
		ON e.Id = pr_h.ReceiveBy";
            migrationBuilder.Sql(sql);

            // Alter view Purchasing.vwPurchaseInvoiceHeader
            sql = @"ALTER VIEW [Purchasing].[vwPurchaseInvoiceHeader]
AS
	SELECT pi_h.*,
		s.[Name] AS SupName,
		e.Initial AS IssuedInitial,
		CASE pi_h.Mark
			WHEN 'A' THEN 'Active'
			WHEN 'V' THEN 'Void' END AS [Status]
	FROM Purchasing.PurchaseInvoiceHeader pi_h
	LEFT JOIN General.Supplier s
		ON s.Code = pi_h.SupCode
	LEFT JOIN General.Employee e
		ON e.Id = pi_h.IssuedBy";
            migrationBuilder.Sql(sql);

            // Alter view Purchasing.vwSalesOrderHeader
            sql = @"ALTER VIEW [Sales].[vwSalesOrderHeader]
AS
	SELECT so_h.*,
		c.[Name] AS CustName,
		e.Initial AS SalesInitial,
		CASE so_h.Mark
			WHEN 'A' THEN 'Active'
			WHEN 'V' THEN 'Void'
			WHEN 'PS' THEN 'Partial Shipped'
			WHEN 'CMP' THEN 'Completed'
			WHEN 'CLS' THEN 'Closed' END AS [Status]
	FROM Sales.SalesOrderHeader so_h
	LEFT JOIN General.Customer c
		ON c.Code = so_h.CustCode
	LEFT JOIN General.Employee e
		ON e.Id = so_h.SalesBy";
            migrationBuilder.Sql(sql);

			// Alter view Purchasing.vwSalesDeliveryHeader
			sql = @"ALTER VIEW [Sales].[vwSalesDeliveryHeader]
AS
	SELECT do_h.*,
		c.[Name] AS CustName,
		e.Initial AS ShippedInitial,
		CASE do_h.Mark
			WHEN 'A' THEN 'Active'
			WHEN 'V' THEN 'Void'
			WHEN 'INV' THEN 'Invoiced' END AS [Status]
	FROM Sales.SalesDeliveryHeader do_h
	LEFT JOIN General.Customer c
		ON c.Code = do_h.CustCode
	LEFT JOIN General.Employee e
		ON e.Id = do_h.ShippedBy";
            migrationBuilder.Sql(sql);

			// Alter view Purchasing.vwSalesInvoiceHeader
			sql = @"ALTER VIEW [Sales].[vwSalesInvoiceHeader]
AS
	SELECT si_h.*,
		c.[Name] AS CustName,
		e.Initial AS IssuedInitial,
		CASE si_h.Mark
			WHEN 'A' THEN 'Active'
			WHEN 'V' THEN 'Void' END AS [Status]
	FROM Sales.SalesInvoiceHeader si_h
	LEFT JOIN General.Customer c
		ON c.Code = si_h.CustCode
	LEFT JOIN General.Employee e
		ON e.Id = si_h.IssuedBy";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Menu",
                schema: "SystemManagement");
        }
    }
}
