using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AlterViewVwCustomer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = @"ALTER VIEW [General].[vwCustomer]
AS
SELECT c.*, 
	ca.Initial AS InitialAddress, 
	ca.Address1, 
	ca.Address2, 
	ca.ContactPerson,
	ca.Phone,
	ca.Fax,
	ISNULL(ca.IsDefault, 0) AS IsDefault,
	t.[Name] AS TypeName,
	A1.[Name] as AreaName1,
	A2.[Name] as AreaName2,
	A3.[Name] as AreaName3,
	A4.[Name] as AreaName4,
	A5.[Name] as AreaName5,
	u.Initial as UpdatedInitial
FROM General.Customer c
LEFT JOIN General.CustomerType t
	ON t.Id = c.TypeId
LEFT JOIN General.CustomerAddress ca
	ON c.Code = ca.Code AND ca.IsDefault = 1
LEFT JOIN SystemManagement.[User] u
	ON u.Id = c.UpdatedBy
LEFT JOIN Sales.Area A1 on C.AreaId1 = A1.Id
LEFT JOIN Sales.Area A2 on C.AreaId2 = A2.Id
LEFT JOIN Sales.Area A3 on C.AreaId3 = A3.Id
LEFT JOIN Sales.Area A4 on C.AreaId4 = A4.Id
LEFT JOIN Sales.Area A5 on C.AreaId5 = A5.Id";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var sql = @"ALTER VIEW [General].[vwCustomer]
AS
SELECT c.*, 
	ca.Initial AS InitialAddress, 
	ca.Address1, 
	ca.Address2, 
	ca.ContactPerson,
	ca.Phone,
	ca.Fax,
	ISNULL(ca.IsDefault, 0) AS IsDefault,
	t.[Name] AS TypeName,
	u.Initial as UpdatedInitial
FROM General.Customer c
LEFT JOIN General.CustomerType t
	ON t.Id = c.TypeId
LEFT JOIN General.CustomerAddress ca
	ON c.Code = ca.Code AND ca.IsDefault = 1
LEFT JOIN SystemManagement.[User] u
	ON u.Id = c.UpdatedBy";
            migrationBuilder.Sql(sql);
        }
    }
}
