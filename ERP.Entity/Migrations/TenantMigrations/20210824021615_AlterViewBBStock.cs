using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AlterViewBBStock : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = @"ALTER VIEW [Inventory].[vwBeginningBalanceStockDetail]
AS
	SELECT bb_d.*,
		i.[Name] AS ItemName,
		i.Initial AS ItemInitial
	FROM Inventory.BeginningBalanceDetail bb_d
	LEFT JOIN Inventory.Item i
		ON i.Id = bb_d.ItemId";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
