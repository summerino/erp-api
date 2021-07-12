using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Web.API.Migrations.TenantMigrations
{
    public partial class CreateViewVisitPlanAndOthers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create view Sales.vwVisitPlanHeader
            var sql = @"CREATE VIEW [Sales].[vwVisitPlanHeader]
AS
    SELECT vph.*,
        sg.Initial AS GroupInitial,
        sg.[Name] AS GroupName,
        e.FirstName + ' ' + e.LastName AS SupervisorName,
        u_c.Initial AS CreatedInitial,
        u_u.Initial AS UpdatedInitial,
        u_a.Initial AS ApprovedInitial,
        CASE vph.Mark
            WHEN 'A' THEN 'Active'
            WHEN 'V' THEN 'Void' END AS [Status]
    FROM Sales.VisitPlanHeader vph
    LEFT JOIN Sales.SalesmanGroup sg
        ON vph.GroupId = sg.Id
    LEFT JOIN General.Employee e
        ON sg.SupervisorId = e.Id
    LEFT JOIN SystemManagement.[User] u_c
        ON u_c.Id = vph.CreatedBy
    LEFT JOIN SystemManagement.[User] u_u
        ON u_u.Id = vph.UpdatedBy
    LEFT JOIN SystemManagement.[User] u_a
        ON u_a.Id = vph.ApprovedBy";
            migrationBuilder.Sql(sql);

            // Create view Sales.vwVisitPlanDetail
            sql = @"CREATE VIEW [Sales].[vwVisitPlanDetail]
AS
    SELECT vpd.Id AS VisitPlanDetailId,
        vpd.Code, e.*,
        ss.AreaId1,
        ss.AreaId2,
        ss.AreaId3,
        ss.AreaId4,
        ss.AreaId5,
        ss.StartDate,
        ss.EndDate,
        ss.Recurrence,
        ss.VisitDay,
        LTRIM(RTRIM(e.FirstName)) + ' ' + LTRIM(RTRIM(e.LastName)) AS FullName,
        sg.Initial AS GroupInitial,
        sg.[Name] AS GroupNameInitial,
        CASE WHEN e.Sex = 1
            THEN 'Pria'
            ELSE 'Wanita'
            END AS GenderInitial
    FROM Sales.VisitPlanDetail vpd
    INNER JOIN General.Employee e
        ON vpd.SalesmanId = e.Id
    LEFT JOIN Sales.SalesmanSchedule ss
        ON vpd.SalesmanId = ss.SalesmanId
    LEFT JOIN Sales.SalesmanGroup sg
        ON e.SalesGroupId = sg.Id";
            migrationBuilder.Sql(sql);

            // Create view Sales.vwVisitPlanDetailCustomer
            sql = @"CREATE VIEW [Sales].[vwVisitPlanDetailCustomer]
AS
    SELECT vpdc.VisitPlanDetailId,
           c.Code,
           c.Initial,
           c.[Name],
           a_1.[Name] AS AreaName1,
           a_2.[Name] AS AreaName2,
           a_3.[Name] AS AreaName3,
           a_4.[Name] AS AreaName4,
           a_5.[Name] AS AreaName5,
           c.Email,
           c.Website,
           c.TypeId
    FROM Sales.VisitPlanDetailCustomer vpdc
    INNER JOIN General.Customer c
        ON vpdc.CustCode = c.Code
    LEFT JOIN Sales.Area a_1
        ON c.AreaId1 = a_1.Id
    LEFT JOIN Sales.Area a_2
        ON c.AreaId2 = a_2.Id
    LEFT JOIN Sales.Area a_3
        ON c.AreaId3 = a_3.Id
    LEFT JOIN Sales.Area a_4
        ON c.AreaId4 = a_4.Id
    LEFT JOIN Sales.Area a_5
        ON c.AreaId5 = a_5.Id";
            migrationBuilder.Sql(sql);

            // Create view Sales.vwSalesmanSchedule
            sql = @"CREATE VIEW [Sales].[vwSalesmanSchedule]
AS
    SELECT ss.Id AS SalesmanScheduleId,
        e.*,
        a_1.[Name] AS AreaName1,
        a_2.[Name] AS AreaName2,
        a_3.[Name] AS AreaName3,
        a_4.[Name] AS AreaName4,
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

            // Create view Sales.vwSalesmanScheduleCustomer
            sql = @"CREATE VIEW [Sales].[vwSalesmanScheduleCustomer]
AS
    SELECT ss.Id AS SalesmanScheduleId,
        c.*,
        a_1.[Name] AS AreaName1,
        a_2.[Name] AS AreaName2,
        a_3.[Name] AS AreaName3,
        a_4.[Name] AS AreaName4,
        a_5.[Name] AS AreaName5
    FROM Sales.SalesmanScheduleCustomer ssc
    LEFT JOIN Sales.SalesmanSchedule ss
        ON ssc.SalesmanScheduleId = ss.Id
    LEFT JOIN General.Customer c
        ON ssc.CustCode = c.Code
    LEFT JOIN Sales.Area a_1
        ON c.AreaId1 = a_1.Id
    LEFT JOIN Sales.Area a_2
        ON c.AreaId2 = a_2.Id
    LEFT JOIN Sales.Area a_3
        ON c.AreaId3 = a_3.Id
    LEFT JOIN Sales.Area a_4
        ON c.AreaId4 = a_4.Id
    LEFT JOIN Sales.Area a_5
        ON c.AreaId5 = a_5.Id";
            migrationBuilder.Sql(sql);

            // Create view Sales.
            sql = @"CREATE VIEW [Sales].[vwVisitOrder]
AS
    SELECT vo.*,
        voc.CustCode,
        c.[Name] AS CustomerName,
        voi.InvCode,
        CASE vo.Mark
            WHEN 'A' THEN 'Active'
            WHEN 'V' THEN 'Void' END AS [Status],
        u_c.Initial AS CreatedInitial,
        u_u.Initial AS UpdatedInitial,
        u_a.Initial AS ApprovedInitial
    FROM Sales.VisitOrder vo
    LEFT JOIN Sales.VisitOrderCustomer voc
        ON vo.Code = voc.Code
    LEFT JOIN Sales.VisitOrderInvoice voi
        ON vo.Code = voi.Code
    LEFT JOIN General.Customer c
        ON voc.CustCode = c.Code
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
            // Drop view Sales.vwVisitPlanHeader
            var sql = @"DROP VIEW [Sales].[vwVisitPlanHeader]";
            migrationBuilder.Sql(sql);

            // Drop view Sales.vwVisitPlanDetail
            sql = @"DROP VIEW [Sales].[vwVisitPlanDetail]";
            migrationBuilder.Sql(sql);

            // Drop view Sales.vwVisitPlanDetailCustomer
            sql = @"DROP VIEW [Sales].[vwVisitPlanDetailCustomer]";
            migrationBuilder.Sql(sql);

            // Drop view Sales.vwSalesmanSchedule
            sql = @"DROP VIEW [Sales].[vwSalesmanSchedule]";
            migrationBuilder.Sql(sql);

            // Drop view Sales.vwSalesmanScheduleCustomer
            sql = @"DROP VIEW [Sales].[vwSalesmanScheduleCustomer]";
            migrationBuilder.Sql(sql);

            // Drop view Sales.vwVisitOrder
            sql = @"DROP VIEW [Sales].[vwVisitOrder]";
            migrationBuilder.Sql(sql);
        }
    }
}
