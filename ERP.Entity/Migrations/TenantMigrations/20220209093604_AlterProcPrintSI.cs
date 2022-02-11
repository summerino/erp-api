using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AlterProcPrintSI : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			// Alter proc dbo.sp_get_si_print_data
			var sql = @"ALTER PROCEDURE [dbo].[sp_get_si_print_data]
	@code varchar(17),
	@displayType varchar(20) = 'HEADER',
	@centToWord bit = 0
AS
BEGIN

	IF @displayType = 'HEADER'
	BEGIN
		SELECT si_h.*,
			dbo.udf_num_to_words_id(si_h.Total, @centToWord) AS TotalInWord,
			e.Initial AS SalesInitial,
			c.Initial AS CustInitial, c.[Name] AS CustName,
			CASE WHEN so_h.BillingAddressId IS NULL THEN ca_d.Address1
				ELSE ca_b.Address1 END AS CustAddress1,
			CASE WHEN so_h.BillingAddressId IS NULL THEN ca_d.Phone
				ELSE ca_b.Phone END AS CustPhone,
			CASE WHEN so_h.BillingAddressId IS NULL THEN ca_d.ContactPerson
				ELSE ca_b.ContactPerson END AS CustContactPerson
		FROM (
			SELECT *
			FROM Sales.SalesInvoiceHeader 
			WHERE Code = @code
			AND Mark <> 'V'
		) si_h
		LEFT JOIN Sales.SalesOrderHeader so_h
			ON so_h.Code = si_h.SOCode
		LEFT JOIN General.Employee e
			ON e.Id = so_h.SalesBy
		LEFT JOIN General.Customer c
			ON c.Code = si_h.CustCode
		LEFT JOIN General.CustomerAddress ca_b
			ON ca_b.Code = si_h.CustCode
			AND ca_b.Id = so_h.BillingAddressId
			AND so_h.BillingAddressId IS NOT NULL
		LEFT JOIN General.CustomerAddress ca_d
			ON ca_d.Code = si_h.CustCode
			AND ca_d.IsDefault = 1
			AND so_h.BillingAddressId IS NULL
	END

	ELSE IF @displayType = 'DETAIL'
	BEGIN
		SELECT si_d.Id, si_d.Code, si_d.DOCode,
			dlv_d.Id AS DlvDetailId, dlv_d.[LineNo] AS DlvDetailLineNo,
			dlv_d.Qty, dlv_d.UnitPrice, dlv_d.Disc, dlv_d.TaxAmount, dlv_d.Total,
			i.Initial AS ItemInitial, i.[Name] AS ItemName,
			uom_c.UnitToConvert AS ItemUnitName,
			1 AS Sort
		FROM (
			SELECT Id, Code, DOCode
			FROM Sales.SalesInvoiceDetail
			WHERE Code = @code
		) si_d
		LEFT JOIN Sales.SalesDeliveryHeader dlv_h
			ON dlv_h.Code = si_d.DOCode
		LEFT JOIN Sales.SalesDeliveryDetail dlv_d
			ON dlv_d.Code = dlv_h.Code
		LEFT JOIN Inventory.Item i
			ON i.Id = dlv_d.ItemId
		LEFT JOIN Inventory.UoMConversion uom_c
			ON uom_c.Id = dlv_d.UnitId
		UNION ALL
		SELECT 0, '', dlv_d_fg.Code,
			dlv_d_fg.DlvOrderDetailId, dlv_d_fg.[LineNo],
			dlv_d_fg.Qty, dlv_d_fg.UnitPrice, dlv_d_fg.UnitPrice, 0, 0,
			i.Initial AS ItemInitial, i.[Name] AS ItemName,
			uom_c.UnitToConvert AS ItemUnitName,
			2 AS Sort
		FROM (
			SELECT *
			FROM Sales.SalesDeliveryDetailFreeGood
			WHERE EXISTS (
				SELECT Id,Code,DOCode
				FROM Sales.SalesInvoiceDetail
				WHERE Code = @code
				AND DOCode = Code
			)
		) dlv_d_fg
		LEFT JOIN Inventory.Item i
			ON i.Id = dlv_d_fg.ItemId
		LEFT JOIN Inventory.UoMConversion uom_c
			ON uom_c.Id = dlv_d_fg.UnitId
		ORDER BY Sort, Id, DOCode, DlvDetailId, DlvDetailLineNo
	END

END";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
