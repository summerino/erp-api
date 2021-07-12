using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Web.API.Migrations.TenantMigrations
{
    public partial class CreateTableExpeditionInvoice : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Expedition");

            migrationBuilder.CreateTable(
                name: "ExpeditionInvoiceHeader",
                schema: "Expedition",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    DueDate = table.Column<DateTime>(type: "date", nullable: false),
                    SrcTrans = table.Column<short>(type: "smallint", nullable: false),
                    RefNo = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    SupCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    CurrCode = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    Mark = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ApprovedBy = table.Column<int>(type: "int", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpeditionInvoiceHeader", x => x.Code);
                    table.ForeignKey(
                        name: "FK_ExpeditionInvoiceHeader_Supplier_SupCode",
                        column: x => x.SupCode,
                        principalSchema: "General",
                        principalTable: "Supplier",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "ExpeditionInvoiceDetail",
                schema: "Expedition",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    TransCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpeditionInvoiceDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExpeditionInvoiceDetail_ExpeditionInvoiceHeader_Code",
                        column: x => x.Code,
                        principalSchema: "Expedition",
                        principalTable: "ExpeditionInvoiceHeader",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateIndex(
                name: "IX_FixedAssetHistory_Code",
                schema: "AssetManagement",
                table: "FixedAssetHistory",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_FixedAssetDepartment_Code",
                schema: "AssetManagement",
                table: "FixedAssetDepartment",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_FixedAsset_SupCode",
                schema: "AssetManagement",
                table: "FixedAsset",
                column: "SupCode");

            migrationBuilder.CreateIndex(
                name: "IX_FixedAsset_TypeId",
                schema: "AssetManagement",
                table: "FixedAsset",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpeditionInvoiceDetail_Code",
                schema: "Expedition",
                table: "ExpeditionInvoiceDetail",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_ExpeditionInvoiceHeader_SupCode",
                schema: "Expedition",
                table: "ExpeditionInvoiceHeader",
                column: "SupCode");

            migrationBuilder.AddForeignKey(
                name: "FK_FixedAsset_AssetType_TypeId",
                schema: "AssetManagement",
                table: "FixedAsset",
                column: "TypeId",
                principalSchema: "AssetManagement",
                principalTable: "AssetType",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FixedAsset_Supplier_SupCode",
                schema: "AssetManagement",
                table: "FixedAsset",
                column: "SupCode",
                principalSchema: "General",
                principalTable: "Supplier",
                principalColumn: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_FixedAssetDepartment_FixedAsset_Code",
                schema: "AssetManagement",
                table: "FixedAssetDepartment",
                column: "Code",
                principalSchema: "AssetManagement",
                principalTable: "FixedAsset",
                principalColumn: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_FixedAssetHistory_FixedAsset_Code",
                schema: "AssetManagement",
                table: "FixedAssetHistory",
                column: "Code",
                principalSchema: "AssetManagement",
                principalTable: "FixedAsset",
                principalColumn: "Code");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FixedAsset_AssetType_TypeId",
                schema: "AssetManagement",
                table: "FixedAsset");

            migrationBuilder.DropForeignKey(
                name: "FK_FixedAsset_Supplier_SupCode",
                schema: "AssetManagement",
                table: "FixedAsset");

            migrationBuilder.DropForeignKey(
                name: "FK_FixedAssetDepartment_FixedAsset_Code",
                schema: "AssetManagement",
                table: "FixedAssetDepartment");

            migrationBuilder.DropForeignKey(
                name: "FK_FixedAssetHistory_FixedAsset_Code",
                schema: "AssetManagement",
                table: "FixedAssetHistory");

            migrationBuilder.DropTable(
                name: "ExpeditionInvoiceDetail",
                schema: "Expedition");

            migrationBuilder.DropTable(
                name: "ExpeditionInvoiceHeader",
                schema: "Expedition");

            migrationBuilder.DropIndex(
                name: "IX_FixedAssetHistory_Code",
                schema: "AssetManagement",
                table: "FixedAssetHistory");

            migrationBuilder.DropIndex(
                name: "IX_FixedAssetDepartment_Code",
                schema: "AssetManagement",
                table: "FixedAssetDepartment");

            migrationBuilder.DropIndex(
                name: "IX_FixedAsset_SupCode",
                schema: "AssetManagement",
                table: "FixedAsset");

            migrationBuilder.DropIndex(
                name: "IX_FixedAsset_TypeId",
                schema: "AssetManagement",
                table: "FixedAsset");
        }
    }
}
