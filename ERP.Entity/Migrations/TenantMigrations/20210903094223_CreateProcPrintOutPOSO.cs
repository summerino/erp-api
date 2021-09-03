using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class CreateProcPrintOutPOSO : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create procedure dbo.sp_get_so_print_data
            var sql = @"CREATE PROCEDURE [dbo].[sp_get_so_print_data]
	@code varchar(17),
	@displayType varchar(20) = 'HEADER',
	@centToWord bit = 0
AS
BEGIN

	IF @displayType = 'HEADER'
	BEGIN
		SELECT so_h.*,
			dbo.udf_num_to_words_id(so_h.Total, @centToWord) AS TotalInWord,
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
			FROM Sales.SalesOrderHeader 
			WHERE Code = @code
			AND Mark <> 'V'
		) so_h
		LEFT JOIN General.Employee e
			ON e.Id = so_h.SalesBy
		LEFT JOIN General.Customer c
			ON c.Code = so_h.CustCode
		LEFT JOIN General.CustomerAddress ca_b
			ON ca_b.Code = so_h.CustCode
			AND ca_b.Id = so_h.BillingAddressId
			AND so_h.BillingAddressId IS NOT NULL
		LEFT JOIN General.CustomerAddress ca_d
			ON ca_d.Code = so_h.CustCode
			AND ca_d.IsDefault = 1
			AND so_h.BillingAddressId IS NULL
	END

	ELSE IF @displayType = 'DETAIL'
	BEGIN
		SELECT si_d.Id, si_d.[LineNo],
			si_d.Qty, si_d.UnitPrice, si_d.Disc, si_d.TaxAmount, si_d.Total,
			i.Initial AS ItemInitial, i.[Name] AS ItemName,
			uom_c.UnitToConvert AS ItemUnitName,
			1 AS Sort
		FROM (
			SELECT Id, Code, [LineNo], ItemId, UnitId, Qty, UnitPrice, Disc, TaxAmount, Total
			FROM Sales.SalesOrderDetail
			WHERE Code = @code
		) si_d
		LEFT JOIN Inventory.Item i
			ON i.Id = si_d.ItemId
		LEFT JOIN Inventory.UoMConversion uom_c
			ON uom_c.Id = si_d.UnitId
		UNION ALL
		SELECT so_d_fg.Id, so_d_fg.[LineNo],
			so_d_fg.Qty, so_d_fg.UnitPrice, so_d_fg.UnitPrice, 0, 0,
			i.Initial AS ItemInitial, i.[Name] AS ItemName,
			uom_c.UnitToConvert AS ItemUnitName,
			2 AS Sort
		FROM (
			SELECT *
			FROM Sales.SalesOrderDetailFreeGood
			WHERE Code = @code
		) so_d_fg
		LEFT JOIN Inventory.Item i
			ON i.Id = so_d_fg.ItemId
		LEFT JOIN Inventory.UoMConversion uom_c
			ON uom_c.Id = so_d_fg.UnitId
		ORDER BY Sort, Id, [LineNo]
	END

END";
            migrationBuilder.Sql(sql);

			// Create procedure dbo.sp_get_po_print_data
			sql = @"CREATE PROCEDURE [dbo].[sp_get_po_print_data]
	@code varchar(17),
	@displayType varchar(20) = 'HEADER',
	@centToWord bit = 0
