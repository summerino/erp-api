using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AlterViewReportAttendance : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Alter view Purchasing.vwPurchaseOrderDetail
            var sql = @"ALTER VIEW [HumanResource].[VwAttendanceReport] 
AS
	SELECT a.*, 
		CONCAT(a.CheckInLat, ' : ', a.CheckInLng) AS CoordinatIn,
		CONCAT(a.CheckOutLat, ' : ', a.CheckOutLng) AS CoordinatOut,
		e.Initial, CONCAT(e.FirstName, ' ', e.LastName) AS [Name], 
		e.[Type], 
		CASE 
			WHEN e.[Type] = 1 THEN 'Karyawan'
			WHEN e.[Type] = 2 THEN 'Penjual'
			WHEN e.[Type] = 3 THEN 'Supir'
			WHEN e.[Type] = 4 THEN 'Gudang'
		END AS TypeName,
		e.SalesGroupId
	FROM HumanResource.Attendance a
	JOIN General.Employee e
		ON e.Id = a.EmployeeId";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
