using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class CreateProcDlvPlanPackingPrint : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create procedure dbo.sp_get_dlv_plan_packing_print_data
            var sql = @"CREATE PROCEDURE [dbo].[sp_get_dlv_plan_packing_print_data]
	@code varchar(17)
AS
BEGIN

	WITH cte_dlv_plan_src AS (
		SELECT dlv_plan_h.Code, dlv_plan_h.[Date], dlv_plan_h.VehicleId, dlv_plan_h.DriverId, dlv_plan_h.WarehouseCode,
			dlv_plan_h.TotalVolume, dlv_plan_h.TotalWeight, dlv_plan_h.Notes, dlv_plan_h.CreatedBy,
			dlv_plan_d.TransCode,
			CASE WHEN ISNULL(si_d.DOCode, '') = '' THEN dlv_plan_d.TransCode
				ELSE si_d.DOCode END AS DOCode
		FROM (
			SELECT *
			FROM Sales.DeliveryPlanHeader
			WHERE Code = @code
			AND Mark <> 'V'
		) dlv_plan_h
		LEFT JOIN Sales.DeliveryPlanDetail dlv_plan_d
			ON dlv_plan_d.Code = dlv_plan_h.Code
		LEFT JOIN Sales.SalesInvoiceHeader si_h
			ON si_h.Code = dlv_plan_d.TransCode
		LEFT JOIN Sales.SalesInvoiceDetail si_d
			ON si_d.Code = si_h.Code
	)
	,cte_normal_src AS (
		SELECT cte.*,
			dlv_d.ItemId, dlv_d.Qty, dlv_d.UnitId
		FROM cte_dlv_plan_src cte
		INNER JOIN Sales.SalesDeliveryDetail dlv_d
			ON dlv_d.Code = cte.DOCode
	)
	,cte_free_src AS (
		SELECT cte.*,
			dlv_d_fg.ItemId, dlv_d_fg.Qty, dlv_d_fg.UnitId
		FROM cte_dlv_plan_src cte
		INNER JOIN Sales.SalesDeliveryDetailFreeGood dlv_d_fg
			ON dlv_d_fg.Code = cte.DOCode
	)
	,cte_union AS (
		SELECT cte_n.*
		FROM cte_normal_src cte_n
		UNION ALL
		SELECT cte_f.*
		FROM cte_free_src cte_f
	)
	,cte_calc_group AS (
		SELECT cte.Code, cte.[Date], cte.VehicleId, cte.DriverId, cte.WarehouseCode, cte.TotalVolume, cte.TotalWeight,
			cte.Notes, cte.CreatedBy, cte.TransCode,
			cte.ItemId, cte.UnitId,
			do_h.CustCode,
			SUM(cte.Qty) AS Qty
		FROM cte_union cte
		LEFT JOIN Sales.SalesDeliveryHeader do_h
			ON do_h.Code = cte.DOCode
		GROUP BY cte.Code, cte.[Date], cte.VehicleId, cte.DriverId, cte.WarehouseCode, cte.TotalVolume, cte.TotalWeight,
			cte.Notes, cte.CreatedBy, cte.TransCode,
			cte.ItemId, cte.UnitId,
			do_h.CustCode
	)
	SELECT cte.*,
		v.VehicleNo,
		e_d.Initial AS DriverInitial, e_d.FirstName AS DriverFirstName, e_d.LastName AS DriverLastName,
		w.Initial AS WarehouseInitial, w.[Name] AS WarehouseName,
		e_c.Initial AS CreatedInitial,
		c.[Name] AS CustName,
		ca_d.Address1 AS CustAddress1, ca_d.ContactPerson AS CustContactPerson, ca_d.Phone AS CustPhone,
		i.Initial AS ItemInitial, i.[Name] AS ItemName,
		uom_c.UnitToConvert AS ItemUnitName
	FROM cte_calc_group cte
	LEFT JOIN General.Vehicle v
		ON v.Id = cte.VehicleId
	LEFT JOIN General.Employee e_d
		ON e_d.Id = cte.DriverId
	LEFT JOIN Inventory.Warehouse w
		ON w.Code = cte.WarehouseCode
	LEFT JOIN General.Employee e_c
		ON e_c.Id = cte.CreatedBy
	LEFT JOIN General.Customer c
		ON c.Code = cte.CustCode
	LEFT JOIN General.CustomerAddress ca_d
		ON ca_d.Code = cte.CustCode
		AND ca_d.IsDefault = 1
	LEFT JOIN Inventory.Item i
		ON i.Id = cte.ItemId
	LEFT JOIN Inventory.UoMConversion uom_c
		ON uom_c.Id = cte.UnitId
	ORDER BY cte.TransCode, i.Initial, i.[Name], uom_c.UnitToConvert

END";
            migrationBuilder.Sql(sql);

            // Alter procedure dbo.sp_get_dlv_plan_picking_print_data
            sql = @"ALTER PROCEDURE [dbo].[sp_get_dlv_plan_picking_print_data]
	@code varchar(17),
	@displayType varchar(20) = 'ITEM'
