using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class CreateViewvwUom : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = @"CREATE VIEW [Inventory].[vwUoM]
AS
SELECT uom.*,
    uc.initial AS CreatedInitial,
    up.initial AS UpdatedInitial
FROM Inventory.UoM uom
LEFT JOIN SystemManagement.[User] uc
	ON uc.Id = uom.CreatedBy
LEFT JOIN SystemManagement.[User] up
	ON up.Id = uom.UpdatedBy";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var sql = @"DROP View [Inventory].[vwUom]";
            migrationBuilder.Sql(sql);
        }
    }
}
