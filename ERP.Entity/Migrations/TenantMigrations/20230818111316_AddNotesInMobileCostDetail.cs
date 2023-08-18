using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Entity.Migrations.TenantMigrations
{
    /// <inheritdoc />
    public partial class AddNotesInMobileCostDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Notes",
                schema: "MobileSales",
                table: "MobileCostDetail",
                type: "varchar(256)",
                unicode: false,
                maxLength: 256,
                nullable: true);

            // Refresh view MobileSales.vwMobileCostDetail
            var sql = @"exec sp_refreshview 'MobileSales.vwMobileCostDetail'";
            migrationBuilder.Sql(sql);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notes",
                schema: "MobileSales",
                table: "MobileCostDetail");
        }
    }
}
