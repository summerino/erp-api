using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AddingSysParCodeColumnInCashBankType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SysParCode",
                schema: "Finance",
                table: "CashBankType",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true);

            // Re-order column Finance.CashBankType
            var sql = @"BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE Finance.Tmp_CashBankType
	(
	Code varchar(5) NOT NULL,
	Name varchar(50) NOT NULL,
	SysParCode varchar(50) NULL,
	Seq smallint NOT NULL,
	IsActive bit NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Finance.Tmp_CashBankType SET (LOCK_ESCALATION = TABLE)
GO
IF EXISTS(SELECT * FROM Finance.CashBankType)
	 EXEC('INSERT INTO Finance.Tmp_CashBankType (Code, Name, SysParCode, Seq, IsActive)
		SELECT Code, Name, SysParCode, Seq, IsActive FROM Finance.CashBankType WITH (HOLDLOCK TABLOCKX)')
GO
ALTER TABLE Finance.GeneralCashBankDetail
	DROP CONSTRAINT FK_GeneralCashBankDetail_CashBankType_Type
GO
DROP TABLE Finance.CashBankType
GO
EXECUTE sp_rename N'Finance.Tmp_CashBankType', N'CashBankType', 'OBJECT' 
GO
ALTER TABLE Finance.CashBankType ADD CONSTRAINT
	PK_CashBankType PRIMARY KEY CLUSTERED 
	(
	Code
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Finance.GeneralCashBankDetail WITH NOCHECK ADD CONSTRAINT
	FK_GeneralCashBankDetail_CashBankType_Type FOREIGN KEY
	(
	Type
	) REFERENCES Finance.CashBankType
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Finance.GeneralCashBankDetail
	NOCHECK CONSTRAINT FK_GeneralCashBankDetail_CashBankType_Type
GO
ALTER TABLE Finance.GeneralCashBankDetail SET (LOCK_ESCALATION = TABLE)
GO
COMMIT";
            migrationBuilder.Sql(sql);

            // Alter view Inventory.vwTransferStockHeader
            sql = @"ALTER VIEW [Inventory].[vwTransferStockHeader]
AS
    SELECT ts_h.*,
        w_f.Initial AS WarehouseInitialFrom,
        w_t.Initial AS WarehouseInitialTo,
        u_c.Initial AS CreatedInitial,
        u_u.Initial AS UpdatedInitial,
        u_a.Initial AS ApprovedInitial,
        CASE ts_h.Mark
            WHEN 'A' THEN 'Active'
            WHEN 'V' THEN 'Void'
            WHEN 'CMP' THEN 'Completed' END AS [Status],
        CASE ts_h.[Type]
            WHEN 'OUT' THEN 'Barang Keluar'
            WHEN 'IN' THEN 'Barang Masuk'
            WHEN 'DT' THEN 'Transfer Langsung'
			WHEN 'C' THEN 'Titip Barang'
			WHEN 'RC' THEN 'Retur Titipan'END AS TypeInitial
    FROM Inventory.TransferStockHeader ts_h
    LEFT JOIN Inventory.Warehouse w_f
        ON w_f.Code = ts_h.WarehouseCodeFrom
    LEFT JOIN Inventory.Warehouse w_t
        ON w_t.Code = ts_h.WarehouseCodeTo
    LEFT JOIN SystemManagement.[User] u_c
        ON u_c.Id = ts_h.CreatedBy
    LEFT JOIN SystemManagement.[User] u_u
        ON u_u.Id = ts_h.UpdatedBy
    LEFT JOIN SystemManagement.[User] u_a
        ON u_a.Id = ts_h.ApprovedBy";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SysParCode",
                schema: "Finance",
                table: "CashBankType");
        }
    }
}
