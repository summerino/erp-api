using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Web.API.Migrations.TenantMigrations
{
    public partial class AlterViewVisitOrder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Alter view Sales.vwVisitOrder
            var sql = @"ALTER VIEW [Sales].[vwVisitOrder]
AS
    SELECT vo.*,
        e.Initial AS SalesmanInitial,
        e.FirstName + ' ' + e.LastName AS SalesmanName,
        sg.Initial AS GroupInitial,
        sg.[Name] AS GroupName,
        CASE WHEN vo.VisitPlanCode IS NULL
            THEN 'Manual'
            ELSE 'Jadwal Kunjungan' END AS SourceTransaction,
        CASE vo.Mark
            WHEN 'A' THEN 'Active'
            WHEN 'V' THEN 'Void' END AS [Status],
        u_c.Initial AS CreatedInitial,
        u_u.Initial AS UpdatedInitial,
        u_a.Initial AS ApprovedInitial
    FROM Sales.VisitOrder vo
    LEFT JOIN General.Employee e
        ON vo.SalesmanId = e.Id
    LEFT JOIN Sales.SalesmanGroup sg
        ON e.SalesGroupId = sg.Id
    LEFT JOIN SystemManagement.[User] u_c
        ON u_c.Id = vo.CreatedBy
    LEFT JOIN SystemManagement.[User] u_u
        ON u_u.Id = vo.UpdatedBy
    LEFT JOIN SystemManagement.[User] u_a
        ON u_a.Id = vo.ApprovedBy";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
