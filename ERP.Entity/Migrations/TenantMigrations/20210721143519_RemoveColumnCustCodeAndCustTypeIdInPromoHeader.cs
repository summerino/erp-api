using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class RemoveColumnCustCodeAndCustTypeIdInPromoHeader : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PromoHeader_Customer_CustCode",
                schema: "Sales",
                table: "PromoHeader");

            migrationBuilder.DropForeignKey(
                name: "FK_PromoHeader_CustomerType_CustTypeId",
                schema: "Sales",
                table: "PromoHeader");

            migrationBuilder.DropIndex(
                name: "IX_PromoHeader_CustCode",
                schema: "Sales",
                table: "PromoHeader");

            migrationBuilder.DropIndex(
                name: "IX_PromoHeader_CustTypeId",
                schema: "Sales",
                table: "PromoHeader");

            migrationBuilder.DropColumn(
                name: "CustCode",
                schema: "Sales",
                table: "PromoHeader");

            migrationBuilder.DropColumn(
                name: "CustTypeId",
                schema: "Sales",
                table: "PromoHeader");

            // Refresh view Sales.vwPromoHeader
            var sql = @"execute sp_refreshview 'Sales.vwPromoHeader'";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustCode",
                schema: "Sales",
                table: "PromoHeader",
                type: "varchar(8)",
                unicode: false,
                maxLength: 8,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CustTypeId",
                schema: "Sales",
                table: "PromoHeader",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PromoHeader_CustCode",
                schema: "Sales",
                table: "PromoHeader",
                column: "CustCode");

            migrationBuilder.CreateIndex(
                name: "IX_PromoHeader_CustTypeId",
                schema: "Sales",
                table: "PromoHeader",
                column: "CustTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_PromoHeader_Customer_CustCode",
                schema: "Sales",
                table: "PromoHeader",
                column: "CustCode",
                principalSchema: "General",
                principalTable: "Customer",
                principalColumn: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_PromoHeader_CustomerType_CustTypeId",
                schema: "Sales",
                table: "PromoHeader",
                column: "CustTypeId",
                principalSchema: "General",
                principalTable: "CustomerType",
                principalColumn: "Id");
        }
    }
}