AS
BEGIN

	IF @displayType = 'ITEM'
	BEGIN
		WITH cte_dlv_plan_src AS (
			SELECT dlv_plan_h.Code, dlv_plan_h.[Date], dlv_plan_h.VehicleId, dlv_plan_h.DriverId, dlv_plan_h.WarehouseCode,
				dlv_plan_h.Notes, dlv_plan_h.CreatedBy,
				dlv_plan_d.TransCode,
				CASE WHEN ISNULL(si_d.DOCode, '') = '' THEN dlv_plan_d.TransCode
					ELSE si_d.DOCode END AS DOCode
			FROM (
				SELECT *
				FROM Sales.DeliveryPlanHeader
				WHERE Code = @code
				AND Mark <> 'V'
			) dlv_plan_h
			LEFT JOIN Sales.DeliveryPlanDetail dlv_plan_d
				ON dlv_plan_d.Code = dlv_plan_h.Code
			LEFT JOIN Sales.SalesInvoiceHeader si_h
				ON si_h.Code = dlv_plan_d.TransCode
			LEFT JOIN Sales.SalesInvoiceDetail si_d
				ON si_d.Code = si_h.Code
		)
		,cte_normal_src AS (
			SELECT cte.*,
				dlv_d.ItemId, dlv_d.Qty, dlv_d.UnitId
			FROM cte_dlv_plan_src cte
			INNER JOIN Sales.SalesDeliveryDetail dlv_d
				ON dlv_d.Code = cte.DOCode
		)
		,cte_free_src AS (
			SELECT cte.*,
				dlv_d_fg.ItemId, dlv_d_fg.Qty, dlv_d_fg.UnitId
			FROM cte_dlv_plan_src cte
			INNER JOIN Sales.SalesDeliveryDetailFreeGood dlv_d_fg
				ON dlv_d_fg.Code = cte.DOCode
		)
		,cte_union AS (
			SELECT cte_n.*,
				0 AS FreeQty
			FROM cte_normal_src cte_n
			UNION ALL
			SELECT cte_f.Code, cte_f.[Date], cte_f.VehicleId, cte_f.DriverId, cte_f.WarehouseCode,
				cte_f.Notes, cte_f.CreatedBy, cte_f.TransCode, cte_f.DOCode,
				cte_f.ItemId, 0 AS Qty, cte_f.UnitId,
				cte_f.Qty AS FreeQty
			FROM cte_free_src cte_f
		)
		,cte_calc_group AS (
			SELECT Code, [Date], VehicleId, DriverId, WarehouseCode, Notes, CreatedBy, ItemId, UnitId,
				SUM(Qty) AS Qty,
				SUM(FreeQty) AS FreeQty
			FROM cte_union
			GROUP BY Code, [Date], VehicleId, DriverId, WarehouseCode, Notes, CreatedBy, ItemId, UnitId
		)
		SELECT cte.*,
			v.VehicleNo,
			e_d.Initial AS DriverInitial, e_d.FirstName AS DriverFirstName, e_d.LastName AS DriverLastName,
			w.Initial AS WarehouseInitial, w.[Name] AS WarehouseName,
			e_c.Initial AS CreatedInitial,
			i.Initial AS ItemInitial, i.[Name] AS ItemName,
			uom_c.UnitToConvert AS ItemUnitName
		FROM cte_calc_group cte
		LEFT JOIN General.Vehicle v
			ON v.Id = cte.VehicleId
		LEFT JOIN General.Employee e_d
			ON e_d.Id = cte.DriverId
		LEFT JOIN Inventory.Warehouse w
			ON w.Code = cte.WarehouseCode
		LEFT JOIN General.Employee e_c
			ON e_c.Id = cte.CreatedBy
		LEFT JOIN Inventory.Item i
			ON i.Id = cte.ItemId
		LEFT JOIN Inventory.UoMConversion uom_c
			ON uom_c.Id = cte.UnitId
		ORDER BY i.Initial, i.[Name], uom_c.UnitToConvert
	END

	ELSE IF @displayType = 'INVOICE'
	BEGIN
		WITH cte_dlv_plan_src AS (
			SELECT dlv_plan_h.Code,
				dlv_plan_d.TransCode,
				CASE WHEN ISNULL(si_d_do.Code, '') = '' THEN si_d_di.Code
					ELSE si_d_do.Code END AS InvCode
			FROM (
				SELECT *
				FROM Sales.DeliveryPlanHeader
				WHERE Code = @code
				AND Mark <> 'V'
			) dlv_plan_h
			LEFT JOIN Sales.DeliveryPlanDetail dlv_plan_d
				ON dlv_plan_d.Code = dlv_plan_h.Code
			LEFT JOIN Sales.SalesInvoiceDetail si_d_do
				ON si_d_do.DOCode = dlv_plan_d.TransCode
			LEFT JOIN Sales.SalesInvoiceDetail si_d_di
				ON si_d_di.Code = dlv_plan_d.TransCode
		)
		,cte_src_with_cust AS (
			SELECT cte.*,
				si_h.CustCode,
				c.Initial AS CustInitial, c.[Name] AS CustName,
				CASE WHEN so_h.BillingAddressId IS NULL THEN ca_d.Address1
					ELSE ca_b.Address1 END AS CustAddress1
			FROM cte_dlv_plan_src cte
			INNER JOIN Sales.SalesInvoiceHeader si_h
				ON si_h.Code = cte.InvCode
			LEFT JOIN Sales.SalesOrderHeader so_h
				ON so_h.Code = si_h.SOCode
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
		)
		SELECT *
		FROM cte_src_with_cust
		ORDER BY InvCode
	END

END";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop procedure dbo.sp_get_dlv_plan_packing_print_data
            var sql = @"DROP PROCEDURE [dbo].[sp_get_dlv_plan_packing_print_data]";
            migrationBuilder.Sql(sql);
        }
    }
}
