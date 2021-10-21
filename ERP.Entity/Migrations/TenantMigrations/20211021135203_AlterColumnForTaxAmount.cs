using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AlterColumnForTaxAmount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "TaxAmount",
                schema: "Sales",
                table: "SalesReturnDetail",
                type: "decimal(19,6)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "TaxAmount",
                schema: "Sales",
                table: "SalesOrderDetail",
                type: "decimal(19,6)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "TaxAmount",
                schema: "Sales",
                table: "SalesDeliveryDetail",
                type: "decimal(19,6)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "TaxAmount",
                schema: "Purchasing",
                table: "PurchaseReturnDetail",
                type: "decimal(19,6)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "TaxAmount",
                schema: "Purchasing",
                table: "PurchaseReceiveDetail",
                type: "decimal(19,6)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "TaxAmount",
                schema: "Purchasing",
                table: "PurchaseOrderDetail",
                type: "decimal(19,6)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            // Refresh view Purchasing.vwPurchaseOrderDetail
            var sql = @"EXECUTE sp_refreshview '[Purchasing].[vwPurchaseOrderDetail]'";
            migrationBuilder.Sql(sql);

            // Refresh view Purchasing.vwPurchaseReceiveDetail
            sql = @"EXECUTE sp_refreshview '[Purchasing].[vwPurchaseReceiveDetail]'";
            migrationBuilder.Sql(sql);

            // Refresh view Purchasing.vwPurchaseReturnDetail
            sql = @"EXECUTE sp_refreshview '[Purchasing].[vwPurchaseReturnDetail]'";
            migrationBuilder.Sql(sql);

            // Refresh view Sales.vwSalesDeliveryDetail
            sql = @"EXECUTE sp_refreshview '[Sales].[vwSalesDeliveryDetail]'";
            migrationBuilder.Sql(sql);

            // Refresh view Sales.vwSalesOrderDetail
            sql = @"EXECUTE sp_refreshview '[Sales].[vwSalesOrderDetail]'";
            migrationBuilder.Sql(sql);

            // Refresh view Sales.vwSalesReturnDetail
            sql = @"EXECUTE sp_refreshview '[Sales].[vwSalesReturnDetail]'";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "TaxAmount",
                schema: "Sales",
                table: "SalesReturnDetail",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(19,6)");

            migrationBuilder.AlterColumn<decimal>(
                name: "TaxAmount",
                schema: "Sales",
                table: "SalesOrderDetail",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(19,6)");

            migrationBuilder.AlterColumn<decimal>(
                name: "TaxAmount",
                schema: "Sales",
                table: "SalesDeliveryDetail",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(19,6)");

            migrationBuilder.AlterColumn<decimal>(
                name: "TaxAmount",
                schema: "Purchasing",
                table: "PurchaseReturnDetail",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(19,6)");

            migrationBuilder.AlterColumn<decimal>(
                name: "TaxAmount",
                schema: "Purchasing",
                table: "PurchaseReceiveDetail",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(19,6)");

            migrationBuilder.AlterColumn<decimal>(
                name: "TaxAmount",
                schema: "Purchasing",
                table: "PurchaseOrderDetail",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(19,6)");
        }
    }
}
