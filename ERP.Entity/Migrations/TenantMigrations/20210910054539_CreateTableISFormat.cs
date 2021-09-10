using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class CreateTableISFormat : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IncomeStatementFormat",
                schema: "Accounting",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(5)", unicode: false, maxLength: 5, nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    ParentCode = table.Column<string>(type: "varchar(5)", unicode: false, maxLength: 5, nullable: true),
                    Category = table.Column<string>(type: "varchar(1)", unicode: false, maxLength: 1, nullable: false),
                    Type = table.Column<string>(type: "varchar(1)", unicode: false, maxLength: 1, nullable: false),
                    PercentOf = table.Column<string>(type: "varchar(5)", unicode: false, maxLength: 5, nullable: false),
                    PercentFrom = table.Column<string>(type: "varchar(5)", unicode: false, maxLength: 5, nullable: true),
                    Position = table.Column<string>(type: "varchar(1)", unicode: false, maxLength: 1, nullable: false),
                    Deep = table.Column<int>(type: "int", nullable: false),
                    Sort = table.Column<int>(type: "int", nullable: false),
                    SubtotalSort = table.Column<int>(type: "int", nullable: false),
                    Detail = table.Column<bool>(type: "bit", nullable: false),
                    Hidden = table.Column<bool>(type: "bit", nullable: false),
                    Bold = table.Column<bool>(type: "bit", nullable: false),
                    ByAccount = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncomeStatementFormat", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "IncomeStatementFormatSubtotal",
                schema: "Accounting",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(5)", unicode: false, maxLength: 5, nullable: false),
                    SubCode = table.Column<string>(type: "varchar(5)", unicode: false, maxLength: 5, nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncomeStatementFormatSubtotal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncomeStatementFormatSubtotal_IncomeStatementFormat_Code",
                        column: x => x.Code,
                        principalSchema: "Accounting",
                        principalTable: "IncomeStatementFormat",
                        principalColumn: "Code");
                    table.ForeignKey(
                        name: "FK_IncomeStatementFormatSubtotal_IncomeStatementFormat_SubCode",
                        column: x => x.SubCode,
                        principalSchema: "Accounting",
                        principalTable: "IncomeStatementFormat",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateIndex(
                name: "IX_IncomeStatementFormatSubtotal_Code",
                schema: "Accounting",
                table: "IncomeStatementFormatSubtotal",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_IncomeStatementFormatSubtotal_SubCode",
                schema: "Accounting",
                table: "IncomeStatementFormatSubtotal",
                column: "SubCode");

            // Create view Accounting.vwIncomeStatementFormatSubtotal
            var sql = @"CREATE VIEW [Accounting].[vwIncomeStatementFormatSubtotal]
AS
	SELECT f_st.*
	,f.[Name] AS SubName
	,f.Position AS SubPosition
	FROM Accounting.IncomeStatementFormatSubtotal f_st
	LEFT JOIN Accounting.IncomeStatementFormat f
		ON f.Code = f_st.SubCode";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IncomeStatementFormatSubtotal",
                schema: "Accounting");

            migrationBuilder.DropTable(
                name: "IncomeStatementFormat",
                schema: "Accounting");

            // Drop view Accounting.vwIncomeStatementFormatSubtotal
            var sql = @"DROP VIEW [Accounting].[vwIncomeStatementFormatSubtotal]";
            migrationBuilder.Sql(sql);
        }
    }
}
