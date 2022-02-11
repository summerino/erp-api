using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class RemoveTypeIdAndIssuedBy : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalesInvoiceHeader_Employee_IssuedBy",
                schema: "Sales",
                table: "SalesInvoiceHeader");

            migrationBuilder.DropIndex(
                name: "IX_SalesInvoiceHeader_IssuedBy",
                schema: "Sales",
                table: "SalesInvoiceHeader");

            migrationBuilder.DropColumn(
                name: "IssuedBy",
                schema: "Sales",
                table: "SalesInvoiceHeader");

            migrationBuilder.DropColumn(
                name: "TypeId",
                schema: "Inventory",
                table: "Item");

            // Alter view Inventory.vwItem
            var sql = @"ALTER VIEW [Inventory].[vwItem]
AS
	SELECT i.*,
		c.[Name] AS CategoryName,
		uom.Initial AS UomInitial,
		uom_c_s.UnitEquivalent AS UomSellName,
		uom_c_b.UnitEquivalent AS UomBuyName,
		uom_c_s.Seq AS SellSeq,
		uom_c_b.Seq AS BuySeq,
		u_c.Initial AS CreatedInitial,
		u_u.Initial AS UpdatedInitial
	FROM Inventory.Item i
	LEFT JOIN Inventory.ItemCategory c
		ON c.Id = i.CategoryId
	LEFT JOIN Inventory.UoM uom
		ON uom.Id = i.UomId
	LEFT JOIN Inventory.UoMConversion uom_c_s
		ON uom_c_s.Id = i.UomSellId
	LEFT JOIN Inventory.UoMConversion uom_c_b
		ON uom_c_b.Id = i.UomBuyId
	LEFT JOIN SystemManagement.[User] u_c
		ON u_c.Id = c.CreatedBy
	LEFT JOIN SystemManagement.[User] u_u
		ON u_u.Id = c.UpdatedBy";
            migrationBuilder.Sql(sql);

            // Alter view Sales.vwSalesInvoiceHeader
            sql = @"ALTER VIEW [Sales].[vwSalesInvoiceHeader]
AS
    SELECT si_h.*,
		si_h.Total - si_h.PaidAmount AS Remaining,
        c.[Name] AS CustName,
		ca.Address1 AS CustAddress,
		sa.[Name] AS CustArea,
        u_c.Initial AS CreatedInitial,
        u_u.Initial AS UpdatedInitial,
        u_a.Initial AS ApprovedInitial,
        CASE si_h.Mark
            WHEN 'A' THEN 'Active'
            WHEN 'PP' THEN 'Pending Payment'
            WHEN 'CMP' THEN 'Completed'
            WHEN 'V' THEN 'Void' END AS [Status]
    FROM Sales.SalesInvoiceHeader si_h
    LEFT JOIN General.Customer c
        ON c.Code = si_h.CustCode
	LEFT JOIN General.CustomerAddress ca
		ON ca.Code = c.Code
		AND ca.IsDefault = 1
	LEFT JOIN Sales.Area sa
		ON sa.Id = c.AreaId1
    LEFT JOIN SystemManagement.[User] u_c
        ON u_c.Id = si_h.CreatedBy
    LEFT JOIN SystemManagement.[User] u_u
        ON u_u.Id = si_h.UpdatedBy
    LEFT JOIN SystemManagement.[User] u_a
        ON u_a.Id = si_h.ApprovedBy";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "IssuedBy",
                schema: "Sales",
                table: "SalesInvoiceHeader",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<short>(
                name: "TypeId",
                schema: "Inventory",
                table: "Item",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceHeader_IssuedBy",
                schema: "Sales",
                table: "SalesInvoiceHeader",
                column: "IssuedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesInvoiceHeader_Employee_IssuedBy",
                schema: "Sales",
                table: "SalesInvoiceHeader",
                column: "IssuedBy",
                principalSchema: "General",
                principalTable: "Employee",
                principalColumn: "Id");
        }
    }
}
