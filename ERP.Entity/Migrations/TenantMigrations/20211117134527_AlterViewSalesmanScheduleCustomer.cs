using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AlterViewSalesmanScheduleCustomer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Alter view Sales.vwSalesmanScheduleCustomer
            var sql = @"ALTER VIEW [Sales].[vwSalesmanScheduleCustomer]
AS
    SELECT ss.Id AS SalesmanScheduleId,
        c.*,
        a_1.[Name] AS AreaName1,
        a_2.[Name] AS AreaName2,
        a_3.[Name] AS AreaName3,
        a_4.[Name] AS AreaName4,
        a_5.[Name] AS AreaName5,
		ca.Address1 AS Address1
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
        ON c.AreaId5 = a_5.Id
	LEFT JOIN General.CustomerAddress ca
		ON c.BillingAddressId = ca.Id";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
