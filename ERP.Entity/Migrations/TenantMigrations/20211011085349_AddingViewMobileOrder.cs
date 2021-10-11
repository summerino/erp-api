using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AddingViewMobileOrder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create view MobileSales.vwMobileOrderHeader
            var sql = @"CREATE VIEW [MobileSales].[vwMobileOrderHeader]
AS
    SELECT mo_h.*,
        c.[Name] AS CustName,
        e.Initial AS SalesInitial,
        u_c.Initial AS CreatedInitial,
        u_u.Initial AS UpdatedInitial,
        u_a.Initial AS ApprovedInitial,
        u_r.Initial AS RejectedInitial,
        CASE mo_h.Mark
            WHEN 'A' THEN 'Aktif'
            WHEN 'APR' THEN 'Disetujui'
            WHEN 'REJ' THEN 'Ditolak' END AS [Status]
    FROM MobileSales.MobileOrderHeader mo_h
    LEFT JOIN General.Customer c
        ON c.Code = mo_h.CustCode
    LEFT JOIN General.Employee e
        ON e.Id = mo_h.SalesBy
    LEFT JOIN SystemManagement.[User] u_c
        ON u_c.Id = mo_h.CreatedBy
    LEFT JOIN SystemManagement.[User] u_u
        ON u_u.Id = mo_h.UpdatedBy
    LEFT JOIN SystemManagement.[User] u_a
        ON u_a.Id = mo_h.ApprovedBy
	LEFT JOIN SystemManagement.[User] u_r
        ON u_r.Id = mo_h.RejectedBy";
            migrationBuilder.Sql(sql);

            // Create view MobileSales.vwMobileOrderDetail
            sql = @"CREATE VIEW [MobileSales].[vwMobileOrderDetail]
AS
	SELECT mo_d.*,
		i.Initial AS ItemInitial,
		i.[Name] AS ItemName,
		i.UomSellId AS ItemUomSellId,
		uom_c_s.UnitEquivalent AS ItemUomSellName,
		i.SellPrice AS ItemSellPrice,
		uom.Initial AS UomInitial,
		uom_c.UnitEquivalent AS UnitName
	FROM MobileSales.MobileOrderDetail mo_d
	LEFT JOIN Inventory.Item i
		ON i.Id = mo_d.ItemId
	LEFT JOIN Inventory.UoMConversion uom_c_s
		ON uom_c_s.Id = i.UomSellId
	LEFT JOIN Inventory.UoM uom
		ON uom.Id = mo_d.UomId
	LEFT JOIN Inventory.UoMConversion uom_c
		ON uom_c.Id = mo_d.UnitId\";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop view MobileSales.vwMobileOrderHeader
            var sql = @"DROP VIEW [MobileSales].[vwMobileOrderHeader]";
            migrationBuilder.Sql(sql);

            // Drop view MobileSales.vwMobileOrderDetail
            sql = @"DROP VIEW [MobileSales].[vwMobileOrderDetail]";
            migrationBuilder.Sql(sql);
        }
    }
}
