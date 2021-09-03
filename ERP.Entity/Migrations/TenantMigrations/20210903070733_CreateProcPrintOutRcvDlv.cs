using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class CreateProcPrintOutRcvDlv : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create procedure dbo.sp_get_rcv_print_data
            var sql = @"CREATE PROCEDURE [dbo].[sp_get_rcv_print_data]
	@code varchar(17),
	@displayType varchar(20) = 'HEADER'
AS
BEGIN

	IF @displayType = 'HEADER'
	BEGIN
		SELECT rcv_h.*,
			e_req.Initial AS RequestInitial,
			s.Initial AS SupInitial, s.[Name] AS SupName, s.Address1 AS SupAddress1, s.Phone AS SupPhone,
			e_rcv.Initial AS ReceiveInitial,
			e_cre.Initial AS CreatedInitial
		FROM (
			SELECT *
			FROM Purchasing.PurchaseReceiveHeader
			WHERE Code = @code
			AND Mark <> 'V'
		) rcv_h
		LEFT JOIN Purchasing.PurchaseOrderHeader po_h
			ON po_h.Code = rcv_h.TransCode
			AND rcv_h.SrcTrans = 1
		LEFT JOIN General.Employee e_req
			ON e_req.Id = po_h.RequestBy
		LEFT JOIN General.Supplier s
			ON s.Code = rcv_h.SupCode
		LEFT JOIN General.Employee e_rcv
			ON e_rcv.Id = rcv_h.ReceiveBy
		LEFT JOIN General.Employee e_cre
			ON e_cre.Id = rcv_h.CreatedBy
	END

	ELSE IF @displayType = 'DETAIL'
	BEGIN
		SELECT rcv_d.Id, rcv_d.[LineNo], rcv_d.Qty,
			i.Initial AS ItemInitial, i.[Name] AS ItemName,
			uom_c.UnitToConvert AS ItemUnitName,
			rcv_d.[Type] AS Sort
		FROM (
			SELECT Id, [LineNo], ItemId, Qty, UnitId, [Type]
			FROM Purchasing.PurchaseReceiveDetail
			WHERE Code = @code
		) rcv_d
		LEFT JOIN Inventory.Item i
			ON i.Id = rcv_d.ItemId
		LEFT JOIN Inventory.UoMConversion uom_c
			ON uom_c.Id = rcv_d.UnitId
		
		ORDER BY Sort, Id, [LineNo]
	END

END";
            migrationBuilder.Sql(sql);

			// Create procedure dbo.sp_get_do_print_data
			sql = @"CREATE PROCEDURE [dbo].[sp_get_do_print_data]
	@code varchar(17),
	@displayType varchar(20) = 'HEADER'
