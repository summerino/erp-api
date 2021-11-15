using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AlterViewPurchaseReceiveDetail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			// Alter view Purchasing.vwPurchaseReceiveDetail
			var sql = @"ALTER VIEW [Purchasing].[vwPurchaseReceiveDetail]
AS
	SELECT rcv_d.*,
		CASE WHEN rcv_h.SrcTrans = 1 THEN ISNULL(po_d.Qty, 0)
			 WHEN rtn_h.Type = 2 THEN ISNULL(rtn_d.Qty, 0)
			 WHEN rtn_h.Type = 3 THEN ISNULL(rtnx_d.Qty, 0)
			 ELSE 0 END AS OrderQty,
		CASE WHEN rcv_h.SrcTrans = 1 THEN ISNULL(po_d.Qty, 0) - ISNULL(po_d.QtyRcv, 0)
			 WHEN rtn_h.Type = 2 THEN ISNULL(rtn_d.Qty, 0) - ISNULL(rtn_d.QtyRcv, 0)
			 WHEN rtn_h.Type = 3 THEN ISNULL(rtnx_d.Qty, 0) - ISNULL(rtnx_d.QtyRcv, 0)
			 ELSE 0 END AS OutstandingQty,
		i.Initial AS ItemInitial,
		i.[Name] AS ItemName,
		i.UomBuyId AS ItemUomBuyId,
		uom_c_b.UnitEquivalent AS ItemUomBuyName,
		i.BuyPrice AS ItemBuyPrice,
		uom.Initial AS UomInitial,
		uom_c.UnitEquivalent AS UnitName
	FROM Purchasing.PurchaseReceiveDetail rcv_d
	LEFT JOIN Purchasing.PurchaseReceiveHeader rcv_h
		ON rcv_h.Code = rcv_d.Code
	LEFT JOIN Purchasing.PurchaseReturnHeader rtn_h
		ON rtn_h.Code = rcv_h.TransCode
	LEFT JOIN Purchasing.PurchaseOrderDetail po_d
		ON po_d.Id = rcv_d.TransDetailId
		AND rcv_h.SrcTrans = 1
	LEFT JOIN Purchasing.PurchaseReturnDetail rtn_d
		ON rtn_d.Id = rcv_d.TransDetailId
		AND rcv_h.SrcTrans = 2
	LEFT JOIN Purchasing.PurchaseReturnDetailExchDiffItem rtnx_d
		ON rtnx_d.Id = rcv_d.TransDetailId
		AND rcv_h.SrcTrans = 2
	LEFT JOIN Inventory.Item i
		ON i.Id = rcv_d.ItemId
	LEFT JOIN Inventory.UoMConversion uom_c_b
		ON uom_c_b.Id = i.UomBuyId
	LEFT JOIN Inventory.UoM uom
		ON uom.Id = rcv_d.UomId
	LEFT JOIN Inventory.UoMConversion uom_c
		ON uom_c.Id = rcv_d.UnitId";
			migrationBuilder.Sql(sql);
		}

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
