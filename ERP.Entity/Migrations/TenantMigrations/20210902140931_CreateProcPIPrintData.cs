using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class CreateProcPIPrintData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create procedure dbo.sp_get_pi_print_data
            var sql = @"CREATE PROCEDURE [dbo].[sp_get_pi_print_data]
	@code varchar(17),
	@displayType varchar(20) = 'HEADER',
	@centToWord bit = 0
AS
BEGIN

	IF @displayType = 'HEADER'
	BEGIN
		SELECT pi_h.*,
			dbo.udf_num_to_words_id(pi_h.Total, @centToWord) AS TotalInWord,
			e_r.Initial AS RequestInitial,
			c.Initial AS SupInitial, c.[Name] AS SupName, c.Address1 AS SupAddress1, c.Phone AS SupPhone,
			e_i.Initial AS IssuedInitial
		FROM (
			SELECT *
			FROM Purchasing.PurchaseInvoiceHeader
			WHERE Code = @code
		) pi_h
		LEFT JOIN Purchasing.PurchaseOrderHeader po_h
			ON po_h.Code = pi_h.POCode
		LEFT JOIN General.Employee e_r
			ON e_r.Id = po_h.RequestBy
		LEFT JOIN General.Supplier c
			ON c.Code = pi_h.SupCode
		LEFT JOIN General.Employee e_i
			ON e_i.Id = pi_h.IssuedBy
	END

	ELSE IF @displayType = 'DETAIL'
	BEGIN
		SELECT pi_d.Id,pi_d.Code,pi_d.RcvCode,
			rcv_d.Id AS RcvDetailId, rcv_d.[LineNo] AS RcvDetailLineNo,
			rcv_d.Qty, rcv_d.UnitPrice, rcv_d.Disc,
			CASE WHEN rcv_d.[Type] = 0 THEN rcv_d.TaxAmount
				ELSE 0 END AS TaxAmount,
			CASE WHEN rcv_d.[Type] = 0 THEN rcv_d.Total
				ELSE 0 END AS Total,
			i.Initial AS ItemInitial,
			i.[Name] AS ItemName,
			uom_c.UnitToConvert AS ItemUnitName,
			rcv_d.[Type] AS Sort
		FROM (
			SELECT Id,Code,RcvCode
			FROM Purchasing.PurchaseInvoiceDetail
			WHERE Code = @code
		) pi_d
		LEFT JOIN Purchasing.PurchaseReceiveHeader rcv_h
			ON rcv_h.Code = pi_d.RcvCode
		LEFT JOIN Purchasing.PurchaseReceiveDetail rcv_d
			ON rcv_d.Code = rcv_h.Code
		LEFT JOIN Inventory.Item i
			ON i.Id = rcv_d.ItemId
		LEFT JOIN Inventory.UoMConversion uom_c
			ON uom_c.Id = rcv_d.UnitId
		ORDER BY Sort, Id, RcvCode, RcvDetailId, RcvDetailLineNo
	END

END";
            migrationBuilder.Sql(sql);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop procedure dbo.sp_get_pi_print_data
            var sql = @"DROP PROCEDURE[dbo].[sp_get_pi_print_data]";
            migrationBuilder.Sql(sql);
		}
	}
}
