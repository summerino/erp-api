using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP_API.Migrations.TenantMigrations
{
    public partial class CreateViewvwUom : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = @"Create View [Inventory].[vwUom]
                        AS
                        select uom.*, uc.initial as CreatedInitial, up.initial as UpdatedInitial
                        from Inventory.UoM uom
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
