using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class CreateProcCBPrint : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			// Create procedure dbo.sp_get_cb_print_data
			var sql = @"CREATE PROCEDURE [dbo].[sp_get_cb_print_data]
	@code varchar(17)
AS
BEGIN

	SELECT cb_h.Code, cb_h.VouCode, cb_h.[Type], cb_h.[Date], cb_h.CurrCode, ABS(cb_h.Amount) AS Amount, cb_h.ChequeNo, cb_h.Notes,
		CONCAT(cb_h.CoaCode, ' - ', c_1.[Name]) AS FullCoaName,
		CASE c_1.CBType
			WHEN 'C' THEN
				CASE cb_h.[Type]
					WHEN 'D' THEN 'BUKTI PENERIMAAN KAS'
					WHEN 'C' THEN 'BUKTI PENGELUARAN KAS'
				END
			WHEN 'B' THEN
				CASE cb_h.[Type]
					WHEN 'D' THEN 'BUKTI PENERIMAAN BANK'
					WHEN 'C' THEN 'BUKTI PENGELUARAN BANK'
				END
		END AS TitleCaption,
		cb_d.TransCode AS DetailTransCode, cb_d.Notes AS DetailNotes,
		CASE cb_h.[Type]
			WHEN 'C' THEN
				CASE cb_d.TypeAmount
					WHEN 'D' THEN cb_d.Amount
					WHEN 'C' THEN -cb_d.Amount
				END
			ELSE
				CASE cb_d.TypeAmount
					WHEN 'D' THEN -cb_d.Amount
					WHEN 'C' THEN cb_d.Amount
				END
		END AS DetailAmount,
		CONCAT(cb_d.CoaCode, ' - ', c_2.[Name]) AS DetailFullCoaName,
		e.Initial AS CreatedInitial
	FROM (
		SELECT *
		FROM Finance.GeneralCashBankHeader
		WHERE Code = @code
		AND Mark <> 'V'
	) cb_h
	LEFT JOIN (
		SELECT *
		FROM Finance.GeneralCashBankDetail
		WHERE Code = @code
	) cb_d
		ON cb_d.Code = cb_h.Code
	LEFT JOIN Accounting.COA c_1
		ON c_1.Code = cb_h.CoaCode
	LEFT JOIN Accounting.COA c_2
		ON c_2.Code = cb_d.CoaCode
	LEFT JOIN General.Employee e
		ON e.Id = cb_h.CreatedBy
	
END";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
			// Drop procedure dbo.sp_get_cb_print_data
			var sql = @"DROP PROCEDURE [dbo].[sp_get_cb_print_data]";
            migrationBuilder.Sql(sql);
        }
    }
}
