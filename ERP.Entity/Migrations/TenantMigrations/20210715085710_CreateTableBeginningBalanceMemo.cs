using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class CreateTableBeginningBalanceMemo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BeginningBalanceCreditMemo",
                schema: "Accounting",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    Type = table.Column<short>(type: "smallint", nullable: false),
                    CustCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    CurrCode = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    CurrRate = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Used = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BeginningBalanceCreditMemo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BeginningBalanceCreditMemo_Currency_CurrCode",
                        column: x => x.CurrCode,
                        principalSchema: "General",
                        principalTable: "Currency",
                        principalColumn: "Code");
                    table.ForeignKey(
                        name: "FK_BeginningBalanceCreditMemo_Supplier_CustCode",
                        column: x => x.CustCode,
                        principalSchema: "General",
                        principalTable: "Supplier",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "BeginningBalanceDebitMemo",
                schema: "Accounting",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    Type = table.Column<short>(type: "smallint", nullable: false),
                    SupCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    CurrCode = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    CurrRate = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Used = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BeginningBalanceDebitMemo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BeginningBalanceDebitMemo_Currency_CurrCode",
                        column: x => x.CurrCode,
                        principalSchema: "General",
                        principalTable: "Currency",
                        principalColumn: "Code");
                    table.ForeignKey(
                        name: "FK_BeginningBalanceDebitMemo_Customer_SupCode",
                        column: x => x.SupCode,
                        principalSchema: "General",
                        principalTable: "Customer",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BeginningBalanceCreditMemo_Code",
                schema: "Accounting",
                table: "BeginningBalanceCreditMemo",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BeginningBalanceCreditMemo_CurrCode",
                schema: "Accounting",
                table: "BeginningBalanceCreditMemo",
                column: "CurrCode");

            migrationBuilder.CreateIndex(
                name: "IX_BeginningBalanceCreditMemo_CustCode",
                schema: "Accounting",
                table: "BeginningBalanceCreditMemo",
                column: "CustCode");

            migrationBuilder.CreateIndex(
                name: "IX_BeginningBalanceDebitMemo_Code",
                schema: "Accounting",
                table: "BeginningBalanceDebitMemo",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BeginningBalanceDebitMemo_CurrCode",
                schema: "Accounting",
                table: "BeginningBalanceDebitMemo",
                column: "CurrCode");

            migrationBuilder.CreateIndex(
                name: "IX_BeginningBalanceDebitMemo_SupCode",
                schema: "Accounting",
                table: "BeginningBalanceDebitMemo",
                column: "SupCode");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BeginningBalanceCreditMemo",
                schema: "Accounting");

            migrationBuilder.DropTable(
                name: "BeginningBalanceDebitMemo",
                schema: "Accounting");
        }
    }
}
