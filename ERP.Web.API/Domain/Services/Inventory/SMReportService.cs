using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.Inventory;
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
		public DataSourceResult GetData(int type, string startDate, string endDate, string whCode, int? itemId, int typeUnit, bool isSM)
		{
			List<ReportByStockMutation> smListData = new();
			var qsetUnit = $"DECLARE @Unit int; SET @Unit = '{typeUnit.ToString().Replace("'", "''")}'; ";

			var orderQuery = @" ORDER BY CASE
					WHEN sm.Src = 'ADJ' AND sm.BaseQty > 0 THEN
						1
					WHEN sm.Src IN('RCV', 'BB', 'SR') THEN
						1
					WHEN sm.Src IN('TS', 'CNEE') AND sm.[Type] = 'OH' AND sm.BaseQty > 0 THEN
						1
					WHEN sm.Src = 'ADJ' AND sm.BaseQty < 0 THEN
						2
					WHEN sm.Src IN('DO', 'DOF', 'PR') THEN
						2
					WHEN sm.Src IN('TS', 'CNEE') AND sm.[Type] = 'OH' AND sm.BaseQty < 0 THEN
						2
					ELSE 3
					END, sm.Date";

			var smData = _db.ReportByStockMutations.FromSqlRaw(qsetUnit + @"SELECT sm.WarehouseCode, sm.ItemId, sm.Date, sm.RefCode1 as TransCode, 
					CASE 
					WHEN sm.Src = 'DO' AND LEFT(sm.RefCode1,2) = 'DI' THEN 'Penjualan Langsung'
					WHEN sm.Src = 'RCV' THEN 'Penerimaan'
					WHEN sm.Src = 'DO' THEN 'Surat Jalan'
					WHEN sm.Src = 'TS' THEN 'Transfer Persediaan'
					WHEN sm.Src = 'ADJ' THEN 'Penyesuaian'
					WHEN sm.Src = 'BB' THEN 'Saldo Awal Persediaan'
					WHEN sm.Src = 'PR' THEN 'Retur Pembelian'
					WHEN sm.Src = 'SR' THEN 'Retur Penjualan'
					WHEN sm.Src = 'CNEE' THEN 'Konsinyasi'
					WHEN sm.Src = 'DOF' THEN 'Surat Jalan Bonus'
					END AS SrcTrans,
					CASE
					WHEN sm.Src = 'ADJ' AND sm.BaseQty > 0 THEN 
						CAST (CASE 
						WHEN @Unit = 1 THEN abs(sm.BaseQty) 
						WHEN @Unit = 2 THEN abs(sm.BaseQty) / ( SELECT EXP(SUM(LOG(Conversion))) FROM Inventory.UoMConversion WHERE UomId = im.UomId AND Seq <= (SELECT Seq FROM Inventory.UoMConversion WHERE Id = im.UomBuyId))
						WHEN @Unit = 3 THEN abs(sm.BaseQty) / ( SELECT EXP(SUM(LOG(Conversion))) FROM Inventory.UoMConversion WHERE UomId = im.UomId AND Seq <= (SELECT Seq FROM Inventory.UoMConversion WHERE Id = im.UomSellId))
						END AS decimal)
					WHEN sm.Src IN ('RCV', 'BB', 'SR') THEN 
						CAST (CASE 
						WHEN @Unit = 1 THEN abs(sm.BaseQty) 
						WHEN @Unit = 2 THEN abs(sm.BaseQty) / ( SELECT EXP(SUM(LOG(Conversion))) FROM Inventory.UoMConversion WHERE UomId = im.UomId AND Seq <= (SELECT Seq FROM Inventory.UoMConversion WHERE Id = im.UomBuyId))
						WHEN @Unit = 3 THEN abs(sm.BaseQty) / ( SELECT EXP(SUM(LOG(Conversion))) FROM Inventory.UoMConversion WHERE UomId = im.UomId AND Seq <= (SELECT Seq FROM Inventory.UoMConversion WHERE Id = im.UomSellId))
						END AS decimal)
					WHEN sm.Src IN ('TS', 'CNEE') AND sm.[Type] = 'OH' AND sm.BaseQty > 0 THEN 
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
					WHEN sm.Src IN ('DO', 'DOF', 'PR') THEN 
						CAST (CASE 
						WHEN @Unit = 1 THEN abs(sm.BaseQty) 
						WHEN @Unit = 2 THEN abs(sm.BaseQty) / ( SELECT EXP(SUM(LOG(Conversion))) FROM Inventory.UoMConversion WHERE UomId = im.UomId AND Seq <= (SELECT Seq FROM Inventory.UoMConversion WHERE Id = im.UomBuyId))
						WHEN @Unit = 3 THEN abs(sm.BaseQty) / ( SELECT EXP(SUM(LOG(Conversion))) FROM Inventory.UoMConversion WHERE UomId = im.UomId AND Seq <= (SELECT Seq FROM Inventory.UoMConversion WHERE Id = im.UomSellId))
						END AS decimal)
					WHEN sm.Src IN ('TS', 'CNEE') AND sm.[Type] = 'OH' AND sm.BaseQty < 0 THEN 
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
					CAST (0 AS decimal) AS InvEnd,
					CAST (0 AS bit) AS IsBold
				FROM Inventory.StockMutation sm
				LEFT JOIN Inventory.Item im on im.Id = sm.ItemId
				WHERE sm.Src IN ('RCV','DO','TS','ADJ','BB','PR','CNEE','DOF') AND sm.[Type] = 'OH' " + (string.IsNullOrEmpty(whCode) ? "" : $"AND sm.WarehouseCode = '{whCode.Replace("'", "''")}'") + orderQuery).ToList();

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

			smData = RemoveVoidSM(smData);

			if(itemId.HasValue)
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

					smListData.Add(new ReportByStockMutation
					{
						TransCode = "Nilai Awal",
						QtyEnd = selectedItem.QtyBegin,
						InvEnd = selectedItem.InvBegin,
						IsBold = true
					});

					foreach (var item in smItemData)
                    {
						var taxAmount = 0m;
						if (item.SrcTrans == "Penjualan Langsung" || item.SrcTrans == "Surat Jalan")
						{
							taxAmount = _db.SalesDeliveryDetails.FirstOrDefault(x => x.Code == item.TransCode && x.ItemId == item.ItemId).TaxAmount;
						}
						else if (item.SrcTrans == "Penerimaan")
                        {
							taxAmount = _db.PurchaseReceiveDetails.FirstOrDefault(x => x.Code == item.TransCode && x.ItemId == item.ItemId).TaxAmount;
						}
						else if (item.SrcTrans == "Retur Pembelian")
						{
							taxAmount = _db.PurchaseReturnDetails.FirstOrDefault(x => x.Code == item.TransCode && x.ItemId == item.ItemId).TaxAmount;
						}
						else if (item.SrcTrans == "Retur Penjualan")
						{
							taxAmount = _db.SalesReturnDetails.FirstOrDefault(x => x.Code == item.TransCode && x.ItemId == item.ItemId).TaxAmount;
						}

						item.QtyEnd = item.QtyIn > 0 ? selectedItem.QtyBegin + item.QtyIn : selectedItem.QtyBegin - item.QtyOut;
						item.InvIn = (item.QtyIn * item.HPP) - (item.QtyIn * taxAmount);
						item.InvOut = (item.QtyOut * item.HPP) - (item.QtyOut * taxAmount);
						item.InvEnd = (selectedItem.InvBegin + item.InvIn) - item.InvOut;
						selectedItem.QtyBegin = item.QtyEnd;
						selectedItem.InvBegin = item.InvEnd;

						smListData.Add(item);
                    }

					smListData.Add(new ReportByStockMutation
					{
						//Date = DateTime.MaxValue,
						TransCode = "Total",
						QtyIn = smListData.Where(x => !x.IsBold).Sum(x => x.QtyIn),
						QtyOut = smListData.Where(x => !x.IsBold).Sum(x => x.QtyOut),
						QtyEnd = selectedItem.QtyEnd,
						InvIn = smListData.Where(x => !x.IsBold).Sum(x => x.InvIn),
						InvOut = smListData.Where(x => !x.IsBold).Sum(x => x.InvOut),
						InvEnd = selectedItem.InvEnd,
						IsBold = true
					});

					return smListData.AsQueryable().ToDataSourceResult(0, smListData.Count(), null, null);
				}
				else
                {
					return itemData.AsQueryable().ToDataSourceResult(0, itemData.Count(), null, null);
				}
			} 
			else
            {
				return whData.AsQueryable().ToDataSourceResult(0, whData.Count(), null, null);
			}
		}

		private List<ReportByStockMutation> RemoveVoidSM(List<ReportByStockMutation> data)
        {
			var result = data;
			var listVoid = new List<string>();

			listVoid.AddRange(_db.PurchaseReceiveHeaders.Where(x => x.Mark == "V").Select(x => x.Code).ToList());
			listVoid.AddRange(_db.SalesDeliveryHeaders.Where(x => x.Mark == "V").Select(x => x.Code).ToList());
			listVoid.AddRange(_db.AdjustmentHeaders.Where(x => x.Mark == "V").Select(x => x.Code).ToList());
			listVoid.AddRange(_db.TransferStockHeaders.Where(x => x.Mark == "V").Select(x => x.Code).ToList());
			listVoid.AddRange(_db.PurchaseReturnHeaders.Where(x => x.Mark == "V").Select(x => x.Code).ToList());

			result = result.Where(x => !listVoid.Contains(x.TransCode)).ToList();

			return result;
        }
    }
}
