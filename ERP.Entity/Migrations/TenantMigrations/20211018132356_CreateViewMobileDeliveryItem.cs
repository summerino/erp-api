using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class CreateViewMobileDeliveryItem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create view MobileWarehouse.vwMobileDeliveryItemHeader
            var sql = @"CREATE VIEW [MobileWarehouse].[vwMobileDeliveryItemHeader]
AS
    SELECT md_h.*,
        u_c.Initial AS CreatedInitial,
        u_u.Initial AS UpdatedInitial,
        u_a.Initial AS ApprovedInitial,
        u_r.Initial AS RejectedInitial,
        CASE md_h.Mark
            WHEN 'A' THEN 'Aktif'
            WHEN 'APR' THEN 'Disetujui'
            WHEN 'REJ' THEN 'Ditolak' END AS [Status]
    FROM MobileWarehouse.MobileDeliveryItemHeader md_h
    LEFT JOIN SystemManagement.[User] u_c
        ON u_c.Id = md_h.CreatedBy
    LEFT JOIN SystemManagement.[User] u_u
        ON u_u.Id = md_h.UpdatedBy
    LEFT JOIN SystemManagement.[User] u_a
        ON u_a.Id = md_h.ApprovedBy
	LEFT JOIN SystemManagement.[User] u_r
        ON u_r.Id = md_h.RejectedBy";
            migrationBuilder.Sql(sql);

            // Create view MobileWarehouse.vwMobileDeliveryItemDetail
            sql = @"CREATE VIEW [MobileWarehouse].[vwMobileDeliveryItemDetail]
AS
	SELECT md_d.*,
		i.Initial AS ItemInitial,
		i.[Name] AS ItemName,
		i.UomSellId AS ItemUomSellId,
		uom_c_s.UnitEquivalent AS ItemUomSellName,
		i.SellPrice AS ItemSellPrice,
		uom.Initial AS UomInitial,
		uom_c.UnitEquivalent AS UnitName
	FROM MobileWarehouse.MobileDeliveryItemDetail md_d
	LEFT JOIN Inventory.Item i
		ON i.Id = md_d.ItemId
	LEFT JOIN Inventory.UoMConversion uom_c_s
		ON uom_c_s.Id = i.UomSellId
	LEFT JOIN Inventory.UoM uom
		ON uom.Id = md_d.UomId
	LEFT JOIN Inventory.UoMConversion uom_c
		ON uom_c.Id = md_d.UnitId";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop view MobileWarehouse.vwMobilePaymentMethod
            var sql = @"DROP VIEW [MobileWarehouse].[vwMobileDeliveryItemHeader]";
            migrationBuilder.Sql(sql);

            // Drop view MobileWarehouse.vwMobileDeliveryItemDetail
            sql = @"DROP VIEW [MobileWarehouse].[vwMobileDeliveryItemDetail]";
            migrationBuilder.Sql(sql);
        }
    }
}
