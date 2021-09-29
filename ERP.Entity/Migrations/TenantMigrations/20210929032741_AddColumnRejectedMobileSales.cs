using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AddColumnRejectedMobileSales : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RejectedBy",
                schema: "MobileSales",
                table: "MobileVisitLog",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RejectedDate",
                schema: "MobileSales",
                table: "MobileVisitLog",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RejectedBy",
                schema: "MobileSales",
                table: "MobilePaymentInvoice",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RejectedDate",
                schema: "MobileSales",
                table: "MobilePaymentInvoice",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RejectedBy",
                schema: "MobileSales",
                table: "MobileOrderHeader",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RejectedDate",
                schema: "MobileSales",
                table: "MobileOrderHeader",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RejectedBy",
                schema: "MobileSales",
                table: "MobileItemRequestHeader",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RejectedDate",
                schema: "MobileSales",
                table: "MobileItemRequestHeader",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RejectedBy",
                schema: "MobileSales",
                table: "MobileCostHeader",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RejectedDate",
                schema: "MobileSales",
                table: "MobileCostHeader",
                type: "datetime",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RejectedBy",
                schema: "MobileSales",
                table: "MobileVisitLog");

            migrationBuilder.DropColumn(
                name: "RejectedDate",
                schema: "MobileSales",
                table: "MobileVisitLog");

            migrationBuilder.DropColumn(
                name: "RejectedBy",
                schema: "MobileSales",
                table: "MobilePaymentInvoice");

            migrationBuilder.DropColumn(
                name: "RejectedDate",
                schema: "MobileSales",
                table: "MobilePaymentInvoice");

            migrationBuilder.DropColumn(
                name: "RejectedBy",
                schema: "MobileSales",
                table: "MobileOrderHeader");

            migrationBuilder.DropColumn(
                name: "RejectedDate",
                schema: "MobileSales",
                table: "MobileOrderHeader");

            migrationBuilder.DropColumn(
                name: "RejectedBy",
                schema: "MobileSales",
                table: "MobileItemRequestHeader");

            migrationBuilder.DropColumn(
                name: "RejectedDate",
                schema: "MobileSales",
                table: "MobileItemRequestHeader");

            migrationBuilder.DropColumn(
                name: "RejectedBy",
                schema: "MobileSales",
                table: "MobileCostHeader");

            migrationBuilder.DropColumn(
                name: "RejectedDate",
                schema: "MobileSales",
                table: "MobileCostHeader");
        }
    }
}
