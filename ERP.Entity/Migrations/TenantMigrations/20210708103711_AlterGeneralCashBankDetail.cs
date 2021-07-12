using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AlterGeneralCashBankDetail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TransCode",
                schema: "Finance",
                table: "GeneralCashBankDetail",
                type: "varchar(17)",
                unicode: false,
                maxLength: 17,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(17)",
                oldUnicode: false,
                oldMaxLength: 17);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TransCode",
                schema: "Finance",
                table: "GeneralCashBankDetail",
                type: "varchar(17)",
                unicode: false,
                maxLength: 17,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(17)",
                oldUnicode: false,
                oldMaxLength: 17,
                oldNullable: true);
        }
    }
}
