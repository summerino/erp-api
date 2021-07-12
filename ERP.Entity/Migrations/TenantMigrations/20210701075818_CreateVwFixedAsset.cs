using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class CreateVwFixedAsset : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = @"CREATE VIEW [AssetManagement].[vwFixedAsset] as
SELECT FA.*, A.[Name] as AssetType, S.[Name] SupName, 
	u_c.Initial AS CreatedInitial,
    u_u.Initial AS UpdatedInitial,
    u_a.Initial AS ApprovedInitial,
    CASE FA.Mark
        WHEN 'A' THEN 'Active'
        WHEN 'V' THEN 'Void'
        WHEN 'CMP' THEN 'Completed' END AS [Status]
FROM [AssetManagement].[FixedAsset] FA
JOIN [AssetManagement].[AssetType] A ON FA.TypeId = A.Id
JOIN [General].[Supplier] S ON FA.SupCode = S.Code
LEFT JOIN SystemManagement.[User] u_c
    ON u_c.Id = FA.CreatedBy
LEFT JOIN SystemManagement.[User] u_u
    ON u_u.Id = FA.UpdatedBy
LEFT JOIN SystemManagement.[User] u_a
    ON u_a.Id = FA.ApprovedBy";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var sql = "DROP VIEW [AssetManagement].[vwFixedAsset]";
            migrationBuilder.Sql(sql);
        }
    }
}
