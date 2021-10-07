using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class CreateProcExpInvPrint : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create procedure dbo.sp_get_ei_print_data
            var sql = @"CREATE PROCEDURE [dbo].[sp_get_ei_print_data]
	@code varchar(17),
	@displayType varchar(20) = 'HEADER',
	@centToWord bit = 0
AS
BEGIN

	IF @displayType = 'HEADER'
	BEGIN
		SELECT ei_h.*,
			dbo.udf_num_to_words_id(ei_h.Amount, @centToWord) AS AmountInWord,
			s.Initial AS SupInitial, s.[Name] AS SupName, s.Address1 AS SupAddress1, s.Phone AS SupPhone,
			e_c.Initial AS CreatedInitial
		FROM (
			SELECT *
			FROM Expedition.ExpeditionInvoiceHeader
			WHERE Code = @code
			AND Mark <> 'V'
		) ei_h
		LEFT JOIN General.Supplier s
			ON s.Code = ei_h.SupCode
		LEFT JOIN General.Employee e_c
			ON e_c.Id = ei_h.CreatedBy
	END

	ELSE IF @displayType = 'DETAIL'
	BEGIN
		SELECT ei_d.Code, ei_d.TransCode,
			CASE ei_h.SrcTrans
				WHEN 1 THEN rcv_h.[Date]
				WHEN 2 THEN do_h.[Date] END AS TransDate
		FROM (
			SELECT Code, TransCode
			FROM Expedition.ExpeditionInvoiceDetail
			WHERE Code = @code
		) ei_d
		LEFT JOIN Expedition.ExpeditionInvoiceHeader ei_h
			ON ei_h.Code = ei_d.Code
		LEFT JOIN Purchasing.PurchaseReceiveHeader rcv_h
			ON rcv_h.Code = ei_d.TransCode
			AND ei_h.SrcTrans = 1
		LEFT JOIN Sales.SalesDeliveryHeader do_h
			ON do_h.Code = ei_d.TransCode
			AND ei_h.SrcTrans = 2
		ORDER BY ei_d.Code, ei_d.TransCode
	END

END";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop procedure dbo.sp_get_ei_print_data
            var sql = @"DROP PROCEDURE [dbo].[sp_get_ei_print_data]";
            migrationBuilder.Sql(sql);
        }
    }
}
