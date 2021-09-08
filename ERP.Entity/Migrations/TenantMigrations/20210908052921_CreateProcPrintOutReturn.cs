using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class CreateProcPrintOutReturn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			// Create procedure dbo.sp_get_sr_print_data
			var sql = @"CREATE PROCEDURE [dbo].[sp_get_sr_print_data]
	@code varchar(17),
	@displayType varchar(20) = 'HEADER',
	@centToWord bit = 0
AS
BEGIN

	IF @displayType = 'HEADER'
	BEGIN
		SELECT sr_h.*,
			dbo.udf_num_to_words_id(sr_h.Total, @centToWord) AS TotalInWord,
			e_sls.Initial AS SalesInitial,
			c.Initial AS CustInitial, c.[Name] AS CustName,
			CASE WHEN c.BillingAddressId IS NULL THEN ca_d.Address1
				ELSE ca_b.Address1 END AS CustAddress1,
			CASE WHEN c.BillingAddressId IS NULL THEN ca_d.Phone
				ELSE ca_b.Phone END AS CustPhone,
			CASE WHEN c.BillingAddressId IS NULL THEN ca_d.ContactPerson
				ELSE ca_b.ContactPerson END AS CustContactPerson,
			w.Initial AS WarehouseInitial,
			e_cre.Initial AS CreatedInitial
		FROM (
			SELECT *
			FROM Sales.SalesReturnHeader
			WHERE Code = @code
			AND Mark <> 'V'
		) sr_h
		LEFT JOIN General.Employee e_sls
			ON e_sls.Id = sr_h.SalesBy
		LEFT JOIN General.Customer c
			ON c.Code = sr_h.CustCode
		LEFT JOIN General.CustomerAddress ca_b
			ON ca_b.Code = sr_h.CustCode
			AND ca_b.Id = c.BillingAddressId
			AND c.BillingAddressId IS NOT NULL
		LEFT JOIN General.CustomerAddress ca_d
			ON ca_d.Code = sr_h.CustCode
			AND ca_d.IsDefault = 1
			AND c.BillingAddressId IS NULL
		LEFT JOIN Inventory.Warehouse w
			ON w.Code = sr_h.WarehouseCode
		LEFT JOIN General.Employee e_cre
			ON e_cre.Id = sr_h.CreatedBy
	END

	ELSE IF @displayType = 'DETAIL'
	BEGIN
		SELECT sr_d.Id, sr_d.[LineNo],
			sr_d.Qty, sr_d.UnitPrice, sr_d.TaxAmount, sr_d.Total,
			i.Initial AS ItemInitial, i.[Name] AS ItemName,
			uom_c.UnitToConvert AS ItemUnitName,
			1 AS Sort
		FROM (
			SELECT Id, Code, [LineNo], ItemId, UnitId, Qty, UnitPrice, TaxAmount, Total
			FROM Sales.SalesReturnDetail
			WHERE Code = @code
		) sr_d
		LEFT JOIN Inventory.Item i
			ON i.Id = sr_d.ItemId
		LEFT JOIN Inventory.UoMConversion uom_c
			ON uom_c.Id = sr_d.UnitId
		UNION ALL
		SELECT sr_d.Id, sr_d.[LineNo],
			sr_d.Qty, sr_d.UnitPrice, sr_d.TaxAmount, sr_d.Total,
			i.Initial AS ItemInitial, i.[Name] AS ItemName,
			uom_c.UnitToConvert AS ItemUnitName,
			2 AS Sort
		FROM (
			SELECT Id, Code, [LineNo], ItemId, UnitId, Qty, UnitPrice, TaxAmount, Total
			FROM Sales.SalesReturnDetailExchDiffItem
			WHERE Code = @code
		) sr_d
		LEFT JOIN Inventory.Item i
			ON i.Id = sr_d.ItemId
		LEFT JOIN Inventory.UoMConversion uom_c
			ON uom_c.Id = sr_d.UnitId
		ORDER BY Sort, Id, [LineNo]
	END

	ELSE IF @displayType = 'SUM-DETAIL'
	BEGIN
		SELECT SUM(Total) AS Total
		FROM Sales.SalesReturnDetail
		WHERE Code = @code
	END

	ELSE IF @displayType = 'SUM-EXCH'
	BEGIN
		SELECT SUM(Total) AS Total
		FROM Sales.SalesReturnDetailExchDiffItem
		WHERE Code = @code
	END

