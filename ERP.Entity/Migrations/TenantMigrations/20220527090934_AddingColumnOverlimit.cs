using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AddingColumnOverlimit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedDate",
                schema: "Sales",
                table: "VisitPlanHeader",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .Annotation("Relational:ColumnOrder", 104);

            migrationBuilder.AlterColumn<int>(
                name: "UpdatedBy",
                schema: "Sales",
                table: "VisitPlanHeader",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 103);

            migrationBuilder.AlterColumn<string>(
                name: "Mark",
                schema: "Sales",
                table: "VisitPlanHeader",
                type: "varchar(3)",
                unicode: false,
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(3)",
                oldUnicode: false,
                oldMaxLength: 3)
                .Annotation("Relational:ColumnOrder", 100);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                schema: "Sales",
                table: "VisitPlanHeader",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .Annotation("Relational:ColumnOrder", 102);

            migrationBuilder.AlterColumn<int>(
                name: "CreatedBy",
                schema: "Sales",
                table: "VisitPlanHeader",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ApprovedDate",
                schema: "Sales",
                table: "VisitPlanHeader",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 106);

            migrationBuilder.AlterColumn<int>(
                name: "ApprovedBy",
                schema: "Sales",
                table: "VisitPlanHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 105);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ViewedDate",
                schema: "Sales",
                table: "VisitOrder",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<int>(
                name: "ViewedBy",
                schema: "Sales",
                table: "VisitOrder",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedDate",
                schema: "Sales",
                table: "VisitOrder",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .Annotation("Relational:ColumnOrder", 104);

            migrationBuilder.AlterColumn<int>(
                name: "UpdatedBy",
                schema: "Sales",
                table: "VisitOrder",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 103);

            migrationBuilder.AlterColumn<string>(
                name: "Mark",
                schema: "Sales",
                table: "VisitOrder",
                type: "varchar(3)",
                unicode: false,
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(3)",
                oldUnicode: false,
                oldMaxLength: 3)
                .Annotation("Relational:ColumnOrder", 100);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                schema: "Sales",
                table: "VisitOrder",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .Annotation("Relational:ColumnOrder", 102);

            migrationBuilder.AlterColumn<int>(
                name: "CreatedBy",
                schema: "Sales",
                table: "VisitOrder",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ApprovedDate",
                schema: "Sales",
                table: "VisitOrder",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 106);

            migrationBuilder.AlterColumn<int>(
                name: "ApprovedBy",
                schema: "Sales",
                table: "VisitOrder",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 105);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ViewedDate",
                schema: "Inventory",
                table: "TransferStockHeader",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<int>(
                name: "ViewedBy",
                schema: "Inventory",
                table: "TransferStockHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedDate",
                schema: "Inventory",
                table: "TransferStockHeader",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .Annotation("Relational:ColumnOrder", 104);

            migrationBuilder.AlterColumn<int>(
                name: "UpdatedBy",
                schema: "Inventory",
                table: "TransferStockHeader",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 103);

            migrationBuilder.AlterColumn<string>(
                name: "Mark",
                schema: "Inventory",
                table: "TransferStockHeader",
                type: "varchar(3)",
                unicode: false,
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(3)",
                oldUnicode: false,
                oldMaxLength: 3)
                .Annotation("Relational:ColumnOrder", 100);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                schema: "Inventory",
                table: "TransferStockHeader",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .Annotation("Relational:ColumnOrder", 102);

            migrationBuilder.AlterColumn<int>(
                name: "CreatedBy",
                schema: "Inventory",
                table: "TransferStockHeader",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ApprovedDate",
                schema: "Inventory",
                table: "TransferStockHeader",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 106);

            migrationBuilder.AlterColumn<int>(
                name: "ApprovedBy",
                schema: "Inventory",
                table: "TransferStockHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 105);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ViewedDate",
                schema: "Sales",
                table: "SalesReturnHeader",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<int>(
                name: "ViewedBy",
                schema: "Sales",
                table: "SalesReturnHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedDate",
                schema: "Sales",
                table: "SalesReturnHeader",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .Annotation("Relational:ColumnOrder", 104);

            migrationBuilder.AlterColumn<int>(
                name: "UpdatedBy",
                schema: "Sales",
                table: "SalesReturnHeader",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 103);

            migrationBuilder.AlterColumn<string>(
                name: "Mark",
                schema: "Sales",
                table: "SalesReturnHeader",
                type: "varchar(3)",
                unicode: false,
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(3)",
                oldUnicode: false,
                oldMaxLength: 3)
                .Annotation("Relational:ColumnOrder", 100);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                schema: "Sales",
                table: "SalesReturnHeader",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .Annotation("Relational:ColumnOrder", 102);

            migrationBuilder.AlterColumn<int>(
                name: "CreatedBy",
                schema: "Sales",
                table: "SalesReturnHeader",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ApprovedDate",
                schema: "Sales",
                table: "SalesReturnHeader",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 106);

            migrationBuilder.AlterColumn<int>(
                name: "ApprovedBy",
                schema: "Sales",
                table: "SalesReturnHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 105);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ViewedDate",
                schema: "Sales",
                table: "SalesOrderHeader",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<int>(
                name: "ViewedBy",
                schema: "Sales",
                table: "SalesOrderHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedDate",
                schema: "Sales",
                table: "SalesOrderHeader",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .Annotation("Relational:ColumnOrder", 104);

            migrationBuilder.AlterColumn<int>(
                name: "UpdatedBy",
                schema: "Sales",
                table: "SalesOrderHeader",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 103);

            migrationBuilder.AlterColumn<string>(
                name: "Mark",
                schema: "Sales",
                table: "SalesOrderHeader",
                type: "varchar(3)",
                unicode: false,
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(3)",
                oldUnicode: false,
                oldMaxLength: 3)
                .Annotation("Relational:ColumnOrder", 100);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                schema: "Sales",
                table: "SalesOrderHeader",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .Annotation("Relational:ColumnOrder", 102);

            migrationBuilder.AlterColumn<int>(
                name: "CreatedBy",
                schema: "Sales",
                table: "SalesOrderHeader",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ApprovedDate",
                schema: "Sales",
                table: "SalesOrderHeader",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 106);

            migrationBuilder.AlterColumn<int>(
                name: "ApprovedBy",
                schema: "Sales",
                table: "SalesOrderHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 105);

            migrationBuilder.AddColumn<int>(
                name: "OverlimitApprovedBy",
                schema: "Sales",
                table: "SalesOrderHeader",
                type: "int",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 107);

            migrationBuilder.AddColumn<DateTime>(
                name: "OverlimitApprovedDate",
                schema: "Sales",
                table: "SalesOrderHeader",
                type: "datetime",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 108);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ViewedDate",
                schema: "Sales",
                table: "SalesInvoiceHeader",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<int>(
                name: "ViewedBy",
                schema: "Sales",
                table: "SalesInvoiceHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedDate",
                schema: "Sales",
                table: "SalesInvoiceHeader",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .Annotation("Relational:ColumnOrder", 104);

            migrationBuilder.AlterColumn<int>(
                name: "UpdatedBy",
                schema: "Sales",
                table: "SalesInvoiceHeader",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 103);

            migrationBuilder.AlterColumn<string>(
                name: "Mark",
                schema: "Sales",
                table: "SalesInvoiceHeader",
                type: "varchar(3)",
                unicode: false,
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(3)",
                oldUnicode: false,
                oldMaxLength: 3)
                .Annotation("Relational:ColumnOrder", 100);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                schema: "Sales",
                table: "SalesInvoiceHeader",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .Annotation("Relational:ColumnOrder", 102);

            migrationBuilder.AlterColumn<int>(
                name: "CreatedBy",
                schema: "Sales",
                table: "SalesInvoiceHeader",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ApprovedDate",
                schema: "Sales",
                table: "SalesInvoiceHeader",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 106);

            migrationBuilder.AlterColumn<int>(
                name: "ApprovedBy",
                schema: "Sales",
                table: "SalesInvoiceHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 105);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ViewedDate",
                schema: "Sales",
                table: "SalesDeliveryHeader",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<int>(
                name: "ViewedBy",
                schema: "Sales",
                table: "SalesDeliveryHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedDate",
                schema: "Sales",
                table: "SalesDeliveryHeader",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .Annotation("Relational:ColumnOrder", 104);

            migrationBuilder.AlterColumn<int>(
                name: "UpdatedBy",
                schema: "Sales",
                table: "SalesDeliveryHeader",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 103);

            migrationBuilder.AlterColumn<string>(
                name: "Mark",
                schema: "Sales",
                table: "SalesDeliveryHeader",
                type: "varchar(3)",
                unicode: false,
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(3)",
                oldUnicode: false,
                oldMaxLength: 3)
                .Annotation("Relational:ColumnOrder", 100);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                schema: "Sales",
                table: "SalesDeliveryHeader",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .Annotation("Relational:ColumnOrder", 102);

            migrationBuilder.AlterColumn<int>(
                name: "CreatedBy",
                schema: "Sales",
                table: "SalesDeliveryHeader",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ApprovedDate",
                schema: "Sales",
                table: "SalesDeliveryHeader",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 106);

            migrationBuilder.AlterColumn<int>(
                name: "ApprovedBy",
                schema: "Sales",
                table: "SalesDeliveryHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 105);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ViewedDate",
                schema: "Purchasing",
                table: "PurchaseReturnHeader",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<int>(
                name: "ViewedBy",
                schema: "Purchasing",
                table: "PurchaseReturnHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedDate",
                schema: "Purchasing",
                table: "PurchaseReturnHeader",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .Annotation("Relational:ColumnOrder", 104);

            migrationBuilder.AlterColumn<int>(
                name: "UpdatedBy",
                schema: "Purchasing",
                table: "PurchaseReturnHeader",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 103);

            migrationBuilder.AlterColumn<string>(
                name: "Mark",
                schema: "Purchasing",
                table: "PurchaseReturnHeader",
                type: "varchar(3)",
                unicode: false,
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(3)",
                oldUnicode: false,
                oldMaxLength: 3)
                .Annotation("Relational:ColumnOrder", 100);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                schema: "Purchasing",
                table: "PurchaseReturnHeader",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .Annotation("Relational:ColumnOrder", 102);

            migrationBuilder.AlterColumn<int>(
                name: "CreatedBy",
                schema: "Purchasing",
                table: "PurchaseReturnHeader",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ApprovedDate",
                schema: "Purchasing",
                table: "PurchaseReturnHeader",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 106);

            migrationBuilder.AlterColumn<int>(
                name: "ApprovedBy",
                schema: "Purchasing",
                table: "PurchaseReturnHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 105);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ViewedDate",
                schema: "Purchasing",
                table: "PurchaseReceiveHeader",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<int>(
                name: "ViewedBy",
                schema: "Purchasing",
                table: "PurchaseReceiveHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedDate",
                schema: "Purchasing",
                table: "PurchaseReceiveHeader",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .Annotation("Relational:ColumnOrder", 104);

            migrationBuilder.AlterColumn<int>(
                name: "UpdatedBy",
                schema: "Purchasing",
                table: "PurchaseReceiveHeader",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 103);

            migrationBuilder.AlterColumn<string>(
                name: "Mark",
                schema: "Purchasing",
                table: "PurchaseReceiveHeader",
                type: "varchar(3)",
                unicode: false,
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(3)",
                oldUnicode: false,
                oldMaxLength: 3)
                .Annotation("Relational:ColumnOrder", 100);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                schema: "Purchasing",
                table: "PurchaseReceiveHeader",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .Annotation("Relational:ColumnOrder", 102);

            migrationBuilder.AlterColumn<int>(
                name: "CreatedBy",
                schema: "Purchasing",
                table: "PurchaseReceiveHeader",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ApprovedDate",
                schema: "Purchasing",
                table: "PurchaseReceiveHeader",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 106);

            migrationBuilder.AlterColumn<int>(
                name: "ApprovedBy",
                schema: "Purchasing",
                table: "PurchaseReceiveHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 105);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ViewedDate",
                schema: "Purchasing",
                table: "PurchaseOrderHeader",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<int>(
                name: "ViewedBy",
                schema: "Purchasing",
                table: "PurchaseOrderHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedDate",
                schema: "Purchasing",
                table: "PurchaseOrderHeader",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .Annotation("Relational:ColumnOrder", 104);

            migrationBuilder.AlterColumn<int>(
                name: "UpdatedBy",
                schema: "Purchasing",
                table: "PurchaseOrderHeader",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 103);

            migrationBuilder.AlterColumn<string>(
                name: "Mark",
                schema: "Purchasing",
                table: "PurchaseOrderHeader",
                type: "varchar(3)",
                unicode: false,
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(3)",
                oldUnicode: false,
                oldMaxLength: 3)
                .Annotation("Relational:ColumnOrder", 100);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                schema: "Purchasing",
                table: "PurchaseOrderHeader",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .Annotation("Relational:ColumnOrder", 102);

            migrationBuilder.AlterColumn<int>(
                name: "CreatedBy",
                schema: "Purchasing",
                table: "PurchaseOrderHeader",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ApprovedDate",
                schema: "Purchasing",
                table: "PurchaseOrderHeader",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 106);

            migrationBuilder.AlterColumn<int>(
                name: "ApprovedBy",
                schema: "Purchasing",
                table: "PurchaseOrderHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 105);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ViewedDate",
                schema: "Purchasing",
                table: "PurchaseInvoiceHeader",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<int>(
                name: "ViewedBy",
                schema: "Purchasing",
                table: "PurchaseInvoiceHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedDate",
                schema: "Purchasing",
                table: "PurchaseInvoiceHeader",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .Annotation("Relational:ColumnOrder", 104);

            migrationBuilder.AlterColumn<int>(
                name: "UpdatedBy",
                schema: "Purchasing",
                table: "PurchaseInvoiceHeader",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 103);

            migrationBuilder.AlterColumn<string>(
                name: "Mark",
                schema: "Purchasing",
                table: "PurchaseInvoiceHeader",
                type: "varchar(3)",
                unicode: false,
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(3)",
                oldUnicode: false,
                oldMaxLength: 3)
                .Annotation("Relational:ColumnOrder", 100);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                schema: "Purchasing",
                table: "PurchaseInvoiceHeader",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .Annotation("Relational:ColumnOrder", 102);

            migrationBuilder.AlterColumn<int>(
                name: "CreatedBy",
                schema: "Purchasing",
                table: "PurchaseInvoiceHeader",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ApprovedDate",
                schema: "Purchasing",
                table: "PurchaseInvoiceHeader",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 106);

            migrationBuilder.AlterColumn<int>(
                name: "ApprovedBy",
                schema: "Purchasing",
                table: "PurchaseInvoiceHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 105);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ViewedDate",
                schema: "Sales",
                table: "PromoHeader",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<int>(
                name: "ViewedBy",
                schema: "Sales",
                table: "PromoHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedDate",
                schema: "Sales",
                table: "PromoHeader",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .Annotation("Relational:ColumnOrder", 104);

            migrationBuilder.AlterColumn<int>(
                name: "UpdatedBy",
                schema: "Sales",
                table: "PromoHeader",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 103);

            migrationBuilder.AlterColumn<string>(
                name: "Mark",
                schema: "Sales",
                table: "PromoHeader",
                type: "varchar(3)",
                unicode: false,
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(3)",
                oldUnicode: false,
                oldMaxLength: 3)
                .Annotation("Relational:ColumnOrder", 100);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                schema: "Sales",
                table: "PromoHeader",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .Annotation("Relational:ColumnOrder", 102);

            migrationBuilder.AlterColumn<int>(
                name: "CreatedBy",
                schema: "Sales",
                table: "PromoHeader",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ApprovedDate",
                schema: "Sales",
                table: "PromoHeader",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 106);

            migrationBuilder.AlterColumn<int>(
                name: "ApprovedBy",
                schema: "Sales",
                table: "PromoHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 105);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ViewedDate",
                schema: "Accounting",
                table: "GeneralJournalHeader",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<int>(
                name: "ViewedBy",
                schema: "Accounting",
                table: "GeneralJournalHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedDate",
                schema: "Accounting",
                table: "GeneralJournalHeader",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .Annotation("Relational:ColumnOrder", 104);

            migrationBuilder.AlterColumn<int>(
                name: "UpdatedBy",
                schema: "Accounting",
                table: "GeneralJournalHeader",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 103);

            migrationBuilder.AlterColumn<string>(
                name: "Mark",
                schema: "Accounting",
                table: "GeneralJournalHeader",
                type: "varchar(3)",
                unicode: false,
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(3)",
                oldUnicode: false,
                oldMaxLength: 3)
                .Annotation("Relational:ColumnOrder", 100);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                schema: "Accounting",
                table: "GeneralJournalHeader",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .Annotation("Relational:ColumnOrder", 102);

            migrationBuilder.AlterColumn<int>(
                name: "CreatedBy",
                schema: "Accounting",
                table: "GeneralJournalHeader",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ApprovedDate",
                schema: "Accounting",
                table: "GeneralJournalHeader",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 106);

            migrationBuilder.AlterColumn<int>(
                name: "ApprovedBy",
                schema: "Accounting",
                table: "GeneralJournalHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 105);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ViewedDate",
                schema: "Finance",
                table: "GeneralCashBankHeader",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<int>(
                name: "ViewedBy",
                schema: "Finance",
                table: "GeneralCashBankHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedDate",
                schema: "Finance",
                table: "GeneralCashBankHeader",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .Annotation("Relational:ColumnOrder", 104);

            migrationBuilder.AlterColumn<int>(
                name: "UpdatedBy",
                schema: "Finance",
                table: "GeneralCashBankHeader",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 103);

            migrationBuilder.AlterColumn<string>(
                name: "Mark",
                schema: "Finance",
                table: "GeneralCashBankHeader",
                type: "varchar(3)",
                unicode: false,
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(3)",
                oldUnicode: false,
                oldMaxLength: 3)
                .Annotation("Relational:ColumnOrder", 100);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                schema: "Finance",
                table: "GeneralCashBankHeader",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .Annotation("Relational:ColumnOrder", 102);

            migrationBuilder.AlterColumn<int>(
                name: "CreatedBy",
                schema: "Finance",
                table: "GeneralCashBankHeader",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ApprovedDate",
                schema: "Finance",
                table: "GeneralCashBankHeader",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 106);

            migrationBuilder.AlterColumn<int>(
                name: "ApprovedBy",
                schema: "Finance",
                table: "GeneralCashBankHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 105);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ViewedDate",
                schema: "AssetManagement",
                table: "FixedAsset",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<int>(
                name: "ViewedBy",
                schema: "AssetManagement",
                table: "FixedAsset",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedDate",
                schema: "AssetManagement",
                table: "FixedAsset",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .Annotation("Relational:ColumnOrder", 104);

            migrationBuilder.AlterColumn<int>(
                name: "UpdatedBy",
                schema: "AssetManagement",
                table: "FixedAsset",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 103);

            migrationBuilder.AlterColumn<string>(
                name: "Mark",
                schema: "AssetManagement",
                table: "FixedAsset",
                type: "varchar(3)",
                unicode: false,
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(3)",
                oldUnicode: false,
                oldMaxLength: 3)
                .Annotation("Relational:ColumnOrder", 100);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                schema: "AssetManagement",
                table: "FixedAsset",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .Annotation("Relational:ColumnOrder", 102);

            migrationBuilder.AlterColumn<int>(
                name: "CreatedBy",
                schema: "AssetManagement",
                table: "FixedAsset",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ApprovedDate",
                schema: "AssetManagement",
                table: "FixedAsset",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 106);

            migrationBuilder.AlterColumn<int>(
                name: "ApprovedBy",
                schema: "AssetManagement",
                table: "FixedAsset",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 105);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ViewedDate",
                schema: "Expedition",
                table: "ExpeditionInvoiceHeader",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<int>(
                name: "ViewedBy",
                schema: "Expedition",
                table: "ExpeditionInvoiceHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedDate",
                schema: "Expedition",
                table: "ExpeditionInvoiceHeader",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .Annotation("Relational:ColumnOrder", 104);

            migrationBuilder.AlterColumn<int>(
                name: "UpdatedBy",
                schema: "Expedition",
                table: "ExpeditionInvoiceHeader",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 103);

            migrationBuilder.AlterColumn<string>(
                name: "Mark",
                schema: "Expedition",
                table: "ExpeditionInvoiceHeader",
                type: "varchar(3)",
                unicode: false,
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(3)",
                oldUnicode: false,
                oldMaxLength: 3)
                .Annotation("Relational:ColumnOrder", 100);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                schema: "Expedition",
                table: "ExpeditionInvoiceHeader",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .Annotation("Relational:ColumnOrder", 102);

            migrationBuilder.AlterColumn<int>(
                name: "CreatedBy",
                schema: "Expedition",
                table: "ExpeditionInvoiceHeader",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ApprovedDate",
                schema: "Expedition",
                table: "ExpeditionInvoiceHeader",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 106);

            migrationBuilder.AlterColumn<int>(
                name: "ApprovedBy",
                schema: "Expedition",
                table: "ExpeditionInvoiceHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 105);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ViewedDate",
                schema: "Sales",
                table: "DeliveryPlanHeader",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<int>(
                name: "ViewedBy",
                schema: "Sales",
                table: "DeliveryPlanHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedDate",
                schema: "Sales",
                table: "DeliveryPlanHeader",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .Annotation("Relational:ColumnOrder", 104);

            migrationBuilder.AlterColumn<int>(
                name: "UpdatedBy",
                schema: "Sales",
                table: "DeliveryPlanHeader",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 103);

            migrationBuilder.AlterColumn<string>(
                name: "Mark",
                schema: "Sales",
                table: "DeliveryPlanHeader",
                type: "varchar(3)",
                unicode: false,
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(3)",
                oldUnicode: false,
                oldMaxLength: 3)
                .Annotation("Relational:ColumnOrder", 100);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                schema: "Sales",
                table: "DeliveryPlanHeader",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .Annotation("Relational:ColumnOrder", 102);

            migrationBuilder.AlterColumn<int>(
                name: "CreatedBy",
                schema: "Sales",
                table: "DeliveryPlanHeader",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ApprovedDate",
                schema: "Sales",
                table: "DeliveryPlanHeader",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 106);

            migrationBuilder.AlterColumn<int>(
                name: "ApprovedBy",
                schema: "Sales",
                table: "DeliveryPlanHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 105);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ViewedDate",
                schema: "Inventory",
                table: "AdjustmentHeader",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<int>(
                name: "ViewedBy",
                schema: "Inventory",
                table: "AdjustmentHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedDate",
                schema: "Inventory",
                table: "AdjustmentHeader",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .Annotation("Relational:ColumnOrder", 104);

            migrationBuilder.AlterColumn<int>(
                name: "UpdatedBy",
                schema: "Inventory",
                table: "AdjustmentHeader",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 103);

            migrationBuilder.AlterColumn<string>(
                name: "Mark",
                schema: "Inventory",
                table: "AdjustmentHeader",
                type: "varchar(3)",
                unicode: false,
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(3)",
                oldUnicode: false,
                oldMaxLength: 3)
                .Annotation("Relational:ColumnOrder", 100);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                schema: "Inventory",
                table: "AdjustmentHeader",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .Annotation("Relational:ColumnOrder", 102);

            migrationBuilder.AlterColumn<int>(
                name: "CreatedBy",
                schema: "Inventory",
                table: "AdjustmentHeader",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ApprovedDate",
                schema: "Inventory",
                table: "AdjustmentHeader",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 106);

            migrationBuilder.AlterColumn<int>(
                name: "ApprovedBy",
                schema: "Inventory",
                table: "AdjustmentHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 105);

            // Alter view Sales.vwSalesOrderHeader
            var sql = @"ALTER VIEW [Sales].[vwSalesOrderHeader]
AS
    SELECT so_h.*,
        c.[Name] AS CustName,
        e.Initial AS SalesInitial,
		e.FirstName AS SalesName,
        u_c.Initial AS CreatedInitial,
        u_u.Initial AS UpdatedInitial,
        u_a.Initial AS ApprovedInitial,
        u_o.Initial AS OverlimitApprovedInitial,
        CASE so_h.Mark
            WHEN 'A' THEN 'Active'
            WHEN 'V' THEN 'Void'
            WHEN 'OL' THEN 'Overlimit'
            WHEN 'PS' THEN 'Partial Shipped'
            WHEN 'CMP' THEN 'Completed'
            WHEN 'CLS' THEN 'Closed' END AS [Status]
    FROM Sales.SalesOrderHeader so_h
    LEFT JOIN General.Customer c
        ON c.Code = so_h.CustCode
    LEFT JOIN General.Employee e
        ON e.Id = so_h.SalesBy
    LEFT JOIN SystemManagement.[User] u_c
        ON u_c.Id = so_h.CreatedBy
    LEFT JOIN SystemManagement.[User] u_u
        ON u_u.Id = so_h.UpdatedBy
    LEFT JOIN SystemManagement.[User] u_a
        ON u_a.Id = so_h.ApprovedBy
    LEFT JOIN SystemManagement.[User] u_o
        ON u_o.Id = so_h.OverlimitApprovedBy";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OverlimitApprovedBy",
                schema: "Sales",
                table: "SalesOrderHeader");

            migrationBuilder.DropColumn(
                name: "OverlimitApprovedDate",
                schema: "Sales",
                table: "SalesOrderHeader");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedDate",
                schema: "Sales",
                table: "VisitPlanHeader",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .OldAnnotation("Relational:ColumnOrder", 104);

            migrationBuilder.AlterColumn<int>(
                name: "UpdatedBy",
                schema: "Sales",
                table: "VisitPlanHeader",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 103);

            migrationBuilder.AlterColumn<string>(
                name: "Mark",
                schema: "Sales",
                table: "VisitPlanHeader",
                type: "varchar(3)",
                unicode: false,
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(3)",
                oldUnicode: false,
                oldMaxLength: 3)
                .OldAnnotation("Relational:ColumnOrder", 100);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                schema: "Sales",
                table: "VisitPlanHeader",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .OldAnnotation("Relational:ColumnOrder", 102);

            migrationBuilder.AlterColumn<int>(
                name: "CreatedBy",
                schema: "Sales",
                table: "VisitPlanHeader",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ApprovedDate",
                schema: "Sales",
                table: "VisitPlanHeader",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 106);

            migrationBuilder.AlterColumn<int>(
                name: "ApprovedBy",
                schema: "Sales",
                table: "VisitPlanHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 105);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ViewedDate",
                schema: "Sales",
                table: "VisitOrder",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<int>(
                name: "ViewedBy",
                schema: "Sales",
                table: "VisitOrder",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedDate",
                schema: "Sales",
                table: "VisitOrder",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .OldAnnotation("Relational:ColumnOrder", 104);

            migrationBuilder.AlterColumn<int>(
                name: "UpdatedBy",
                schema: "Sales",
                table: "VisitOrder",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 103);

            migrationBuilder.AlterColumn<string>(
                name: "Mark",
                schema: "Sales",
                table: "VisitOrder",
                type: "varchar(3)",
                unicode: false,
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(3)",
                oldUnicode: false,
                oldMaxLength: 3)
                .OldAnnotation("Relational:ColumnOrder", 100);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                schema: "Sales",
                table: "VisitOrder",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .OldAnnotation("Relational:ColumnOrder", 102);

            migrationBuilder.AlterColumn<int>(
                name: "CreatedBy",
                schema: "Sales",
                table: "VisitOrder",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ApprovedDate",
                schema: "Sales",
                table: "VisitOrder",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 106);

            migrationBuilder.AlterColumn<int>(
                name: "ApprovedBy",
                schema: "Sales",
                table: "VisitOrder",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 105);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ViewedDate",
                schema: "Inventory",
                table: "TransferStockHeader",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<int>(
                name: "ViewedBy",
                schema: "Inventory",
                table: "TransferStockHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedDate",
                schema: "Inventory",
                table: "TransferStockHeader",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .OldAnnotation("Relational:ColumnOrder", 104);

            migrationBuilder.AlterColumn<int>(
                name: "UpdatedBy",
                schema: "Inventory",
                table: "TransferStockHeader",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 103);

            migrationBuilder.AlterColumn<string>(
                name: "Mark",
                schema: "Inventory",
                table: "TransferStockHeader",
                type: "varchar(3)",
                unicode: false,
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(3)",
                oldUnicode: false,
                oldMaxLength: 3)
                .OldAnnotation("Relational:ColumnOrder", 100);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                schema: "Inventory",
                table: "TransferStockHeader",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .OldAnnotation("Relational:ColumnOrder", 102);

            migrationBuilder.AlterColumn<int>(
                name: "CreatedBy",
                schema: "Inventory",
                table: "TransferStockHeader",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ApprovedDate",
                schema: "Inventory",
                table: "TransferStockHeader",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 106);

            migrationBuilder.AlterColumn<int>(
                name: "ApprovedBy",
                schema: "Inventory",
                table: "TransferStockHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 105);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ViewedDate",
                schema: "Sales",
                table: "SalesReturnHeader",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<int>(
                name: "ViewedBy",
                schema: "Sales",
                table: "SalesReturnHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedDate",
                schema: "Sales",
                table: "SalesReturnHeader",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .OldAnnotation("Relational:ColumnOrder", 104);

            migrationBuilder.AlterColumn<int>(
                name: "UpdatedBy",
                schema: "Sales",
                table: "SalesReturnHeader",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 103);

            migrationBuilder.AlterColumn<string>(
                name: "Mark",
                schema: "Sales",
                table: "SalesReturnHeader",
                type: "varchar(3)",
                unicode: false,
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(3)",
                oldUnicode: false,
                oldMaxLength: 3)
                .OldAnnotation("Relational:ColumnOrder", 100);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                schema: "Sales",
                table: "SalesReturnHeader",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .OldAnnotation("Relational:ColumnOrder", 102);

            migrationBuilder.AlterColumn<int>(
                name: "CreatedBy",
                schema: "Sales",
                table: "SalesReturnHeader",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ApprovedDate",
                schema: "Sales",
                table: "SalesReturnHeader",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 106);

            migrationBuilder.AlterColumn<int>(
                name: "ApprovedBy",
                schema: "Sales",
                table: "SalesReturnHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 105);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ViewedDate",
                schema: "Sales",
                table: "SalesOrderHeader",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<int>(
                name: "ViewedBy",
                schema: "Sales",
                table: "SalesOrderHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedDate",
                schema: "Sales",
                table: "SalesOrderHeader",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .OldAnnotation("Relational:ColumnOrder", 104);

            migrationBuilder.AlterColumn<int>(
                name: "UpdatedBy",
                schema: "Sales",
                table: "SalesOrderHeader",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 103);

            migrationBuilder.AlterColumn<string>(
                name: "Mark",
                schema: "Sales",
                table: "SalesOrderHeader",
                type: "varchar(3)",
                unicode: false,
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(3)",
                oldUnicode: false,
                oldMaxLength: 3)
                .OldAnnotation("Relational:ColumnOrder", 100);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                schema: "Sales",
                table: "SalesOrderHeader",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .OldAnnotation("Relational:ColumnOrder", 102);

            migrationBuilder.AlterColumn<int>(
                name: "CreatedBy",
                schema: "Sales",
                table: "SalesOrderHeader",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ApprovedDate",
                schema: "Sales",
                table: "SalesOrderHeader",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 106);

            migrationBuilder.AlterColumn<int>(
                name: "ApprovedBy",
                schema: "Sales",
                table: "SalesOrderHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 105);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ViewedDate",
                schema: "Sales",
                table: "SalesInvoiceHeader",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<int>(
                name: "ViewedBy",
                schema: "Sales",
                table: "SalesInvoiceHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedDate",
                schema: "Sales",
                table: "SalesInvoiceHeader",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .OldAnnotation("Relational:ColumnOrder", 104);

            migrationBuilder.AlterColumn<int>(
                name: "UpdatedBy",
                schema: "Sales",
                table: "SalesInvoiceHeader",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 103);

            migrationBuilder.AlterColumn<string>(
                name: "Mark",
                schema: "Sales",
                table: "SalesInvoiceHeader",
                type: "varchar(3)",
                unicode: false,
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(3)",
                oldUnicode: false,
                oldMaxLength: 3)
                .OldAnnotation("Relational:ColumnOrder", 100);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                schema: "Sales",
                table: "SalesInvoiceHeader",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .OldAnnotation("Relational:ColumnOrder", 102);

            migrationBuilder.AlterColumn<int>(
                name: "CreatedBy",
                schema: "Sales",
                table: "SalesInvoiceHeader",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ApprovedDate",
                schema: "Sales",
                table: "SalesInvoiceHeader",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 106);

            migrationBuilder.AlterColumn<int>(
                name: "ApprovedBy",
                schema: "Sales",
                table: "SalesInvoiceHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 105);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ViewedDate",
                schema: "Sales",
                table: "SalesDeliveryHeader",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<int>(
                name: "ViewedBy",
                schema: "Sales",
                table: "SalesDeliveryHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedDate",
                schema: "Sales",
                table: "SalesDeliveryHeader",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .OldAnnotation("Relational:ColumnOrder", 104);

            migrationBuilder.AlterColumn<int>(
                name: "UpdatedBy",
                schema: "Sales",
                table: "SalesDeliveryHeader",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 103);

            migrationBuilder.AlterColumn<string>(
                name: "Mark",
                schema: "Sales",
                table: "SalesDeliveryHeader",
                type: "varchar(3)",
                unicode: false,
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(3)",
                oldUnicode: false,
                oldMaxLength: 3)
                .OldAnnotation("Relational:ColumnOrder", 100);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                schema: "Sales",
                table: "SalesDeliveryHeader",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .OldAnnotation("Relational:ColumnOrder", 102);

            migrationBuilder.AlterColumn<int>(
                name: "CreatedBy",
                schema: "Sales",
                table: "SalesDeliveryHeader",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ApprovedDate",
                schema: "Sales",
                table: "SalesDeliveryHeader",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 106);

            migrationBuilder.AlterColumn<int>(
                name: "ApprovedBy",
                schema: "Sales",
                table: "SalesDeliveryHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 105);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ViewedDate",
                schema: "Purchasing",
                table: "PurchaseReturnHeader",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<int>(
                name: "ViewedBy",
                schema: "Purchasing",
                table: "PurchaseReturnHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedDate",
                schema: "Purchasing",
                table: "PurchaseReturnHeader",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .OldAnnotation("Relational:ColumnOrder", 104);

            migrationBuilder.AlterColumn<int>(
                name: "UpdatedBy",
                schema: "Purchasing",
                table: "PurchaseReturnHeader",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 103);

            migrationBuilder.AlterColumn<string>(
                name: "Mark",
                schema: "Purchasing",
                table: "PurchaseReturnHeader",
                type: "varchar(3)",
                unicode: false,
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(3)",
                oldUnicode: false,
                oldMaxLength: 3)
                .OldAnnotation("Relational:ColumnOrder", 100);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                schema: "Purchasing",
                table: "PurchaseReturnHeader",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .OldAnnotation("Relational:ColumnOrder", 102);

            migrationBuilder.AlterColumn<int>(
                name: "CreatedBy",
                schema: "Purchasing",
                table: "PurchaseReturnHeader",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ApprovedDate",
                schema: "Purchasing",
                table: "PurchaseReturnHeader",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 106);

            migrationBuilder.AlterColumn<int>(
                name: "ApprovedBy",
                schema: "Purchasing",
                table: "PurchaseReturnHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 105);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ViewedDate",
                schema: "Purchasing",
                table: "PurchaseReceiveHeader",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<int>(
                name: "ViewedBy",
                schema: "Purchasing",
                table: "PurchaseReceiveHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedDate",
                schema: "Purchasing",
                table: "PurchaseReceiveHeader",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .OldAnnotation("Relational:ColumnOrder", 104);

            migrationBuilder.AlterColumn<int>(
                name: "UpdatedBy",
                schema: "Purchasing",
                table: "PurchaseReceiveHeader",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 103);

            migrationBuilder.AlterColumn<string>(
                name: "Mark",
                schema: "Purchasing",
                table: "PurchaseReceiveHeader",
                type: "varchar(3)",
                unicode: false,
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(3)",
                oldUnicode: false,
                oldMaxLength: 3)
                .OldAnnotation("Relational:ColumnOrder", 100);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                schema: "Purchasing",
                table: "PurchaseReceiveHeader",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .OldAnnotation("Relational:ColumnOrder", 102);

            migrationBuilder.AlterColumn<int>(
                name: "CreatedBy",
                schema: "Purchasing",
                table: "PurchaseReceiveHeader",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ApprovedDate",
                schema: "Purchasing",
                table: "PurchaseReceiveHeader",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 106);

            migrationBuilder.AlterColumn<int>(
                name: "ApprovedBy",
                schema: "Purchasing",
                table: "PurchaseReceiveHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 105);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ViewedDate",
                schema: "Purchasing",
                table: "PurchaseOrderHeader",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<int>(
                name: "ViewedBy",
                schema: "Purchasing",
                table: "PurchaseOrderHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedDate",
                schema: "Purchasing",
                table: "PurchaseOrderHeader",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .OldAnnotation("Relational:ColumnOrder", 104);

            migrationBuilder.AlterColumn<int>(
                name: "UpdatedBy",
                schema: "Purchasing",
                table: "PurchaseOrderHeader",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 103);

            migrationBuilder.AlterColumn<string>(
                name: "Mark",
                schema: "Purchasing",
                table: "PurchaseOrderHeader",
                type: "varchar(3)",
                unicode: false,
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(3)",
                oldUnicode: false,
                oldMaxLength: 3)
                .OldAnnotation("Relational:ColumnOrder", 100);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                schema: "Purchasing",
                table: "PurchaseOrderHeader",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .OldAnnotation("Relational:ColumnOrder", 102);

            migrationBuilder.AlterColumn<int>(
                name: "CreatedBy",
                schema: "Purchasing",
                table: "PurchaseOrderHeader",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ApprovedDate",
                schema: "Purchasing",
                table: "PurchaseOrderHeader",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 106);

            migrationBuilder.AlterColumn<int>(
                name: "ApprovedBy",
                schema: "Purchasing",
                table: "PurchaseOrderHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 105);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ViewedDate",
                schema: "Purchasing",
                table: "PurchaseInvoiceHeader",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<int>(
                name: "ViewedBy",
                schema: "Purchasing",
                table: "PurchaseInvoiceHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedDate",
                schema: "Purchasing",
                table: "PurchaseInvoiceHeader",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .OldAnnotation("Relational:ColumnOrder", 104);

            migrationBuilder.AlterColumn<int>(
                name: "UpdatedBy",
                schema: "Purchasing",
                table: "PurchaseInvoiceHeader",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 103);

            migrationBuilder.AlterColumn<string>(
                name: "Mark",
                schema: "Purchasing",
                table: "PurchaseInvoiceHeader",
                type: "varchar(3)",
                unicode: false,
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(3)",
                oldUnicode: false,
                oldMaxLength: 3)
                .OldAnnotation("Relational:ColumnOrder", 100);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                schema: "Purchasing",
                table: "PurchaseInvoiceHeader",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .OldAnnotation("Relational:ColumnOrder", 102);

            migrationBuilder.AlterColumn<int>(
                name: "CreatedBy",
                schema: "Purchasing",
                table: "PurchaseInvoiceHeader",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ApprovedDate",
                schema: "Purchasing",
                table: "PurchaseInvoiceHeader",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 106);

            migrationBuilder.AlterColumn<int>(
                name: "ApprovedBy",
                schema: "Purchasing",
                table: "PurchaseInvoiceHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 105);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ViewedDate",
                schema: "Sales",
                table: "PromoHeader",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<int>(
                name: "ViewedBy",
                schema: "Sales",
                table: "PromoHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedDate",
                schema: "Sales",
                table: "PromoHeader",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .OldAnnotation("Relational:ColumnOrder", 104);

            migrationBuilder.AlterColumn<int>(
                name: "UpdatedBy",
                schema: "Sales",
                table: "PromoHeader",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 103);

            migrationBuilder.AlterColumn<string>(
                name: "Mark",
                schema: "Sales",
                table: "PromoHeader",
                type: "varchar(3)",
                unicode: false,
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(3)",
                oldUnicode: false,
                oldMaxLength: 3)
                .OldAnnotation("Relational:ColumnOrder", 100);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                schema: "Sales",
                table: "PromoHeader",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .OldAnnotation("Relational:ColumnOrder", 102);

            migrationBuilder.AlterColumn<int>(
                name: "CreatedBy",
                schema: "Sales",
                table: "PromoHeader",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ApprovedDate",
                schema: "Sales",
                table: "PromoHeader",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 106);

            migrationBuilder.AlterColumn<int>(
                name: "ApprovedBy",
                schema: "Sales",
                table: "PromoHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 105);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ViewedDate",
                schema: "Accounting",
                table: "GeneralJournalHeader",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<int>(
                name: "ViewedBy",
                schema: "Accounting",
                table: "GeneralJournalHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedDate",
                schema: "Accounting",
                table: "GeneralJournalHeader",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .OldAnnotation("Relational:ColumnOrder", 104);

            migrationBuilder.AlterColumn<int>(
                name: "UpdatedBy",
                schema: "Accounting",
                table: "GeneralJournalHeader",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 103);

            migrationBuilder.AlterColumn<string>(
                name: "Mark",
                schema: "Accounting",
                table: "GeneralJournalHeader",
                type: "varchar(3)",
                unicode: false,
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(3)",
                oldUnicode: false,
                oldMaxLength: 3)
                .OldAnnotation("Relational:ColumnOrder", 100);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                schema: "Accounting",
                table: "GeneralJournalHeader",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .OldAnnotation("Relational:ColumnOrder", 102);

            migrationBuilder.AlterColumn<int>(
                name: "CreatedBy",
                schema: "Accounting",
                table: "GeneralJournalHeader",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ApprovedDate",
                schema: "Accounting",
                table: "GeneralJournalHeader",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 106);

            migrationBuilder.AlterColumn<int>(
                name: "ApprovedBy",
                schema: "Accounting",
                table: "GeneralJournalHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 105);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ViewedDate",
                schema: "Finance",
                table: "GeneralCashBankHeader",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<int>(
                name: "ViewedBy",
                schema: "Finance",
                table: "GeneralCashBankHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedDate",
                schema: "Finance",
                table: "GeneralCashBankHeader",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .OldAnnotation("Relational:ColumnOrder", 104);

            migrationBuilder.AlterColumn<int>(
                name: "UpdatedBy",
                schema: "Finance",
                table: "GeneralCashBankHeader",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 103);

            migrationBuilder.AlterColumn<string>(
                name: "Mark",
                schema: "Finance",
                table: "GeneralCashBankHeader",
                type: "varchar(3)",
                unicode: false,
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(3)",
                oldUnicode: false,
                oldMaxLength: 3)
                .OldAnnotation("Relational:ColumnOrder", 100);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                schema: "Finance",
                table: "GeneralCashBankHeader",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .OldAnnotation("Relational:ColumnOrder", 102);

            migrationBuilder.AlterColumn<int>(
                name: "CreatedBy",
                schema: "Finance",
                table: "GeneralCashBankHeader",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ApprovedDate",
                schema: "Finance",
                table: "GeneralCashBankHeader",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 106);

            migrationBuilder.AlterColumn<int>(
                name: "ApprovedBy",
                schema: "Finance",
                table: "GeneralCashBankHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 105);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ViewedDate",
                schema: "AssetManagement",
                table: "FixedAsset",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<int>(
                name: "ViewedBy",
                schema: "AssetManagement",
                table: "FixedAsset",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedDate",
                schema: "AssetManagement",
                table: "FixedAsset",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .OldAnnotation("Relational:ColumnOrder", 104);

            migrationBuilder.AlterColumn<int>(
                name: "UpdatedBy",
                schema: "AssetManagement",
                table: "FixedAsset",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 103);

            migrationBuilder.AlterColumn<string>(
                name: "Mark",
                schema: "AssetManagement",
                table: "FixedAsset",
                type: "varchar(3)",
                unicode: false,
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(3)",
                oldUnicode: false,
                oldMaxLength: 3)
                .OldAnnotation("Relational:ColumnOrder", 100);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                schema: "AssetManagement",
                table: "FixedAsset",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .OldAnnotation("Relational:ColumnOrder", 102);

            migrationBuilder.AlterColumn<int>(
                name: "CreatedBy",
                schema: "AssetManagement",
                table: "FixedAsset",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ApprovedDate",
                schema: "AssetManagement",
                table: "FixedAsset",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 106);

            migrationBuilder.AlterColumn<int>(
                name: "ApprovedBy",
                schema: "AssetManagement",
                table: "FixedAsset",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 105);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ViewedDate",
                schema: "Expedition",
                table: "ExpeditionInvoiceHeader",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<int>(
                name: "ViewedBy",
                schema: "Expedition",
                table: "ExpeditionInvoiceHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedDate",
                schema: "Expedition",
                table: "ExpeditionInvoiceHeader",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .OldAnnotation("Relational:ColumnOrder", 104);

            migrationBuilder.AlterColumn<int>(
                name: "UpdatedBy",
                schema: "Expedition",
                table: "ExpeditionInvoiceHeader",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 103);

            migrationBuilder.AlterColumn<string>(
                name: "Mark",
                schema: "Expedition",
                table: "ExpeditionInvoiceHeader",
                type: "varchar(3)",
                unicode: false,
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(3)",
                oldUnicode: false,
                oldMaxLength: 3)
                .OldAnnotation("Relational:ColumnOrder", 100);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                schema: "Expedition",
                table: "ExpeditionInvoiceHeader",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .OldAnnotation("Relational:ColumnOrder", 102);

            migrationBuilder.AlterColumn<int>(
                name: "CreatedBy",
                schema: "Expedition",
                table: "ExpeditionInvoiceHeader",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ApprovedDate",
                schema: "Expedition",
                table: "ExpeditionInvoiceHeader",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 106);

            migrationBuilder.AlterColumn<int>(
                name: "ApprovedBy",
                schema: "Expedition",
                table: "ExpeditionInvoiceHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 105);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ViewedDate",
                schema: "Sales",
                table: "DeliveryPlanHeader",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<int>(
                name: "ViewedBy",
                schema: "Sales",
                table: "DeliveryPlanHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedDate",
                schema: "Sales",
                table: "DeliveryPlanHeader",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .OldAnnotation("Relational:ColumnOrder", 104);

            migrationBuilder.AlterColumn<int>(
                name: "UpdatedBy",
                schema: "Sales",
                table: "DeliveryPlanHeader",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 103);

            migrationBuilder.AlterColumn<string>(
                name: "Mark",
                schema: "Sales",
                table: "DeliveryPlanHeader",
                type: "varchar(3)",
                unicode: false,
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(3)",
                oldUnicode: false,
                oldMaxLength: 3)
                .OldAnnotation("Relational:ColumnOrder", 100);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                schema: "Sales",
                table: "DeliveryPlanHeader",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .OldAnnotation("Relational:ColumnOrder", 102);

            migrationBuilder.AlterColumn<int>(
                name: "CreatedBy",
                schema: "Sales",
                table: "DeliveryPlanHeader",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ApprovedDate",
                schema: "Sales",
                table: "DeliveryPlanHeader",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 106);

            migrationBuilder.AlterColumn<int>(
                name: "ApprovedBy",
                schema: "Sales",
                table: "DeliveryPlanHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 105);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ViewedDate",
                schema: "Inventory",
                table: "AdjustmentHeader",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<int>(
                name: "ViewedBy",
                schema: "Inventory",
                table: "AdjustmentHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedDate",
                schema: "Inventory",
                table: "AdjustmentHeader",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .OldAnnotation("Relational:ColumnOrder", 104);

            migrationBuilder.AlterColumn<int>(
                name: "UpdatedBy",
                schema: "Inventory",
                table: "AdjustmentHeader",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 103);

            migrationBuilder.AlterColumn<string>(
                name: "Mark",
                schema: "Inventory",
                table: "AdjustmentHeader",
                type: "varchar(3)",
                unicode: false,
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(3)",
                oldUnicode: false,
                oldMaxLength: 3)
                .OldAnnotation("Relational:ColumnOrder", 100);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                schema: "Inventory",
                table: "AdjustmentHeader",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime")
                .OldAnnotation("Relational:ColumnOrder", 102);

            migrationBuilder.AlterColumn<int>(
                name: "CreatedBy",
                schema: "Inventory",
                table: "AdjustmentHeader",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 101);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ApprovedDate",
                schema: "Inventory",
                table: "AdjustmentHeader",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 106);

            migrationBuilder.AlterColumn<int>(
                name: "ApprovedBy",
                schema: "Inventory",
                table: "AdjustmentHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 105);
        }
    }
}
