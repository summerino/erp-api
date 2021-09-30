using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AddingViewMobileItemRequest : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create view MobileSales.vwMobileItemRequestHeader
            var sql = @"CREATE VIEW [MobileSales].[vwMobileItemRequestHeader]
AS
    SELECT mir.*,
        u_c.Initial AS CreatedInitial,
        u_u.Initial AS UpdatedInitial,
        u_a.Initial AS ApprovedInitial,
        u_r.Initial AS RejectedInitial,
		emp.Initial AS SalesmanInitial,
		emp.FirstName AS SalesmanName,
        CASE mir.Mark
            WHEN 'A' THEN 'Active'
            WHEN 'APR' THEN 'Approved'
            WHEN 'REJ' THEN 'Rejected' END AS [Status]
    FROM MobileSales.MobileItemRequestHeader mir
    LEFT JOIN SystemManagement.[User] u_c
        ON u_c.Id = mir.CreatedBy
    LEFT JOIN SystemManagement.[User] u_u
        ON u_u.Id = mir.UpdatedBy
    LEFT JOIN SystemManagement.[User] u_a
        ON u_a.Id = mir.ApprovedBy
	LEFT JOIN SystemManagement.[User] u_r
        ON u_r.Id = mir.RejectedBy
	LEFT JOIN General.Employee emp
		ON emp.Id = mir.SalesmanId";
            migrationBuilder.Sql(sql);

            // Create view MobileSales.vwMobileItemRequestDetail
            sql = @"CREATE VIEW [MobileSales].[vwMobileItemRequestDetail]
AS
    SELECT mir.*,
        i.[Name] AS ItemName,
        uom_c.UnitEquivalent AS UnitName,
		uom_c.UomId AS UomId
    FROM MobileSales.MobileItemRequestDetail mir
    LEFT JOIN Inventory.Item i
        ON i.Id = mir.ItemId
    LEFT JOIN Inventory.UoMConversion uom_c
        ON uom_c.Id = mir.UnitId";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop view MobileSales.vwMobileItemRequestHeader
            var sql = @"DROP VIEW [Finance].[vwMobileItemRequestHeader]";
            migrationBuilder.Sql(sql);

            // Drop view MobileSales.vwMobileItemRequestDetail
            sql = @"DROP VIEW [Finance].[vwMobileItemRequestDetail]";
            migrationBuilder.Sql(sql);
        }
    }
}