AS
BEGIN

	IF @displayType = 'HEADER'
	BEGIN
		SELECT po_h.*,
			dbo.udf_num_to_words_id(po_h.Total, @centToWord) AS TotalInWord,
			e_r.Initial AS RequestInitial,
			s.Initial AS SupInitial, s.[Name] AS SupName, s.Address1 AS SupAddress1, s.Phone AS SupPhone,
			e_c.Initial AS CreatedInitial
		FROM (
			SELECT *
			FROM Purchasing.PurchaseOrderHeader
			WHERE Code = @code
			AND Mark <> 'V'
		) po_h
		LEFT JOIN General.Employee e_r
			ON e_r.Id = po_h.RequestBy
		LEFT JOIN General.Supplier s
			ON s.Code = po_h.SupCode
		LEFT JOIN General.Employee e_c
			ON e_c.Id = po_h.CreatedBy
	END

	ELSE IF @displayType = 'DETAIL'
	BEGIN
		SELECT po_d.Id, po_d.[LineNo],
			po_d.Qty, po_d.UnitPrice, po_d.Disc,
			CASE WHEN po_d.[Type] = 0 THEN po_d.TaxAmount
				ELSE 0 END AS TaxAmount,
			CASE WHEN po_d.[Type] = 0 THEN po_d.Total
				ELSE 0 END AS Total,
			i.Initial AS ItemInitial, i.[Name] AS ItemName,
			uom_c.UnitToConvert AS ItemUnitName,
			po_d.[Type] AS Sort
		FROM (
			SELECT Id, Code, [LineNo], ItemId, UnitId, Qty, UnitPrice, Disc, TaxAmount, Total, [Type]
			FROM Purchasing.PurchaseOrderDetail
			WHERE Code = @code
		) po_d
		LEFT JOIN Inventory.Item i
			ON i.Id = po_d.ItemId
		LEFT JOIN Inventory.UoMConversion uom_c
			ON uom_c.Id = po_d.UnitId
		ORDER BY Sort, Id, [LineNo]
	END

END";
            migrationBuilder.Sql(sql);

            // Alter procedure dbo.sp_get_pi_print_data
            sql = @"ALTER PROCEDURE [dbo].[sp_get_pi_print_data]
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
			s.Initial AS SupInitial, s.[Name] AS SupName, s.Address1 AS SupAddress1, s.Phone AS SupPhone,
			e_i.Initial AS IssuedInitial
		FROM (
			SELECT *
			FROM Purchasing.PurchaseInvoiceHeader
			WHERE Code = @code
			AND Mark <> 'V'
		) pi_h
		LEFT JOIN Purchasing.PurchaseOrderHeader po_h
			ON po_h.Code = pi_h.POCode
		LEFT JOIN General.Employee e_r
			ON e_r.Id = po_h.RequestBy
		LEFT JOIN General.Supplier s
			ON s.Code = pi_h.SupCode
		LEFT JOIN General.Employee e_i
			ON e_i.Id = pi_h.IssuedBy
	END

	ELSE IF @displayType = 'DETAIL'
	BEGIN
		SELECT pi_d.Id, pi_d.Code, pi_d.RcvCode,
			rcv_d.Id AS RcvDetailId, rcv_d.[LineNo] AS RcvDetailLineNo,
			rcv_d.Qty, rcv_d.UnitPrice, rcv_d.Disc,
			CASE WHEN rcv_d.[Type] = 0 THEN rcv_d.TaxAmount
				ELSE 0 END AS TaxAmount,
			CASE WHEN rcv_d.[Type] = 0 THEN rcv_d.Total
				ELSE 0 END AS Total,
			i.Initial AS ItemInitial, i.[Name] AS ItemName,
			uom_c.UnitToConvert AS ItemUnitName,
			rcv_d.[Type] AS Sort
		FROM (
			SELECT Id, Code, RcvCode
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

			// Alter procedure dbo.sp_get_si_print_data
			sql = @"ALTER PROCEDURE [dbo].[sp_get_si_print_data]
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
			// Drop procedure dbo.sp_get_so_print_data
			var sql = @"DROP PROCEDURE[dbo].[sp_get_so_print_data]";
            migrationBuilder.Sql(sql);

            // Drop procedure dbo.sp_get_po_print_data
            sql = @"DROP PROCEDURE[dbo].[sp_get_po_print_data]";
            migrationBuilder.Sql(sql);
        }
    }
}
