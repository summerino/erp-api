using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Web.API.Migrations.TenantMigrations
{
    public partial class AlterViewUserAndOthers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Alter view SystemManagement.vwUser
            var sql = @"ALTER VIEW [SystemManagement].[vwUser]
AS
    SELECT a.*,
        r.[Name] As RoleName,
        e.Initial As EmployeeInitial,
        u.Initial AS UpdatedInitial
    FROM SystemManagement.[User] a
    LEFT JOIN SystemManagement.[User] u
        ON u.Id = a.UpdatedBy
    LEFT JOIN General.Employee e
        ON u.EmployeeId = e.Id
    LEFT JOIN SystemManagement.[Role] r
        ON a.RoleId = r.Id";
            migrationBuilder.Sql(sql);

            // Alter view Sales.vwSalesmanSchedule
            sql = @"ALTER VIEW [Sales].[vwSalesmanSchedule]
AS
    SELECT ss.Id AS SalesmanScheduleId,
        e.*,
        ss.AreaId1,
        a_1.[Name] AS AreaName1,
        ss.AreaId2,
        a_2.[Name] AS AreaName2,
        ss.AreaId3,
        a_3.[Name] AS AreaName3,
        ss.AreaId4,
        a_4.[Name] AS AreaName4,
        ss.AreaId5,
        a_5.[Name] AS AreaName5,
        ss.StartDate,
        ss.EndDate,
        ss.Recurrence,
        ss.VisitDay,
        LTRIM(RTRIM(e.FirstName)) + ' ' + LTRIM(RTRIM(e.LastName)) AS FullName
    FROM Sales.SalesmanSchedule ss
    LEFT JOIN General.Employee e
        ON ss.SalesmanId = e.Id
    LEFT JOIN Sales.Area a_1
        ON ss.AreaId1 = a_1.Id
    LEFT JOIN Sales.Area a_2
        ON ss.AreaId2 = a_2.Id
    LEFT JOIN Sales.Area a_3
        ON ss.AreaId3 = a_3.Id
    LEFT JOIN Sales.Area a_4
        ON ss.AreaId4 = a_4.Id
    LEFT JOIN Sales.Area a_5
        ON ss.AreaId5 = a_5.Id";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
