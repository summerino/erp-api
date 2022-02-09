using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AddingColumnViewedInformationAtTransaction : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ViewedBy",
                schema: "Sales",
                table: "VisitOrder",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ViewedDate",
                schema: "Sales",
                table: "VisitOrder",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ViewedBy",
                schema: "Inventory",
                table: "TransferStockHeader",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ViewedDate",
                schema: "Inventory",
                table: "TransferStockHeader",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ViewedBy",
                schema: "Sales",
                table: "SalesReturnHeader",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ViewedDate",
                schema: "Sales",
                table: "SalesReturnHeader",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ViewedBy",
                schema: "Sales",
                table: "SalesOrderHeader",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ViewedDate",
                schema: "Sales",
                table: "SalesOrderHeader",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ViewedBy",
                schema: "Sales",
                table: "SalesInvoiceHeader",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ViewedDate",
                schema: "Sales",
                table: "SalesInvoiceHeader",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ViewedBy",
                schema: "Sales",
                table: "SalesDeliveryHeader",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ViewedDate",
                schema: "Sales",
                table: "SalesDeliveryHeader",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ViewedBy",
                schema: "Purchasing",
                table: "PurchaseReturnHeader",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ViewedDate",
                schema: "Purchasing",
                table: "PurchaseReturnHeader",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ViewedBy",
                schema: "Purchasing",
                table: "PurchaseReceiveHeader",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ViewedDate",
                schema: "Purchasing",
                table: "PurchaseReceiveHeader",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ViewedBy",
                schema: "Purchasing",
                table: "PurchaseOrderHeader",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ViewedDate",
                schema: "Purchasing",
                table: "PurchaseOrderHeader",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ViewedBy",
                schema: "Purchasing",
                table: "PurchaseInvoiceHeader",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ViewedDate",
                schema: "Purchasing",
                table: "PurchaseInvoiceHeader",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ViewedBy",
                schema: "Sales",
                table: "PromoHeader",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ViewedDate",
                schema: "Sales",
                table: "PromoHeader",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ViewedBy",
                schema: "Accounting",
                table: "GeneralJournalHeader",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ViewedDate",
                schema: "Accounting",
                table: "GeneralJournalHeader",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ViewedBy",
                schema: "Finance",
                table: "GeneralCashBankHeader",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ViewedDate",
                schema: "Finance",
                table: "GeneralCashBankHeader",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ViewedBy",
                schema: "AssetManagement",
                table: "FixedAsset",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ViewedDate",
                schema: "AssetManagement",
                table: "FixedAsset",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ViewedBy",
                schema: "Expedition",
                table: "ExpeditionInvoiceHeader",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ViewedDate",
                schema: "Expedition",
                table: "ExpeditionInvoiceHeader",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ViewedBy",
                schema: "Sales",
                table: "DeliveryPlanHeader",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ViewedDate",
                schema: "Sales",
                table: "DeliveryPlanHeader",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ViewedBy",
                schema: "Inventory",
                table: "AdjustmentHeader",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ViewedDate",
                schema: "Inventory",
                table: "AdjustmentHeader",
                type: "datetime",
                nullable: true);

            // Refresh view Inventory.vwTransferStockHeader
            var sql = @"exec sp_refreshview 'Inventory.vwTransferStockHeader'";
            migrationBuilder.Sql(sql);

            // Refresh view Inventory.vwAdjustmentHeader
            sql = @"exec sp_refreshview 'Inventory.vwAdjustmentHeader'";
            migrationBuilder.Sql(sql);

            // Refresh view Purchasing.vwPurchaseOrderHeader
            sql = @"exec sp_refreshview 'Purchasing.vwPurchaseOrderHeader'";
            migrationBuilder.Sql(sql);

            // Refresh view Purchasing.vwPurchaseReceiveHeader
            sql = @"exec sp_refreshview 'Purchasing.vwPurchaseReceiveHeader'";
            migrationBuilder.Sql(sql);

            // Refresh view Purchasing.vwPurchaseInvoiceHeader
            sql = @"exec sp_refreshview 'Purchasing.vwPurchaseInvoiceHeader'";
            migrationBuilder.Sql(sql);

            // Refresh view Purchasing.vwPurchaseReturnHeader
            sql = @"exec sp_refreshview 'Purchasing.vwPurchaseReturnHeader'";
            migrationBuilder.Sql(sql);

            // Refresh view Sales.vwPromoHeader
            sql = @"exec sp_refreshview 'Sales.vwPromoHeader'";
            migrationBuilder.Sql(sql);

            // Refresh view Sales.vwSalesOrderHeader
            sql = @"exec sp_refreshview 'Sales.vwSalesOrderHeader'";
            migrationBuilder.Sql(sql);

            // Refresh view Sales.vwSalesDeliveryHeader
            sql = @"exec sp_refreshview 'Sales.vwSalesDeliveryHeader'";
            migrationBuilder.Sql(sql);

            // Refresh view Sales.vwSalesInvoiceHeader
            sql = @"exec sp_refreshview 'Sales.vwSalesInvoiceHeader'";
            migrationBuilder.Sql(sql);

            // Refresh view Sales.vwSalesReturnHeader
            sql = @"exec sp_refreshview 'Sales.vwSalesReturnHeader'";
            migrationBuilder.Sql(sql);

            // Refresh view Sales.vwDeliveryPlanHeader
            sql = @"exec sp_refreshview 'Sales.vwDeliveryPlanHeader'";
            migrationBuilder.Sql(sql);

            // Refresh view Expedition.vwExpeditionInvoiceHeader
            sql = @"exec sp_refreshview 'Expedition.vwExpeditionInvoiceHeader'";
            migrationBuilder.Sql(sql);

            // Refresh view Finance.vwGeneralCashBankHeader
            sql = @"exec sp_refreshview 'Finance.vwGeneralCashBankHeader'";
            migrationBuilder.Sql(sql);

            // Refresh view Accounting.vwGeneralJournalHeader
            sql = @"exec sp_refreshview 'Accounting.vwGeneralJournalHeader'";
            migrationBuilder.Sql(sql);

            // Refresh view AssetManagement.vwFixedAsset
            sql = @"exec sp_refreshview 'AssetManagement.vwFixedAsset'";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ViewedBy",
                schema: "Sales",
                table: "VisitOrder");

            migrationBuilder.DropColumn(
                name: "ViewedDate",
                schema: "Sales",
                table: "VisitOrder");

            migrationBuilder.DropColumn(
                name: "ViewedBy",
                schema: "Inventory",
                table: "TransferStockHeader");

            migrationBuilder.DropColumn(
                name: "ViewedDate",
                schema: "Inventory",
                table: "TransferStockHeader");

            migrationBuilder.DropColumn(
                name: "ViewedBy",
                schema: "Sales",
                table: "SalesReturnHeader");

            migrationBuilder.DropColumn(
                name: "ViewedDate",
                schema: "Sales",
                table: "SalesReturnHeader");

            migrationBuilder.DropColumn(
                name: "ViewedBy",
                schema: "Sales",
                table: "SalesOrderHeader");

            migrationBuilder.DropColumn(
                name: "ViewedDate",
                schema: "Sales",
                table: "SalesOrderHeader");

            migrationBuilder.DropColumn(
                name: "ViewedBy",
                schema: "Sales",
                table: "SalesInvoiceHeader");

            migrationBuilder.DropColumn(
                name: "ViewedDate",
                schema: "Sales",
                table: "SalesInvoiceHeader");

            migrationBuilder.DropColumn(
                name: "ViewedBy",
                schema: "Sales",
                table: "SalesDeliveryHeader");

            migrationBuilder.DropColumn(
                name: "ViewedDate",
                schema: "Sales",
                table: "SalesDeliveryHeader");

            migrationBuilder.DropColumn(
                name: "ViewedBy",
                schema: "Purchasing",
                table: "PurchaseReturnHeader");

            migrationBuilder.DropColumn(
                name: "ViewedDate",
                schema: "Purchasing",
                table: "PurchaseReturnHeader");

            migrationBuilder.DropColumn(
                name: "ViewedBy",
                schema: "Purchasing",
                table: "PurchaseReceiveHeader");

            migrationBuilder.DropColumn(
                name: "ViewedDate",
                schema: "Purchasing",
                table: "PurchaseReceiveHeader");

            migrationBuilder.DropColumn(
                name: "ViewedBy",
                schema: "Purchasing",
                table: "PurchaseOrderHeader");

            migrationBuilder.DropColumn(
                name: "ViewedDate",
                schema: "Purchasing",
                table: "PurchaseOrderHeader");

            migrationBuilder.DropColumn(
                name: "ViewedBy",
                schema: "Purchasing",
                table: "PurchaseInvoiceHeader");

            migrationBuilder.DropColumn(
                name: "ViewedDate",
                schema: "Purchasing",
                table: "PurchaseInvoiceHeader");

            migrationBuilder.DropColumn(
                name: "ViewedBy",
                schema: "Sales",
                table: "PromoHeader");

            migrationBuilder.DropColumn(
                name: "ViewedDate",
                schema: "Sales",
                table: "PromoHeader");

            migrationBuilder.DropColumn(
                name: "ViewedBy",
                schema: "Accounting",
                table: "GeneralJournalHeader");

            migrationBuilder.DropColumn(
                name: "ViewedDate",
                schema: "Accounting",
                table: "GeneralJournalHeader");

            migrationBuilder.DropColumn(
                name: "ViewedBy",
                schema: "Finance",
                table: "GeneralCashBankHeader");

            migrationBuilder.DropColumn(
                name: "ViewedDate",
                schema: "Finance",
                table: "GeneralCashBankHeader");

            migrationBuilder.DropColumn(
                name: "ViewedBy",
                schema: "AssetManagement",
                table: "FixedAsset");

            migrationBuilder.DropColumn(
                name: "ViewedDate",
                schema: "AssetManagement",
                table: "FixedAsset");

            migrationBuilder.DropColumn(
                name: "ViewedBy",
                schema: "Expedition",
                table: "ExpeditionInvoiceHeader");

            migrationBuilder.DropColumn(
                name: "ViewedDate",
                schema: "Expedition",
                table: "ExpeditionInvoiceHeader");

            migrationBuilder.DropColumn(
                name: "ViewedBy",
                schema: "Sales",
                table: "DeliveryPlanHeader");

            migrationBuilder.DropColumn(
                name: "ViewedDate",
                schema: "Sales",
                table: "DeliveryPlanHeader");

            migrationBuilder.DropColumn(
                name: "ViewedBy",
                schema: "Inventory",
                table: "AdjustmentHeader");

            migrationBuilder.DropColumn(
                name: "ViewedDate",
                schema: "Inventory",
                table: "AdjustmentHeader");
        }
    }
}
