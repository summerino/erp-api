using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP_API.Migrations.TenantMigrations
{
    public partial class CreateTableCashBankType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CashBankType",
                schema: "Finance",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(5)", unicode: false, maxLength: 5, nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Seq = table.Column<short>(type: "smallint", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashBankType", x => x.Code);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GeneralCashBankDetail_Type",
                schema: "Finance",
                table: "GeneralCashBankDetail",
                column: "Type");

            migrationBuilder.AddForeignKey(
                name: "FK_GeneralCashBankDetail_CashBankType_Type",
                schema: "Finance",
                table: "GeneralCashBankDetail",
                column: "Type",
                principalSchema: "Finance",
                principalTable: "CashBankType",
                principalColumn: "Code");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GeneralCashBankDetail_CashBankType_Type",
                schema: "Finance",
                table: "GeneralCashBankDetail");

            migrationBuilder.DropTable(
                name: "CashBankType",
                schema: "Finance");

            migrationBuilder.DropIndex(
                name: "IX_GeneralCashBankDetail_Type",
                schema: "Finance",
                table: "GeneralCashBankDetail");
        }
    }
}
