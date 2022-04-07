using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.Inventory;
using ERP.Web.API.Domain.Interfaces.Inventory;
using Microsoft.EntityFrameworkCore;

namespace ERP.Web.API.Domain.Services.Inventory;

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

        var orderQuery = @" ORDER BY sm.Date, CASE
					WHEN sm.Src = 'BB' THEN 1
					WHEN sm.Src = 'RCV' AND rcv.SrcTrans = 1 THEN 2
					WHEN sm.Src = 'RCV' AND rcv.SrcTrans = 2 THEN 3
					WHEN sm.Src = 'ADJ' AND sm.BaseQty > 0 THEN 3
					WHEN sm.Src = 'SR' THEN 3
					WHEN sm.Src IN('TS', 'CNEE') AND sm.[Type] = 'OH' AND sm.BaseQty > 0 THEN 3
					WHEN sm.Src = 'DO' AND sr.[Type] = 2 THEN 4
					WHEN sm.Src = 'ADJ' AND sm.BaseQty < 0 THEN 5
					WHEN sm.Src IN('DO', 'DOF', 'PR') THEN 5
					WHEN sm.Src IN('TS', 'CNEE') AND sm.[Type] = 'OH' AND sm.BaseQty < 0 THEN 5
					ELSE 6
					END";

        var smData = _db.ReportByStockMutations.FromSqlRaw(qsetUnit + @"SELECT sm.WarehouseCode, sm.ItemId, sm.Date, sm.RefCode1 as TransCode, 
					CASE 
					WHEN sm.Src = 'DO' AND do.FromDirectInvoice = 1 THEN 'Penjualan Langsung'
					WHEN sm.Src = 'RCV' THEN 'Penerimaan'
					WHEN sm.Src = 'DO' THEN 'Surat Jalan'
					WHEN sm.Src = 'TS' THEN 'Transfer Persediaan'
					WHEN sm.Src = 'ADJ' THEN 'Penyesuaian'
					WHEN sm.Src = 'BB' THEN 'Saldo Awal Persediaan'
					WHEN sm.Src = 'PR' THEN 'Retur Pembelian'
					WHEN sm.Src = 'SR' THEN 'Retur Penjualan'
					WHEN sm.Src = 'CNEE' THEN 'Konsinyasi'
					WHEN sm.Src = 'DOF' THEN 'Surat Jalan Bonus'
					WHEN sm.Src = 'BB' THEN 'Saldo Awal'
					END AS SrcTrans,
					CASE
					WHEN sm.Src = 'ADJ' AND sm.BaseQty > 0 THEN 
						CAST (CASE 
						WHEN @Unit = 1 THEN abs(sm.BaseQty) 
						WHEN @Unit = 2 THEN abs(sm.BaseQty) / ( SELECT EXP(SUM(LOG(Conversion))) FROM Inventory.UoMConversion WHERE UomId = im.UomId AND Seq <= (SELECT Seq FROM Inventory.UoMConversion WHERE Id = im.UomBuyId))
						WHEN @Unit = 3 THEN abs(sm.BaseQty) / ( SELECT EXP(SUM(LOG(Conversion))) FROM Inventory.UoMConversion WHERE UomId = im.UomId AND Seq <= (SELECT Seq FROM Inventory.UoMConversion WHERE Id = im.UomSellId))
						END AS decimal(18,6))
					WHEN sm.Src IN ('RCV', 'BB', 'SR') THEN 
						CAST (CASE 
						WHEN @Unit = 1 THEN abs(sm.BaseQty) 
						WHEN @Unit = 2 THEN abs(sm.BaseQty) / ( SELECT EXP(SUM(LOG(Conversion))) FROM Inventory.UoMConversion WHERE UomId = im.UomId AND Seq <= (SELECT Seq FROM Inventory.UoMConversion WHERE Id = im.UomBuyId))
						WHEN @Unit = 3 THEN abs(sm.BaseQty) / ( SELECT EXP(SUM(LOG(Conversion))) FROM Inventory.UoMConversion WHERE UomId = im.UomId AND Seq <= (SELECT Seq FROM Inventory.UoMConversion WHERE Id = im.UomSellId))
						END AS decimal(18,6))
					WHEN sm.Src IN ('TS', 'CNEE') AND sm.[Type] = 'OH' AND sm.BaseQty > 0 THEN 
						CAST (CASE 
						WHEN @Unit = 1 THEN abs(sm.BaseQty) 
						WHEN @Unit = 2 THEN abs(sm.BaseQty) / ( SELECT EXP(SUM(LOG(Conversion))) FROM Inventory.UoMConversion WHERE UomId = im.UomId AND Seq <= (SELECT Seq FROM Inventory.UoMConversion WHERE Id = im.UomBuyId))
						WHEN @Unit = 3 THEN abs(sm.BaseQty) / ( SELECT EXP(SUM(LOG(Conversion))) FROM Inventory.UoMConversion WHERE UomId = im.UomId AND Seq <= (SELECT Seq FROM Inventory.UoMConversion WHERE Id = im.UomSellId))
						END AS decimal)
					ELSE CAST (0 AS decimal(18,6))
					END AS QtyIn,
					CASE
					WHEN sm.Src = 'ADJ' AND sm.BaseQty < 0 THEN 
						CAST (CASE 
						WHEN @Unit = 1 THEN abs(sm.BaseQty) 
						WHEN @Unit = 2 THEN abs(sm.BaseQty) / ( SELECT EXP(SUM(LOG(Conversion))) FROM Inventory.UoMConversion WHERE UomId = im.UomId AND Seq <= (SELECT Seq FROM Inventory.UoMConversion WHERE Id = im.UomBuyId))
						WHEN @Unit = 3 THEN abs(sm.BaseQty) / ( SELECT EXP(SUM(LOG(Conversion))) FROM Inventory.UoMConversion WHERE UomId = im.UomId AND Seq <= (SELECT Seq FROM Inventory.UoMConversion WHERE Id = im.UomSellId))
						END AS decimal(18,6))
					WHEN sm.Src IN ('DO', 'DOF', 'PR') THEN 
						CAST (CASE 
						WHEN @Unit = 1 THEN abs(sm.BaseQty) 
						WHEN @Unit = 2 THEN abs(sm.BaseQty) / ( SELECT EXP(SUM(LOG(Conversion))) FROM Inventory.UoMConversion WHERE UomId = im.UomId AND Seq <= (SELECT Seq FROM Inventory.UoMConversion WHERE Id = im.UomBuyId))
						WHEN @Unit = 3 THEN abs(sm.BaseQty) / ( SELECT EXP(SUM(LOG(Conversion))) FROM Inventory.UoMConversion WHERE UomId = im.UomId AND Seq <= (SELECT Seq FROM Inventory.UoMConversion WHERE Id = im.UomSellId))
						END AS decimal(18,6))
					WHEN sm.Src IN ('TS', 'CNEE') AND sm.[Type] = 'OH' AND sm.BaseQty < 0 THEN 
						CAST (CASE 
						WHEN @Unit = 1 THEN abs(sm.BaseQty) 
						WHEN @Unit = 2 THEN abs(sm.BaseQty) / ( SELECT EXP(SUM(LOG(Conversion))) FROM Inventory.UoMConversion WHERE UomId = im.UomId AND Seq <= (SELECT Seq FROM Inventory.UoMConversion WHERE Id = im.UomBuyId))
						WHEN @Unit = 3 THEN abs(sm.BaseQty) / ( SELECT EXP(SUM(LOG(Conversion))) FROM Inventory.UoMConversion WHERE UomId = im.UomId AND Seq <= (SELECT Seq FROM Inventory.UoMConversion WHERE Id = im.UomSellId))
						END AS decimal(18,6))
					ELSE CAST (0 AS decimal(18,6))
					END AS QtyOut,
					CAST (0 AS decimal(18,6)) AS QtyEnd,
					CAST (CASE 
						WHEN @Unit = 1 THEN abs(sm.BaseNettPrice) 
						WHEN @Unit = 2 THEN abs(sm.BaseNettPrice) * ( SELECT EXP(SUM(LOG(Conversion))) FROM Inventory.UoMConversion WHERE UomId = im.UomId AND Seq <= (SELECT Seq FROM Inventory.UoMConversion WHERE Id = im.UomBuyId))
						WHEN @Unit = 3 THEN abs(sm.BaseNettPrice) * ( SELECT EXP(SUM(LOG(Conversion))) FROM Inventory.UoMConversion WHERE UomId = im.UomId AND Seq <= (SELECT Seq FROM Inventory.UoMConversion WHERE Id = im.UomSellId))
						END AS decimal(19,6))
					AS HPP,
					CAST (0 AS decimal(19,6)) AS InvIn,
					CAST (0 AS decimal(19,6)) AS InvOut,
					CAST (0 AS decimal(19,6)) AS InvEnd,
					CAST (0 AS bit) AS IsBold,
					CASE 
						WHEN sm.Src = 'RCV' AND rcv.SrcTrans = 1 THEN 'QtyInPO'
						WHEN (sm.Src = 'RCV' AND rcv.SrcTrans = 2) OR sm.Src = 'SR' THEN 'QtyInRtn'
						WHEN sm.Src = 'TS' AND sm.[Type] = 'OH' AND sm.BaseQty > 0 THEN 'QtyInTS'
						WHEN sm.Src = 'CNEE' AND sm.[Type] = 'OH' AND sm.BaseQty > 0 THEN 'QtyInCNEE'
						WHEN sm.Src = 'ADJ' AND sm.BaseQty > 0 THEN 'QtyInADJ'
						WHEN (sm.Src = 'DO' AND do.SrcTrans = 1 AND do.FromDirectInvoice = 0) OR sm.Src = 'DOF' THEN 'QtyOutDO'
						WHEN sm.Src = 'DO' AND do.FromDirectInvoice = 1 THEN 'QtyOutDI'
						WHEN sm.Src = 'PR' OR (sm.Src = 'DO' AND do.SrcTrans = 2 AND do.FromDirectInvoice = 0) THEN 'QtyOutRtn'
						WHEN sm.Src = 'TS' AND sm.[Type] = 'OH' AND sm.BaseQty < 0 THEN 'QtyOutTS'
						WHEN sm.Src = 'CNEE' AND sm.[Type] = 'OH' AND sm.BaseQty < 0 THEN 'QtyOutCNEE'
						WHEN sm.Src = 'ADJ' AND sm.BaseQty < 0 THEN 'QtyOutADJ'
						ELSE 'BB'
					END AS SrcType
				FROM Inventory.StockMutation sm
				LEFT JOIN Inventory.Item im on im.Id = sm.ItemId
				LEFT JOIN Sales.SalesDeliveryHeader do ON do.Code = sm.RefCode1
				LEFT JOIN Sales.SalesReturnHeader sr ON sr.Code = do.TransCode
				LEFT JOIN Purchasing.PurchaseReceiveHeader rcv ON rcv.Code = sm.RefCode1
				WHERE sm.Src IN ('RCV','DO','TS','ADJ','BB','PR','CNEE','DOF', 'SR', 'BB') AND sm.[Type] = 'OH' " + 
                                                           (string.IsNullOrEmpty(whCode) ? "" : $"AND sm.WarehouseCode = '{whCode.Replace("'", "''")}'") +
                                                           orderQuery).ToList();

        var itemData = _db.ReportByItems.FromSqlRaw(qsetUnit +
                                                    @"SELECT im.Id, im.Initial, im.[Name], ic.Initial AS CategoryInitial,
					CASE 
					WHEN @Unit = 1 THEN (SELECT UnitEquivalent FROM Inventory.UoMConversion WHERE UomId = im.UomId AND IsBaseUnit = 1)
					WHEN @Unit = 2 THEN (SELECT UnitEquivalent FROM Inventory.UoMConversion WHERE Id = im.UomBuyId)
					WHEN @Unit = 3 THEN (SELECT UnitEquivalent FROM Inventory.UoMConversion WHERE Id = im.UomSellId)
					END AS Unit,
					CAST (0 AS decimal(18,6)) AS QtyBegin,
					CAST (0 AS decimal(18,6)) AS QtyIn,
					CAST (0 AS decimal(18,6)) AS QtyOut,
					CAST (0 AS decimal(18,6)) AS QtyEnd,
					CAST (0 AS decimal(19,6)) AS InvBegin,
					CAST (0 AS decimal(19,6)) AS InvIn,
					CAST (0 AS decimal(19,6)) AS InvOut,
					CAST (0 AS decimal(19,6)) AS InvEnd
				FROM Inventory.Item im
				LEFT JOIN Inventory.ItemCategory ic ON im.CategoryId = ic.Id " +
                                                    (string.IsNullOrEmpty(whCode) ? "WHERE im.Id IN (SELECT ItemId FROM Inventory.WarehouseQuantity)" : $"WHERE im.Id IN (SELECT itemId FROM Inventory.WarehouseQuantity WHERE WarehouseCode = '{whCode}')")).ToList();

        var whData = _db.ReportByWarehouses.FromSqlRaw(@"SELECT wh.Initial, wh.Code, wh.Name,
							CAST (0 AS decimal(18,6)) AS QtyBegin,
							CAST (0 AS decimal(18,6)) AS QtyIn,
							CAST (0 AS decimal(18,6)) AS QtyOut,
							CAST (0 AS decimal(18,6)) AS QtyEnd,
							CAST (0 AS decimal(19,6)) AS InvBegin,
							CAST (0 AS decimal(19,6)) AS InvIn,
							CAST (0 AS decimal(19,6)) AS InvOut,
							CAST (0 AS decimal(19,6)) AS InvEnd
						FROM Inventory.Warehouse wh").ToList();

        smData = RemoveVoidSM(smData);

        var typeItemData = _db.ReportByTypeSMs.FromSqlRaw(qsetUnit +
                                                          @"SELECT im.Id, im.Initial, im.[Name], ic.Initial AS CategoryInitial,
				CASE 
					WHEN @Unit = 1 THEN (SELECT UnitEquivalent FROM Inventory.UoMConversion WHERE UomId = im.UomId AND IsBaseUnit = 1)
					WHEN @Unit = 2 THEN (SELECT UnitEquivalent FROM Inventory.UoMConversion WHERE Id = im.UomBuyId)
					WHEN @Unit = 3 THEN (SELECT UnitEquivalent FROM Inventory.UoMConversion WHERE Id = im.UomSellId)
				END AS Unit,
				CAST (0 AS decimal(18,6)) AS QtyBegin,
				CAST (0 AS decimal(18,6)) AS QtyInPO,
				CAST (0 AS decimal(18,6)) AS QtyInRtn,
				CAST (0 AS decimal(18,6)) AS QtyInTS,
				CAST (0 AS decimal(18,6)) AS QtyInCNEE,
				CAST (0 AS decimal(18,6)) AS QtyInADJ,
				CAST (0 AS decimal(18,6)) AS QtyOutDO,
				CAST (0 AS decimal(18,6)) AS QtyOutDI,
				CAST (0 AS decimal(18,6)) AS QtyOutRtn,
				CAST (0 AS decimal(18,6)) AS QtyOutTS,
				CAST (0 AS decimal(18,6)) AS QtyOutCNEE,
				CAST (0 AS decimal(18,6)) AS QtyOutADJ,
				CAST (0 AS decimal(18,6)) AS QtyEnd,
				CAST (0 AS decimal(19,6)) AS InvBegin,
				CAST (0 AS decimal(19,6)) AS InvInPO,
				CAST (0 AS decimal(19,6)) AS InvInRtn,
				CAST (0 AS decimal(19,6)) AS InvInTS,
				CAST (0 AS decimal(19,6)) AS InvInCNEE,
				CAST (0 AS decimal(19,6)) AS InvInADJ,
				CAST (0 AS decimal(19,6)) AS InvOutDO,
				CAST (0 AS decimal(19,6)) AS InvOutDI,
				CAST (0 AS decimal(19,6)) AS InvOutRtn,
				CAST (0 AS decimal(19,6)) AS InvOutTS,
				CAST (0 AS decimal(19,6)) AS InvOutCNEE,
				CAST (0 AS decimal(19,6)) AS InvOutADJ,
				CAST (0 AS decimal(19,6)) AS InvEnd
				FROM Inventory.Item im
				LEFT JOIN Inventory.ItemCategory ic ON im.CategoryId = ic.Id " +
                                                          (string.IsNullOrEmpty(whCode) ? "WHERE im.Id IN (SELECT ItemId FROM Inventory.WarehouseQuantity)" : $"WHERE im.Id IN (SELECT itemId FROM Inventory.WarehouseQuantity WHERE WarehouseCode = '{whCode}')")).ToList();

        foreach (var itemSmData in smData)
        {
            //var taxAmount = 0m;
            //if (itemSmData.SrcTrans == "Penjualan Langsung" || itemSmData.SrcTrans == "Surat Jalan")
            //{
            //	var data = _db.SalesDeliveryDetails.FirstOrDefault(x => x.Code == itemSmData.TransCode && x.ItemId == itemSmData.ItemId);
            //	taxAmount = data?.TaxAmount ?? 0m;
            //}
            //else if (itemSmData.SrcTrans == "Penerimaan")
            //{
            //    var data = _db.PurchaseReceiveDetails.FirstOrDefault(x => x.Code == itemSmData.TransCode && x.ItemId == itemSmData.ItemId);
            //    taxAmount = data?.TaxAmount ?? 0m;
            //}
            //else if (itemSmData.SrcTrans == "Retur Pembelian")
            //{
            //	var data = _db.PurchaseReturnDetails.FirstOrDefault(x => x.Code == itemSmData.TransCode && x.ItemId == itemSmData.ItemId);
            //	taxAmount = data?.TaxAmount ?? 0m;
            //}
            //else if (itemSmData.SrcTrans == "Retur Penjualan")
            //{
            //	var data = _db.SalesReturnDetails.FirstOrDefault(x => x.Code == itemSmData.TransCode && x.ItemId == itemSmData.ItemId);
            //	taxAmount = data?.TaxAmount ?? 0m;
            //}

            itemSmData.InvIn = (itemSmData.QtyIn * itemSmData.HPP);
            itemSmData.InvOut = (itemSmData.QtyOut * itemSmData.HPP);
        }

        var initData = smData.Where(x => x.Date < Convert.ToDateTime(startDate)).ToList();

        if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
        {
            smData = smData.Where(x => x.Date >= Convert.ToDateTime(startDate) && x.Date <= Convert.ToDateTime(endDate)).ToList();
        }
        else if (!string.IsNullOrEmpty(startDate)) 
        {
            smData = smData.Where(x => x.Date >= Convert.ToDateTime(startDate)).ToList();
        }

        if (itemId.HasValue)
        {
            itemData = itemData.Where(x => x.Id == itemId).ToList();
            initData = initData.Where(x => x.ItemId == itemId).ToList();
        }

        if (type == 3)
        {
            foreach (var item in typeItemData)
            {
                var qtyIn = smData.Where(x => x.ItemId == item.Id).Sum(x => x.QtyIn);
                var qtyOut = smData.Where(x => x.ItemId == item.Id).Sum(x => x.QtyOut);
                item.QtyBegin = initData.Where(x => x.ItemId == item.Id).Sum(x => x.QtyIn) - initData.Where(x => x.ItemId == item.Id).Sum(x => x.QtyOut);
                item.QtyInPO = smData.Where(x => x.ItemId == item.Id && x.SrcType == "QtyInPO").Sum(x => x.QtyIn);
                item.QtyInRtn = smData.Where(x => x.ItemId == item.Id && x.SrcType == "QtyInRtn").Sum(x => x.QtyIn);
                item.QtyInTS = smData.Where(x => x.ItemId == item.Id && x.SrcType == "QtyInTS").Sum(x => x.QtyIn);
                item.QtyInCNEE = smData.Where(x => x.ItemId == item.Id && x.SrcType == "QtyInCNEE").Sum(x => x.QtyIn);
                item.QtyInADJ = smData.Where(x => x.ItemId == item.Id && x.SrcType == "QtyInADJ").Sum(x => x.QtyIn);
                item.QtyOutDO = smData.Where(x => x.ItemId == item.Id && x.SrcType == "QtyOutDO").Sum(x => x.QtyOut);
                item.QtyOutDI = smData.Where(x => x.ItemId == item.Id && x.SrcType == "QtyOutDI").Sum(x => x.QtyOut);
                item.QtyOutRtn = smData.Where(x => x.ItemId == item.Id && x.SrcType == "QtyOutRtn").Sum(x => x.QtyOut);
                item.QtyOutTS = smData.Where(x => x.ItemId == item.Id && x.SrcType == "QtyOutTS").Sum(x => x.QtyOut);
                item.QtyOutCNEE = smData.Where(x => x.ItemId == item.Id && x.SrcType == "QtyOutCNEE").Sum(x => x.QtyOut);
                item.QtyOutADJ = smData.Where(x => x.ItemId == item.Id && x.SrcType == "QtyOutADJ").Sum(x => x.QtyOut);
                item.QtyEnd = item.QtyBegin + (qtyIn - qtyOut);
                item.InvBegin = initData.Where(x => x.ItemId == item.Id).Sum(x => x.InvIn) - initData.Where(x => x.ItemId == item.Id).Sum(x => x.InvOut);


                var selectedItem = item;
                var smItemData = smData.Where(x => x.ItemId == item.Id);

                smListData.Add(new ReportByStockMutation
                {
                    TransCode = "Nilai Awal",
                    QtyEnd = selectedItem.QtyBegin,
                    InvEnd = selectedItem.InvBegin,
                    IsBold = true
                });

                foreach (var smItem in smItemData)
                {
                    smItem.QtyEnd = smItem.QtyIn > 0 ? selectedItem.QtyBegin + smItem.QtyIn : selectedItem.QtyBegin - smItem.QtyOut;
                    smItem.InvEnd = (selectedItem.InvBegin + smItem.InvIn) - smItem.InvOut;
                    selectedItem.QtyBegin = smItem.QtyEnd;
                    selectedItem.InvBegin = smItem.InvEnd;

                    smListData.Add(smItem);
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
                    InvEnd = smListData.First(x => x.TransCode == "Nilai Awal").InvEnd + smListData.Where(x => !x.IsBold).Sum(x => x.InvIn) - smListData.Where(x => !x.IsBold).Sum(x => x.InvOut),
                    IsBold = true
                });

                var invIn = smListData.Where(x => x.ItemId == item.Id && x.WarehouseCode != null).Sum(x => x.InvIn);
                var invOut = smListData.Where(x => x.ItemId == item.Id && x.WarehouseCode != null).Sum(x => x.InvOut);
                item.QtyBegin = initData.Where(x => x.ItemId == item.Id).Sum(x => x.QtyIn) - initData.Where(x => x.ItemId == item.Id).Sum(x => x.QtyOut);
                item.InvBegin = initData.Where(x => x.ItemId == item.Id).Sum(x => x.InvIn) - initData.Where(x => x.ItemId == item.Id).Sum(x => x.InvOut);
                item.InvInPO = smListData.Where(x => x.ItemId == item.Id && x.SrcType == "QtyInPO").Sum(x => x.InvIn);
                item.InvInRtn = smListData.Where(x => x.ItemId == item.Id && x.SrcType == "QtyInRtn").Sum(x => x.InvIn);
                item.InvInTS = smListData.Where(x => x.ItemId == item.Id && x.SrcType == "QtyInTS").Sum(x => x.InvIn);
                item.InvInCNEE = smListData.Where(x => x.ItemId == item.Id && x.SrcType == "QtyInCNEE").Sum(x => x.InvIn);
                item.InvInADJ = smListData.Where(x => x.ItemId == item.Id && x.SrcType == "QtyInADJ").Sum(x => x.InvIn);
                item.InvOutDO = smListData.Where(x => x.ItemId == item.Id && x.SrcType == "QtyOutDO").Sum(x => x.InvOut);
                item.InvOutDI = smListData.Where(x => x.ItemId == item.Id && x.SrcType == "QtyOutDI").Sum(x => x.InvOut);
                item.InvOutRtn = smListData.Where(x => x.ItemId == item.Id && x.SrcType == "QtyOutRtn").Sum(x => x.InvOut);
                item.InvOutTS = smListData.Where(x => x.ItemId == item.Id && x.SrcType == "QtyOutTS").Sum(x => x.InvOut);
                item.InvOutCNEE = smListData.Where(x => x.ItemId == item.Id && x.SrcType == "QtyOutCNEE").Sum(x => x.InvOut);
                item.InvOutADJ = smListData.Where(x => x.ItemId == item.Id && x.SrcType == "QtyOutADJ").Sum(x => x.InvOut);
                item.InvEnd = item.InvBegin + (invIn - invOut);
            }

            return typeItemData.AsQueryable().ToDataSourceResult(0, typeItemData.Count(), null, null);
        }
        else
        {
            foreach (var item in itemData)
            {
                item.QtyBegin = initData.Where(x => x.ItemId == item.Id).Sum(x => x.QtyIn) - initData.Where(x => x.ItemId == item.Id).Sum(x => x.QtyOut);
                item.QtyIn = smData.Where(x => x.ItemId == item.Id).Sum(x => x.QtyIn);
                item.QtyOut = smData.Where(x => x.ItemId == item.Id).Sum(x => x.QtyOut);
                item.QtyEnd = item.QtyBegin + (item.QtyIn - item.QtyOut);
                item.InvBegin = initData.Where(x => x.ItemId == item.Id).Sum(x => x.InvIn) - initData.Where(x => x.ItemId == item.Id).Sum(x => x.InvOut);


                var selectedItem = item;
                var smItemData = smData.Where(x => x.ItemId == item.Id);

                smListData.Add(new ReportByStockMutation
                {
                    TransCode = "Nilai Awal",
                    QtyEnd = selectedItem.QtyBegin,
                    InvEnd = selectedItem.InvBegin,
                    IsBold = true
                });

                foreach (var smItem in smItemData)
                {
                    smItem.QtyEnd = smItem.QtyIn > 0 ? selectedItem.QtyBegin + smItem.QtyIn : selectedItem.QtyBegin - smItem.QtyOut;
                    smItem.InvEnd = (selectedItem.InvBegin + smItem.InvIn) - smItem.InvOut;
                    selectedItem.QtyBegin = smItem.QtyEnd;
                    selectedItem.InvBegin = smItem.InvEnd;

                    smListData.Add(smItem);
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
                    InvEnd = smListData.First(x => x.TransCode == "Nilai Awal").InvEnd + smListData.Where(x => !x.IsBold).Sum(x => x.InvIn) - smListData.Where(x => !x.IsBold).Sum(x => x.InvOut),
                    IsBold = true
                });

                item.QtyBegin = initData.Where(x => x.ItemId == item.Id).Sum(x => x.QtyIn) - initData.Where(x => x.ItemId == item.Id).Sum(x => x.QtyOut);
                item.QtyIn = smData.Where(x => x.ItemId == item.Id).Sum(x => x.QtyIn);
                item.QtyOut = smData.Where(x => x.ItemId == item.Id).Sum(x => x.QtyOut);
                item.QtyEnd = item.QtyBegin + (item.QtyIn - item.QtyOut);
                item.InvBegin = initData.Where(x => x.ItemId == item.Id).Sum(x => x.InvIn) - initData.Where(x => x.ItemId == item.Id).Sum(x => x.InvOut);
                item.InvIn = smListData.Where(x => x.ItemId == item.Id && x.WarehouseCode != null).Sum(x => x.InvIn);
                item.InvOut = smListData.Where(x => x.ItemId == item.Id && x.WarehouseCode != null).Sum(x => x.InvOut);
                item.InvEnd = item.InvBegin + (item.InvIn - item.InvOut);
            }

            foreach (var wh in whData)
            {
                wh.QtyBegin = (initData.Where(x => x.WarehouseCode == wh.Code).Sum(x => x.QtyIn) - initData.Where(x => x.WarehouseCode == wh.Code).Sum(x => x.QtyOut));
                wh.QtyIn = smData.Where(x => x.WarehouseCode == wh.Code).Sum(x => x.QtyIn);
                wh.QtyOut = smData.Where(x => x.WarehouseCode == wh.Code).Sum(x => x.QtyOut);
                wh.QtyEnd = wh.QtyBegin + (wh.QtyIn - wh.QtyOut);
                wh.InvBegin = (initData.Where(x => x.WarehouseCode == wh.Code).Sum(x => x.QtyIn * x.HPP) - initData.Where(x => x.WarehouseCode == wh.Code).Sum(x => x.QtyOut * x.HPP));
                wh.InvIn = smListData.Where(x => x.WarehouseCode == wh.Code).Sum(x => x.InvIn);
                wh.InvOut = smListData.Where(x => x.WarehouseCode == wh.Code).Sum(x => x.InvOut);
                wh.InvEnd = wh.InvBegin + (wh.InvIn - wh.InvOut);
            }

            if (type == 1)
            {
                if (isSM)
                {
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
        listVoid.AddRange(_db.SalesReturnHeaders.Where(x => x.Mark == "V").Select(x => x.Code).ToList());

        result = result.Where(x => !listVoid.Contains(x.TransCode)).ToList();

        return result;
    }
}