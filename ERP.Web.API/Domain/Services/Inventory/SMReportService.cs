using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Inventory;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP.Web.API.Domain.Services.Inventory
{
    public class SMReportService : ISMReportService
    {
        private readonly TenantContext _db;

        public SMReportService(TenantContext db)
        {
            _db = db;        
        }
		public DataSourceResult GetData(int type, string startDate, string endDate, string whCode, int itemId, int typeUnit, bool isSM, IEnumerable<Sort> sorts)
		{
			var qsetUnit = $"DECLARE @Unit int; SET @Unit = '{typeUnit.ToString().Replace("'", "''")}'; ";

			var smData = _db.ReportByStockMutations.FromSqlRaw(qsetUnit + @"SELECT sm.WarehouseCode, sm.ItemId, sm.Date, sm.RefCode1 as TransCode, 
					CASE 
					WHEN sm.Src IN ('DO','RCV') AND LEFT(sm.RefCode1,2) = 'DI' THEN 'Penjualan Langsung'
					WHEN sm.Src = 'RCV' THEN 'Penerimaan'
					WHEN sm.Src = 'DO' THEN 'Surat Jalan'
					WHEN sm.Src = 'TS' THEN 'Transfer Persediaan'
					WHEN sm.Src = 'ADJ' THEN 'Penyesuaian'
					END AS SrcTrans,
					CASE
					WHEN sm.Src = 'ADJ' AND sm.BaseQty > 0 THEN 
						CAST (CASE 
						WHEN @Unit = 1 THEN abs(sm.BaseQty) 
						WHEN @Unit = 2 THEN abs(sm.BaseQty) / ( SELECT EXP(SUM(LOG(Conversion))) FROM Inventory.UoMConversion WHERE UomId = im.UomId AND Seq <= (SELECT Seq FROM Inventory.UoMConversion WHERE Id = im.UomBuyId))
						WHEN @Unit = 3 THEN abs(sm.BaseQty) / ( SELECT EXP(SUM(LOG(Conversion))) FROM Inventory.UoMConversion WHERE UomId = im.UomId AND Seq <= (SELECT Seq FROM Inventory.UoMConversion WHERE Id = im.UomSellId))
						END AS decimal)
					WHEN sm.Src = 'RCV' THEN 
						CAST (CASE 
						WHEN @Unit = 1 THEN abs(sm.BaseQty) 
						WHEN @Unit = 2 THEN abs(sm.BaseQty) / ( SELECT EXP(SUM(LOG(Conversion))) FROM Inventory.UoMConversion WHERE UomId = im.UomId AND Seq <= (SELECT Seq FROM Inventory.UoMConversion WHERE Id = im.UomBuyId))
						WHEN @Unit = 3 THEN abs(sm.BaseQty) / ( SELECT EXP(SUM(LOG(Conversion))) FROM Inventory.UoMConversion WHERE UomId = im.UomId AND Seq <= (SELECT Seq FROM Inventory.UoMConversion WHERE Id = im.UomSellId))
						END AS decimal)
					WHEN sm.Src = 'TS' AND sm.[Type] = 'OH' AND sm.BaseQty > 0 THEN 
						CAST (CASE 
						WHEN @Unit = 1 THEN abs(sm.BaseQty) 
						WHEN @Unit = 2 THEN abs(sm.BaseQty) / ( SELECT EXP(SUM(LOG(Conversion))) FROM Inventory.UoMConversion WHERE UomId = im.UomId AND Seq <= (SELECT Seq FROM Inventory.UoMConversion WHERE Id = im.UomBuyId))
						WHEN @Unit = 3 THEN abs(sm.BaseQty) / ( SELECT EXP(SUM(LOG(Conversion))) FROM Inventory.UoMConversion WHERE UomId = im.UomId AND Seq <= (SELECT Seq FROM Inventory.UoMConversion WHERE Id = im.UomSellId))
						END AS decimal)
					ELSE CAST (0 AS decimal)
					END AS QtyIn,
					CASE
					WHEN sm.Src = 'ADJ' AND sm.BaseQty < 0 THEN 
						CAST (CASE 
						WHEN @Unit = 1 THEN abs(sm.BaseQty) 
						WHEN @Unit = 2 THEN abs(sm.BaseQty) / ( SELECT EXP(SUM(LOG(Conversion))) FROM Inventory.UoMConversion WHERE UomId = im.UomId AND Seq <= (SELECT Seq FROM Inventory.UoMConversion WHERE Id = im.UomBuyId))
						WHEN @Unit = 3 THEN abs(sm.BaseQty) / ( SELECT EXP(SUM(LOG(Conversion))) FROM Inventory.UoMConversion WHERE UomId = im.UomId AND Seq <= (SELECT Seq FROM Inventory.UoMConversion WHERE Id = im.UomSellId))
						END AS decimal)
					WHEN sm.Src = 'DO' THEN 
						CAST (CASE 
						WHEN @Unit = 1 THEN abs(sm.BaseQty) 
						WHEN @Unit = 2 THEN abs(sm.BaseQty) / ( SELECT EXP(SUM(LOG(Conversion))) FROM Inventory.UoMConversion WHERE UomId = im.UomId AND Seq <= (SELECT Seq FROM Inventory.UoMConversion WHERE Id = im.UomBuyId))
						WHEN @Unit = 3 THEN abs(sm.BaseQty) / ( SELECT EXP(SUM(LOG(Conversion))) FROM Inventory.UoMConversion WHERE UomId = im.UomId AND Seq <= (SELECT Seq FROM Inventory.UoMConversion WHERE Id = im.UomSellId))
						END AS decimal)
					WHEN sm.Src = 'TS' AND sm.[Type] = 'OH' AND sm.BaseQty < 0 THEN 
						CAST (CASE 
						WHEN @Unit = 1 THEN abs(sm.BaseQty) 
						WHEN @Unit = 2 THEN abs(sm.BaseQty) / ( SELECT EXP(SUM(LOG(Conversion))) FROM Inventory.UoMConversion WHERE UomId = im.UomId AND Seq <= (SELECT Seq FROM Inventory.UoMConversion WHERE Id = im.UomBuyId))
						WHEN @Unit = 3 THEN abs(sm.BaseQty) / ( SELECT EXP(SUM(LOG(Conversion))) FROM Inventory.UoMConversion WHERE UomId = im.UomId AND Seq <= (SELECT Seq FROM Inventory.UoMConversion WHERE Id = im.UomSellId))
						END AS decimal)
					ELSE CAST (0 AS decimal)
					END AS QtyOut,
					CAST (0 AS decimal) AS QtyEnd,
					CAST (CASE 
						WHEN @Unit = 1 THEN abs(sm.BaseNettPrice) 
						WHEN @Unit = 2 THEN abs(sm.BaseNettPrice) * ( SELECT EXP(SUM(LOG(Conversion))) FROM Inventory.UoMConversion WHERE UomId = im.UomId AND Seq <= (SELECT Seq FROM Inventory.UoMConversion WHERE Id = im.UomBuyId))
						WHEN @Unit = 3 THEN abs(sm.BaseNettPrice) * ( SELECT EXP(SUM(LOG(Conversion))) FROM Inventory.UoMConversion WHERE UomId = im.UomId AND Seq <= (SELECT Seq FROM Inventory.UoMConversion WHERE Id = im.UomSellId))
						END AS decimal)
					AS HPP,
					CAST (0 AS decimal) AS InvIn,
					CAST (0 AS decimal) AS InvOut,
					CAST (0 AS decimal) AS InvEnd
				FROM Inventory.StockMutation sm
				LEFT JOIN Inventory.Item im on im.Id = sm.ItemId
				WHERE sm.Src IN ('RCV','DO','TS','ADJ') AND sm.[Type] = 'OH' " + (string.IsNullOrEmpty(whCode) ? "" : $"AND sm.WarehouseCode = '{whCode.Replace("'", "''")}'") + " ORDER BY sm.Date").ToList();

			var itemData = _db.ReportByItems.FromSqlRaw(qsetUnit +
				@"SELECT im.Id, im.Initial, im.[Name],
					CASE 
					WHEN @Unit = 1 THEN (SELECT UnitEquivalent FROM Inventory.UoMConversion WHERE UomId = im.UomId AND IsBaseUnit = 1)
					WHEN @Unit = 2 THEN (SELECT UnitEquivalent FROM Inventory.UoMConversion WHERE Id = im.UomBuyId)
					WHEN @Unit = 3 THEN (SELECT UnitEquivalent FROM Inventory.UoMConversion WHERE Id = im.UomSellId)
					END AS Unit,
					CAST (0 AS decimal) AS QtyBegin,
					CAST (0 AS decimal) AS QtyIn,
					CAST (0 AS decimal) AS QtyOut,
					CAST (0 AS decimal) AS QtyEnd,
					CAST (0 AS decimal) AS InvBegin,
					CAST (0 AS decimal) AS InvIn,
					CAST (0 AS decimal) AS InvOut,
					CAST (0 AS decimal) AS InvEnd
				FROM Inventory.Item im " +
				(string.IsNullOrEmpty(whCode) ? "WHERE Id IN (SELECT ItemId FROM Inventory.WarehouseQuantity)" : $"WHERE Id IN (SELECT itemId FROM Inventory.WarehouseQuantity WHERE WarehouseCode = '{whCode}')")).ToList();

			var whData = _db.ReportByWarehouses.FromSqlRaw(@"SELECT wh.Initial, wh.Code, wh.Name,
							CAST (0 AS decimal) AS QtyBegin,
							CAST (0 AS decimal) AS QtyIn,
							CAST (0 AS decimal) AS QtyOut,
							CAST (0 AS decimal) AS QtyEnd,
							CAST (0 AS decimal) AS InvBegin,
							CAST (0 AS decimal) AS InvIn,
							CAST (0 AS decimal) AS InvOut,
							CAST (0 AS decimal) AS InvEnd
						FROM Inventory.Warehouse wh").ToList();

			var initData = smData.Where(x => x.Date < Convert.ToDateTime(startDate)).ToList();

			if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
				smData = smData.Where(x => x.Date >= Convert.ToDateTime(startDate) && x.Date <= Convert.ToDateTime(endDate)).ToList();
			}
			else if (!string.IsNullOrEmpty(startDate)) 
			{
				smData = smData.Where(x => x.Date >= Convert.ToDateTime(startDate)).ToList();
			}

			if(itemId > 0)
            {
				itemData = itemData.Where(x => x.Id == itemId).ToList();
				initData = initData.Where(x => x.ItemId == itemId).ToList();
			}

            foreach (var item in itemData)
            {
				item.QtyBegin = (initData.Where(x => x.ItemId == item.Id).Sum(x => x.QtyIn) - initData.Where(x => x.ItemId == item.Id).Sum(x => x.QtyOut));
				item.QtyIn = smData.Where(x => x.ItemId == item.Id).Sum(x => x.QtyIn);
				item.QtyOut = smData.Where(x => x.ItemId == item.Id).Sum(x => x.QtyOut);
				item.QtyEnd = item.QtyBegin + (item.QtyIn - item.QtyOut);
				item.InvBegin = (initData.Where(x => x.ItemId == item.Id).Sum(x => x.QtyIn * x.HPP) - initData.Where(x => x.ItemId == item.Id).Sum(x => x.QtyOut * x.HPP));
				item.InvIn = smData.Where(x => x.ItemId == item.Id).Sum(x => x.QtyIn * x.HPP);
				item.InvOut = smData.Where(x => x.ItemId == item.Id).Sum(x => x.QtyOut * x.HPP);
				item.InvEnd = item.InvBegin + (item.InvIn - item.InvOut);
			}

			foreach (var wh in whData)
            {
				wh.QtyBegin = (initData.Where(x => x.WarehouseCode == wh.Code).Sum(x => x.QtyIn) - initData.Where(x => x.WarehouseCode == wh.Code).Sum(x => x.QtyOut));
				wh.QtyIn = smData.Where(x => x.WarehouseCode == wh.Code).Sum(x => x.QtyIn);
				wh.QtyOut = smData.Where(x => x.WarehouseCode == wh.Code).Sum(x => x.QtyOut);
				wh.QtyEnd = wh.QtyBegin + (wh.QtyIn - wh.QtyOut);
				wh.InvBegin = (initData.Where(x => x.WarehouseCode == wh.Code).Sum(x => x.QtyIn * x.HPP) - initData.Where(x => x.WarehouseCode == wh.Code).Sum(x => x.QtyOut * x.HPP));
				wh.InvIn = smData.Where(x => x.WarehouseCode == wh.Code).Sum(x => x.QtyIn * x.HPP);
				wh.InvOut = smData.Where(x => x.WarehouseCode == wh.Code).Sum(x => x.QtyOut * x.HPP);
				wh.InvEnd = wh.InvBegin + (wh.InvIn - wh.InvOut);
			}

			if (type == 1)
			{
				if (isSM)
                {
					var selectedItem = itemData.FirstOrDefault(x => x.Id == itemId);
					var smItemData = smData.Where(x => x.ItemId == itemId);
					foreach (var item in smItemData)
                    {
						item.QtyEnd = item.QtyIn > 0 ? selectedItem.QtyBegin + item.QtyIn : selectedItem.QtyBegin - item.QtyOut;
						item.InvIn = item.QtyIn * item.HPP;
						item.InvOut = item.QtyOut * item.HPP;
						item.InvEnd = (selectedItem.InvBegin + item.InvIn) - item.InvOut;
						selectedItem.QtyBegin = item.QtyEnd;
						selectedItem.InvBegin = item.InvEnd;
                    }

					return smItemData.AsQueryable().ToDataSourceResult(0, smItemData.Count(), null, sorts);
				}
				else
                {
					return itemData.AsQueryable().ToDataSourceResult(0, itemData.Count(), null, sorts);
				}
			} 
			else
            {
				return whData.AsQueryable().ToDataSourceResult(0, whData.Count(), null, sorts);
			}
		}
    }
}
