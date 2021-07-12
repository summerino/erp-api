using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AlterVwAdjustmentHeader : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = @"ALTER VIEW [Inventory].[vwAdjustmentHeader]
AS
	SELECT adj_h.*,
		Case adj_h.[Type]
			WHEN 1 THEN 'Penyesuaian'
			WHEN 2 THEN 'Perhitungan Persediaan' END AS [Types],
		w.Initial AS WarehouseInitial,
		u_c.Initial AS CreatedInitial,
		u_u.Initial AS UpdatedInitial,
		u_a.Initial AS ApprovedInitial,
		CASE adj_h.Mark
			WHEN 'A' THEN 'Active'
			WHEN 'V' THEN 'Void' END AS [Status]
	FROM Inventory.AdjustmentHeader adj_h
	LEFT JOIN Inventory.Warehouse w
		ON w.Code = adj_h.WarehouseCode
	LEFT JOIN SystemManagement.[User] u_c
		ON u_c.Id = adj_h.CreatedBy
	LEFT JOIN SystemManagement.[User] u_u
		ON u_u.Id = adj_h.UpdatedBy
	LEFT JOIN SystemManagement.[User] u_a
		ON u_a.Id = adj_h.ApprovedBy";
			migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
			var sql = @"ALTER VIEW [Inventory].[vwAdjustmentHeader]
AS
	SELECT adj_h.*,
		w.Initial AS WarehouseInitial,
		u_c.Initial AS CreatedInitial,
		u_u.Initial AS UpdatedInitial,
		u_a.Initial AS ApprovedInitial,
		CASE adj_h.Mark
			WHEN 'A' THEN 'Active'
			WHEN 'V' THEN 'Void' END AS [Status]
	FROM Inventory.AdjustmentHeader adj_h
	LEFT JOIN Inventory.Warehouse w
		ON w.Code = adj_h.WarehouseCode
	LEFT JOIN SystemManagement.[User] u_c
		ON u_c.Id = adj_h.CreatedBy
	LEFT JOIN SystemManagement.[User] u_u
		ON u_u.Id = adj_h.UpdatedBy
	LEFT JOIN SystemManagement.[User] u_a
		ON u_a.Id = adj_h.ApprovedBy";
			migrationBuilder.Sql(sql);
		}
    }
}
