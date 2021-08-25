using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AddVwAttendanceReport : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = @"CREATE VIEW [HumanResource].[VwAttendanceReport] 
as
SELECT A.*, 
	Concat(A.CheckInLat, ' : ', A.CheckInLng) as CoordinatIn,
	Concat(A.CheckOutLat, ' : ', A.CheckOutLng) as CoordinatOut,
	E.Initial, CONCAT(E.FirstName, ' ', E.LastName) as [Name], 
	E.[Type], 
	Case 
		when E.[Type] = 1 then 'Karyawan'
		when E.[Type] = 2 then 'Penjual'
		when E.[Type] = 3 then 'Supir'
		when E.[Type] = 4 then 'Gudang'
		end as TypeName,
	E.SalesGroupId
FROM HumanResource.Attendance A
JOIN General.Employee E on A.EmployeeId = E.Id";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var sql = @"DROP VIEW [HumanResource].[VwAttendanceReport]";
            migrationBuilder.Sql(sql);
        }
    }
}
