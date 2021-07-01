using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP_API.Migrations.TenantMigrations
{
    public partial class CreateTableGeneralCashBank : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Finance");

            migrationBuilder.CreateTable(
                name: "GeneralCashBankHeader",
                schema: "Finance",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    VouCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: true),
                    Type = table.Column<string>(type: "varchar(1)", unicode: false, maxLength: 1, nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    CoaCode = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: false),
                    CurrCode = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ChequeNo = table.Column<string>(type: "varchar(25)", unicode: false, maxLength: 25, nullable: true),
                    ChequeDate = table.Column<DateTime>(type: "date", nullable: true),
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
                    table.PrimaryKey("PK_GeneralCashBankHeader", x => x.Code);
                    table.ForeignKey(
                        name: "FK_GeneralCashBankHeader_Currency_CurrCode",
                        column: x => x.CurrCode,
                        principalSchema: "General",
                        principalTable: "Currency",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "GeneralCashBankDetail",
                schema: "Finance",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    Type = table.Column<string>(type: "varchar(5)", unicode: false, maxLength: 5, nullable: false),
                    TransCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    CoaCode = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: false),
                    CurrCode = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TypeAmount = table.Column<string>(type: "varchar(1)", unicode: false, maxLength: 1, nullable: false),
                    TransAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneralCashBankDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GeneralCashBankDetail_Currency_CurrCode",
                        column: x => x.CurrCode,
                        principalSchema: "General",
                        principalTable: "Currency",
                        principalColumn: "Code");
                    table.ForeignKey(
                        name: "FK_GeneralCashBankDetail_GeneralCashBankHeader_Code",
                        column: x => x.Code,
                        principalSchema: "Finance",
                        principalTable: "GeneralCashBankHeader",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateIndex(
                name: "IX_GeneralCashBankDetail_CoaCode",
                schema: "Finance",
                table: "GeneralCashBankDetail",
                column: "CoaCode");

            migrationBuilder.CreateIndex(
                name: "IX_GeneralCashBankDetail_Code",
                schema: "Finance",
                table: "GeneralCashBankDetail",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_GeneralCashBankDetail_CurrCode",
                schema: "Finance",
                table: "GeneralCashBankDetail",
                column: "CurrCode");

            migrationBuilder.CreateIndex(
                name: "IX_GeneralCashBankDetail_TransCode",
                schema: "Finance",
                table: "GeneralCashBankDetail",
                column: "TransCode");

            migrationBuilder.CreateIndex(
                name: "IX_GeneralCashBankHeader_CoaCode",
                schema: "Finance",
                table: "GeneralCashBankHeader",
                column: "CoaCode");

            migrationBuilder.CreateIndex(
                name: "IX_GeneralCashBankHeader_CurrCode",
                schema: "Finance",
                table: "GeneralCashBankHeader",
                column: "CurrCode");

            // Create view Accounting.vwBeginningBalanceAR
            var sql = @"CREATE VIEW Accounting.vwBeginningBalanceAR
AS
	SELECT ar.*,
		c.[Name] AS CustName,
		cr.Initial AS CreatedInitial,
		up.Initial AS UpdatedInitial
	FROM Accounting.BeginningBalanceAR ar
	LEFT JOIN General.Customer c
		ON ar.CustCode = c.Code
	LEFT JOIN SystemManagement.[User] cr
		ON ar.CreatedBy = cr.Id
	LEFT JOIN SystemManagement.[User] up
		ON ar.UpdatedBy = up.Id";
            migrationBuilder.Sql(sql);

            // Create view Accounting.vwBeginningBalanceAP
            sql = @"CREATE VIEW Accounting.vwBeginningBalanceAP
AS
	SELECT ap.*,
		s.[Name] AS SupName,
		cr.Initial AS CreatedInitial,
		up.Initial AS UpdatedInitial
	FROM Accounting.BeginningBalanceAP ap
	LEFT JOIN General.Supplier s
		ON ap.SupCode = s.Code
	LEFT JOIN SystemManagement.[User] cr
		ON ap.CreatedBy = cr.Id
	LEFT JOIN SystemManagement.[User] up
		ON ap.UpdatedBy = up.Id";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GeneralCashBankDetail",
                schema: "Finance");

            migrationBuilder.DropTable(
                name: "GeneralCashBankHeader",
                schema: "Finance");

            // Drop view Accounting.vwBeginningBalanceAR
            var sql = @"DROP VIEW Accounting.vwBeginningBalanceAR";
            migrationBuilder.Sql(sql);

            // Drop view Accounting.vwBeginningBalanceAP
            sql = @"DROP VIEW Accounting.vwBeginningBalanceAP";
            migrationBuilder.Sql(sql);
        }
    }
}
