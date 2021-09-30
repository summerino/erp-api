using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class UpdateViewInterCashbank : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Alter view Purchasing.vwPurchaseReturnHeader
            var sql = @"ALTER VIEW [Finance].[vwInterCashBankHeader]
AS
    SELECT gcbh.*,
        gcbd.TransCode,
        gcbh_t.CoaCode AS CoaCodeTo,
        gcbd.CurrCode AS CurrDetail,
        gcbd.Rate AS RateDetail,
        gcbd.Amount AS AmountDetail,
        gcbd.[Type] AS TypeDetail,
        gcbd.TypeAmount,
        gcbd.TransAmount,
        gcbd.Notes AS NotesDetail,
        c_f.[Name] AS CoaNameFrom,
        c_t.[Name] AS CoaNameTo,
        u_c.Initial AS CreatedInitial,
        u_u.Initial AS UpdatedInitial,
        u_a.Initial AS ApprovedInitial,
        CASE gcbh.Mark
            WHEN 'A' THEN 'Active'
            WHEN 'V' THEN 'Void'
            WHEN 'CMP' THEN 'Completed'
            WHEN 'CLS' THEN 'Closed' END AS [Status]
    FROM Finance.GeneralCashBankHeader gcbh
    LEFT JOIN Finance.GeneralCashBankDetail gcbd
        ON gcbh.Code = gcbd.Code and gcbd.[Type] = 'ICBO'
    LEFT JOIN Finance.GeneralCashBankHeader gcbh_t
        ON gcbh_t.Code = gcbd.TransCode
    LEFT JOIN Accounting.COA c_f
        ON c_f.Code = gcbh.CoaCode
    LEFT JOIN Accounting.COA c_t
        ON c_t.Code = gcbh_t.CoaCode
    LEFT JOIN SystemManagement.[User] u_c
        ON u_c.Id = gcbh.CreatedBy
    LEFT JOIN SystemManagement.[User] u_u
        ON u_u.Id = gcbh.UpdatedBy
    LEFT JOIN SystemManagement.[User] u_a
        ON u_a.Id = gcbh.ApprovedBy
    WHERE gcbd.TransCode IS NOT NULL";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