AS
BEGIN

	IF @displayType = 'HEADER'
	BEGIN
		SELECT do_h.*,
			e_sls.Initial AS SalesInitial,
			c.Initial AS CustInitial, c.[Name] AS CustName,
			CASE WHEN c.ShippingAddressId IS NULL THEN ca_d.Address1
				ELSE ca_b.Address1 END AS CustAddress1,
			CASE WHEN c.ShippingAddressId IS NULL THEN ca_d.Phone
				ELSE ca_b.Phone END AS CustPhone,
			CASE WHEN c.ShippingAddressId IS NULL THEN ca_d.ContactPerson
				ELSE ca_b.ContactPerson END AS CustContactPerson,
			e_shp.Initial AS ShippedInitial
		FROM (
			SELECT *
			FROM Sales.SalesDeliveryHeader
			WHERE Code = @code
			AND Mark <> 'V'
		) do_h
		LEFT JOIN Sales.SalesOrderHeader so_h
			ON so_h.Code = do_h.TransCode
			AND do_h.SrcTrans = 1
		LEFT JOIN General.Employee e_sls
			ON e_sls.Id = so_h.SalesBy
		LEFT JOIN General.Customer c
			ON c.Code = do_h.CustCode
		LEFT JOIN General.CustomerAddress ca_d
			ON ca_d.Code = c.Code
			AND ca_d.IsDefault = 1
			AND c.BillingAddressId IS NULL
		LEFT JOIN General.CustomerAddress ca_b
			ON ca_b.Code = c.Code
			AND ca_b.Id = c.ShippingAddressId
			AND c.BillingAddressId IS NOT NULL
		LEFT JOIN General.Employee e_shp
			ON e_shp.Id = do_h.ShippedBy
	END

	ELSE IF @displayType = 'DETAIL'
	BEGIN
		SELECT do_d.Id, do_d.[LineNo], do_d.Qty,
			i.Initial AS ItemInitial, i.[Name] AS ItemName,
			uom_c.UnitToConvert AS ItemUnitName,
			1 AS Sort
		FROM (
			SELECT Id, [LineNo], ItemId, Qty, UnitId
			FROM Sales.SalesDeliveryDetail
			WHERE Code = @code
		) do_d
		LEFT JOIN Inventory.Item i
			ON i.Id = do_d.ItemId
		LEFT JOIN Inventory.UoMConversion uom_c
			ON uom_c.Id = do_d.UnitId
		UNION ALL
		SELECT do_d_fg.Id, do_d_fg.[LineNo], do_d_fg.Qty,
			i.Initial AS ItemInitial, i.[Name] AS ItemName,
			uom_c.UnitToConvert AS ItemUnitName,
			2 AS Sort
		FROM (
			SELECT Id, Code, [LineNo], ItemId, UnitId, Qty
			FROM Sales.SalesDeliveryDetailFreeGood
			WHERE Code = @code
		) do_d_fg
		LEFT JOIN Inventory.Item i
			ON i.Id = do_d_fg.ItemId
		LEFT JOIN Inventory.UoMConversion uom_c
			ON uom_c.Id = do_d_fg.UnitId
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
			c.Initial AS SupInitial, c.[Name] AS SupName, c.Address1 AS SupAddress1, c.Phone AS SupPhone,
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
			i.Initial AS ItemInitial, i.[Name] AS ItemName,
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

			// Create procedure dbo.sp_get_si_print_data
			sql = @"CREATE PROCEDURE [dbo].[sp_get_si_print_data]
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
			CASE WHEN c.BillingAddressId IS NULL THEN ca_d.Address1
				ELSE ca_b.Address1 END AS CustAddress1,
			CASE WHEN c.BillingAddressId IS NULL THEN ca_d.Phone
				ELSE ca_b.Phone END AS CustPhone,
			CASE WHEN c.BillingAddressId IS NULL THEN ca_d.ContactPerson
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
		LEFT JOIN General.CustomerAddress ca_d
			ON ca_d.Code = c.Code
			AND ca_d.IsDefault = 1
			AND c.BillingAddressId IS NULL
		LEFT JOIN General.CustomerAddress ca_b
			ON ca_b.Code = c.Code
			AND ca_b.Id = c.BillingAddressId
			AND c.BillingAddressId IS NOT NULL
	END

	ELSE IF @displayType = 'DETAIL'
	BEGIN
		SELECT si_d.Id,si_d.Code,si_d.DOCode,
			dlv_d.Id AS DlvDetailId, dlv_d.[LineNo] AS DlvDetailLineNo,
			dlv_d.Qty, dlv_d.UnitPrice, dlv_d.Disc, dlv_d.TaxAmount, dlv_d.Total,
			i.Initial AS ItemInitial, i.[Name] AS ItemName,
			uom_c.UnitToConvert AS ItemUnitName,
			1 AS Sort
		FROM (
			SELECT Id,Code,DOCode
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
			// Drop procedure dbo.sp_get_rcv_print_data
			var sql = @"DROP PROCEDURE[dbo].[sp_get_rcv_print_data]";
            migrationBuilder.Sql(sql);

			// Drop procedure dbo.sp_get_do_print_data
			sql = @"DROP PROCEDURE[dbo].[sp_get_do_print_data]";
            migrationBuilder.Sql(sql);

			// Drop procedure dbo.sp_get_si_print_data
			sql = @"DROP PROCEDURE[dbo].[sp_get_si_print_data]";
            migrationBuilder.Sql(sql);
		}
    }
}