END";
            migrationBuilder.Sql(sql);

			// Create procedure dbo.sp_get_pr_print_data
			sql = @"CREATE PROCEDURE [dbo].[sp_get_pr_print_data]
	@code varchar(17),
	@displayType varchar(20) = 'HEADER',
	@centToWord bit = 0
AS
BEGIN

	IF @displayType = 'HEADER'
	BEGIN
		SELECT pr_h.*,
			dbo.udf_num_to_words_id(pr_h.Total, @centToWord) AS TotalInWord,
			e_shp.Initial AS ShippedInitial,
			s.Initial AS SupInitial, s.[Name] AS SupName, s.Address1 AS SupAddress1, s.Phone AS SupPhone,
			e_cre.Initial AS CreatedInitial
		FROM (
			SELECT *
			FROM Purchasing.PurchaseReturnHeader
			WHERE Code = @code
			AND Mark <> 'V'
		) pr_h
		LEFT JOIN General.Employee e_shp
			ON e_shp.Id = pr_h.ShippedBy
		LEFT JOIN General.Supplier s
			ON s.Code = pr_h.SupCode
		LEFT JOIN General.Employee e_cre
			ON e_cre.Id = pr_h.CreatedBy
	END

	ELSE IF @displayType = 'DETAIL'
	BEGIN
		SELECT pr_d.Id, pr_d.[LineNo],
			pr_d.Qty, pr_d.UnitPrice, pr_d.TaxAmount, pr_d.Total,
			i.Initial AS ItemInitial, i.[Name] AS ItemName,
			uom_c.UnitToConvert AS ItemUnitName,
			1 AS Sort
		FROM (
			SELECT Id, Code, [LineNo], ItemId, UnitId, Qty, UnitPrice, TaxAmount, Total
			FROM Purchasing.PurchaseReturnDetail
			WHERE Code = @code
		) pr_d
		LEFT JOIN Inventory.Item i
			ON i.Id = pr_d.ItemId
		LEFT JOIN Inventory.UoMConversion uom_c
			ON uom_c.Id = pr_d.UnitId
		UNION ALL
		SELECT pr_d.Id, pr_d.[LineNo],
			pr_d.Qty, pr_d.UnitPrice, pr_d.TaxAmount, pr_d.Total,
			i.Initial AS ItemInitial, i.[Name] AS ItemName,
			uom_c.UnitToConvert AS ItemUnitName,
			2 AS Sort
		FROM (
			SELECT Id, Code, [LineNo], ItemId, UnitId, Qty, UnitPrice, TaxAmount, Total
			FROM Purchasing.PurchaseReturnDetailExchDiffItem
			WHERE Code = @code
		) pr_d
		LEFT JOIN Inventory.Item i
			ON i.Id = pr_d.ItemId
		LEFT JOIN Inventory.UoMConversion uom_c
			ON uom_c.Id = pr_d.UnitId
		ORDER BY Sort, Id, [LineNo]
	END

	ELSE IF @displayType = 'SUM-DETAIL'
	BEGIN
		SELECT SUM(Total) AS Total
		FROM Purchasing.PurchaseReturnDetail
		WHERE Code = @code
	END

	ELSE IF @displayType = 'SUM-EXCH'
	BEGIN
		SELECT SUM(Total) AS Total
		FROM Purchasing.PurchaseReturnDetailExchDiffItem
		WHERE Code = @code
	END

END";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
			// Drop procedure dbo.sp_get_sr_print_data
			var sql = @"DROP PROCEDURE[dbo].[sp_get_sr_print_data]";
            migrationBuilder.Sql(sql);

			// Drop procedure dbo.sp_get_pr_print_data
			sql = @"DROP PROCEDURE[dbo].[sp_get_pr_print_data]";
            migrationBuilder.Sql(sql);
        }
    }
}
