using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class FixingMobileCostFK : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MobileCostDetail_MobileItemRequestHeader_Code",
                schema: "MobileSales",
                table: "MobileCostDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_MobileCostImage_MobileItemRequestHeader_Code",
                schema: "MobileSales",
                table: "MobileCostImage");

            migrationBuilder.AddForeignKey(
                name: "FK_MobileCostDetail_MobileCostHeader_Code",
                schema: "MobileSales",
                table: "MobileCostDetail",
                column: "Code",
                principalSchema: "MobileSales",
                principalTable: "MobileCostHeader",
                principalColumn: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_MobileCostImage_MobileCostHeader_Code",
                schema: "MobileSales",
                table: "MobileCostImage",
                column: "Code",
                principalSchema: "MobileSales",
                principalTable: "MobileCostHeader",
                principalColumn: "Code");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MobileCostDetail_MobileCostHeader_Code",
                schema: "MobileSales",
                table: "MobileCostDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_MobileCostImage_MobileCostHeader_Code",
                schema: "MobileSales",
                table: "MobileCostImage");

            migrationBuilder.AddForeignKey(
                name: "FK_MobileCostDetail_MobileItemRequestHeader_Code",
                schema: "MobileSales",
                table: "MobileCostDetail",
                column: "Code",
                principalSchema: "MobileSales",
                principalTable: "MobileItemRequestHeader",
                principalColumn: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_MobileCostImage_MobileItemRequestHeader_Code",
                schema: "MobileSales",
                table: "MobileCostImage",
                column: "Code",
                principalSchema: "MobileSales",
                principalTable: "MobileItemRequestHeader",
                principalColumn: "Code");
        }
    }
}
