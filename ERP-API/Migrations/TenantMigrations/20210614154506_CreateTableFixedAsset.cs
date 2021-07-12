using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Web.API.Migrations.TenantMigrations
{
    public partial class CreateTableFixedAsset : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "AssetManagement");

            migrationBuilder.CreateTable(
                name: "AssetType",
                schema: "AssetManagement",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Initial = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    CoaDeprecExpense = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: false),
                    CoaAccumDeprec = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: false),
                    CoaAsset = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: false),
                    CoaExpense = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FixedAsset",
                schema: "AssetManagement",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    TypeId = table.Column<int>(type: "int", nullable: false),
                    PurchaseDate = table.Column<DateTime>(type: "date", nullable: false),
                    StartDepreciateOn = table.Column<DateTime>(type: "date", nullable: false),
                    PurchaseValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AcquiredValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SalvageValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DepreciationMonth = table.Column<int>(type: "int", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    SupCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    PurchaseOrderNo = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    InvoiceNo = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    PaymentVoucherNo = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    YearWarranty = table.Column<int>(type: "int", nullable: false),
                    CodeWarranty = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    EstimatedLife = table.Column<short>(type: "smallint", nullable: false),
                    DepreciationMethod = table.Column<int>(type: "int", nullable: false),
                    InitDepreciationExpense = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BookValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    CoaExpense = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: true),
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
                    table.PrimaryKey("PK_FixedAsset", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "FixedAssetDepartment",
                schema: "AssetManagement",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    Percentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FixedAssetDepartment", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FixedAssetHistory",
                schema: "AssetManagement",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    JournalCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    FiscalYear = table.Column<string>(type: "varchar(4)", unicode: false, maxLength: 4, nullable: false),
                    NumberOfMonth = table.Column<short>(type: "smallint", nullable: false),
                    Period = table.Column<short>(type: "smallint", nullable: false),
                    DepreciateDate = table.Column<DateTime>(type: "date", nullable: false),
                    DepreciateValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BookValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FixedAssetHistory", x => x.Id);
                });

            // Create view AssetManagement.vwAssetType
            var sql = @"CREATE VIEW [AssetManagement].[vwAssetType]
AS
	SELECT [at].*,
		u.Initial AS UpdatedInitial
	FROM AssetManagement.AssetType [at]
	LEFT JOIN SystemManagement.[User] u
		ON u.Id = [at].UpdatedBy";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssetType",
                schema: "AssetManagement");

            migrationBuilder.DropTable(
                name: "FixedAsset",
                schema: "AssetManagement");

            migrationBuilder.DropTable(
                name: "FixedAssetDepartment",
                schema: "AssetManagement");

            migrationBuilder.DropTable(
                name: "FixedAssetHistory",
                schema: "AssetManagement");

            // Drop view AssetManagement.vwAssetType
            var sql = @"DROP VIEW [AssetManagement].[vwAssetType]";
            migrationBuilder.Sql(sql);
        }
    }
}
