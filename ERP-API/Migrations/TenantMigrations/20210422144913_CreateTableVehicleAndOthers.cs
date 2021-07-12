using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Web.API.Migrations.TenantMigrations
{
    public partial class CreateTableVehicleAndOthers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CurrencyRate",
                schema: "Accounting",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    CurrCode = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrencyRate", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vehicle",
                schema: "General",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VehicleNo = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    TypeId = table.Column<int>(type: "int", nullable: false),
                    MaxLoadVolume = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxLoadWeight = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DriverId = table.Column<long>(type: "bigint", nullable: false),
                    HelperId1 = table.Column<long>(type: "bigint", nullable: true),
                    HelperId2 = table.Column<long>(type: "bigint", nullable: true),
                    Notes = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicle", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VehicleType",
                schema: "General",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Initial = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleType", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyRate_Date_CurrCode",
                schema: "Accounting",
                table: "CurrencyRate",
                columns: new[] { "Date", "CurrCode" },
                unique: true);

            // Create view General.vwTax
            var sql = @"CREATE VIEW [General].[vwTax]
AS
    SELECT c.*,
        x.[Name] AS CoaName
    FROM General.Tax c
    LEFT JOIN Accounting.COA x
        ON x.Code = c.CoaCode";
            migrationBuilder.Sql(sql);

            // Create store procedure dbo.sp_insert_curr_rate
            sql = @"CREATE PROCEDURE [dbo].[sp_insert_curr_rate]
	@dateFrom date,
	@dateTo date,
	@currCode varchar(3),
	@amount decimal(19, 6),
	@createdBy int
AS
BEGIN
	
	BEGIN TRY
		BEGIN TRANSACTION

			;WITH cte_rate_src AS (
				SELECT DATEADD(DAY, nbr - 1, @dateFrom) AS [date], @currCode AS currCode, @amount AS amount
				FROM (
					SELECT ROW_NUMBER() OVER ( ORDER BY c.object_id ) AS nbr
					FROM sys.columns c
				) nbrs
				WHERE nbr - 1 <= DATEDIFF(DAY, @dateFrom, @dateTo)
			)
	
			MERGE Accounting.CurrencyRate AS T
			USING cte_rate_src AS S
			ON (T.[Date] = S.[date] AND T.CurrCode = S.currCode)
			WHEN MATCHED THEN
				UPDATE SET T.Amount = S.amount,
						   T.UpdatedBy = @createdBy,
						   T.UpdatedDate = GETDATE()
			WHEN NOT MATCHED BY TARGET THEN
				INSERT ([Date], CurrCode, Amount, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate)
				VALUES (S.[date], S.currCode, S.amount, @createdBy, GETDATE(), @createdBy, GETDATE());

		COMMIT TRANSACTION
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;

		EXEC dbo.sp_raiseerror
	END CATCH

END";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CurrencyRate",
                schema: "Accounting");

            migrationBuilder.DropTable(
                name: "Vehicle",
                schema: "General");

            migrationBuilder.DropTable(
                name: "VehicleType",
                schema: "General");

            // Drop view General.vwTax
            var sql = @"DROP VIEW [General].[vwTax]";
            migrationBuilder.Sql(sql);

            // Drop store procedure dbo.sp_insert_curr_rate
            sql = @"DROP PROCEDURE [dbo].[sp_insert_curr_rate]";
            migrationBuilder.Sql(sql);
        }
    }
}
