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
    public DataSourceResult GetData(int type, string startDate, string endDate, string whCode, int? itemId,
	    int typeUnit, bool isSM)
    {
		string whBeginDate, whStartDate = "";
        if (!string.IsNullOrEmpty(startDate))
        {
	        whBeginDate = $"WHERE [Date] < '{startDate.Replace("'", "''")}'";
	        whStartDate = $"AND [Date] >= '{startDate.Replace("'", "''")}'";
        }
        else
        {
	        whBeginDate = "WHERE [Date] < '1900-01-01'";
        }
        
        string whEndDate = "", whSmEndDate = "";
        if (!string.IsNullOrEmpty(endDate))
        {
	        whEndDate = $"AND [Date] <= '{endDate.Replace("'", "''")}'";
	        whSmEndDate = $"AND sm.[Date] <= '{endDate.Replace("'", "''")}'";
        }

        string whWarehouse = "", whSmWarehouse = "";
        if (!string.IsNullOrEmpty(whCode))
        {
	        whWarehouse = $"AND WarehouseCode = '{whCode.Replace("'", "''")}'";
	        whSmWarehouse = $"AND sm.WarehouseCode = '{whCode.Replace("'", "''")}'";
        }
        
        string whItem = "", whSmItem = "";
        if (itemId.HasValue)
        {
	        whItem = $"AND ItemId = {itemId}";
	        whSmItem = $"AND sm.ItemId = {itemId}";
        }

        var baseQuery = @$"DECLARE @Unit int; SET @Unit = {typeUnit};
			WITH cte_sm_src AS (
				SELECT sm.WarehouseCode, sm.ItemId, sm.[Date], sm.BaseQty,
					sm.RefCode1 AS TransCode, sm.[Type], sm.Src,
					CAST(CASE
						WHEN sm.Src = 'ADJ' AND sm.BaseQty > 0 THEN 
							CASE 
							WHEN @Unit = 1 THEN ABS(sm.BaseQty) 
							WHEN @Unit = 2 THEN ABS(sm.BaseQty) / ( SELECT EXP(SUM(LOG(Conversion))) FROM Inventory.UoMConversion WHERE UomId = i.UomId AND Seq <= (SELECT Seq FROM Inventory.UoMConversion WHERE Id = i.UomBuyId))
							WHEN @Unit = 3 THEN ABS(sm.BaseQty) / ( SELECT EXP(SUM(LOG(Conversion))) FROM Inventory.UoMConversion WHERE UomId = i.UomId AND Seq <= (SELECT Seq FROM Inventory.UoMConversion WHERE Id = i.UomSellId))
							END
						WHEN sm.Src IN ('RCV', 'BB', 'SR') THEN 
							CASE 
							WHEN @Unit = 1 THEN ABS(sm.BaseQty) 
							WHEN @Unit = 2 THEN ABS(sm.BaseQty) / ( SELECT EXP(SUM(LOG(Conversion))) FROM Inventory.UoMConversion WHERE UomId = i.UomId AND Seq <= (SELECT Seq FROM Inventory.UoMConversion WHERE Id = i.UomBuyId))
							WHEN @Unit = 3 THEN ABS(sm.BaseQty) / ( SELECT EXP(SUM(LOG(Conversion))) FROM Inventory.UoMConversion WHERE UomId = i.UomId AND Seq <= (SELECT Seq FROM Inventory.UoMConversion WHERE Id = i.UomSellId))
							END
						WHEN sm.Src IN ('TS', 'CNEE') AND sm.[Type] = 'OH' AND sm.BaseQty > 0 THEN 
							CASE 
							WHEN @Unit = 1 THEN ABS(sm.BaseQty) 
							WHEN @Unit = 2 THEN ABS(sm.BaseQty) / ( SELECT EXP(SUM(LOG(Conversion))) FROM Inventory.UoMConversion WHERE UomId = i.UomId AND Seq <= (SELECT Seq FROM Inventory.UoMConversion WHERE Id = i.UomBuyId))
							WHEN @Unit = 3 THEN ABS(sm.BaseQty) / ( SELECT EXP(SUM(LOG(Conversion))) FROM Inventory.UoMConversion WHERE UomId = i.UomId AND Seq <= (SELECT Seq FROM Inventory.UoMConversion WHERE Id = i.UomSellId))
							END
						ELSE 0
						END AS decimal(30,15)) AS QtyIn,
					CAST(CASE
						WHEN sm.Src = 'ADJ' AND sm.BaseQty < 0 THEN 
							CASE 
							WHEN @Unit = 1 THEN ABS(sm.BaseQty) 
							WHEN @Unit = 2 THEN ABS(sm.BaseQty) / ( SELECT EXP(SUM(LOG(Conversion))) FROM Inventory.UoMConversion WHERE UomId = i.UomId AND Seq <= (SELECT Seq FROM Inventory.UoMConversion WHERE Id = i.UomBuyId))
							WHEN @Unit = 3 THEN ABS(sm.BaseQty) / ( SELECT EXP(SUM(LOG(Conversion))) FROM Inventory.UoMConversion WHERE UomId = i.UomId AND Seq <= (SELECT Seq FROM Inventory.UoMConversion WHERE Id = i.UomSellId))
							END
						WHEN sm.Src IN ('DO', 'DOF', 'PR') THEN 
							CASE 
							WHEN @Unit = 1 THEN ABS(sm.BaseQty) 
							WHEN @Unit = 2 THEN ABS(sm.BaseQty) / ( SELECT EXP(SUM(LOG(Conversion))) FROM Inventory.UoMConversion WHERE UomId = i.UomId AND Seq <= (SELECT Seq FROM Inventory.UoMConversion WHERE Id = i.UomBuyId))
							WHEN @Unit = 3 THEN ABS(sm.BaseQty) / ( SELECT EXP(SUM(LOG(Conversion))) FROM Inventory.UoMConversion WHERE UomId = i.UomId AND Seq <= (SELECT Seq FROM Inventory.UoMConversion WHERE Id = i.UomSellId))
							END
						WHEN sm.Src IN ('TS', 'CNEE') AND sm.[Type] = 'OH' AND sm.BaseQty < 0 THEN 
							CASE 
							WHEN @Unit = 1 THEN ABS(sm.BaseQty) 
							WHEN @Unit = 2 THEN ABS(sm.BaseQty) / ( SELECT EXP(SUM(LOG(Conversion))) FROM Inventory.UoMConversion WHERE UomId = i.UomId AND Seq <= (SELECT Seq FROM Inventory.UoMConversion WHERE Id = i.UomBuyId))
							WHEN @Unit = 3 THEN ABS(sm.BaseQty) / ( SELECT EXP(SUM(LOG(Conversion))) FROM Inventory.UoMConversion WHERE UomId = i.UomId AND Seq <= (SELECT Seq FROM Inventory.UoMConversion WHERE Id = i.UomSellId))
							END
						ELSE 0
						END AS decimal(30,15)) AS QtyOut,
					CAST(0 AS decimal(30,15)) AS QtyEnd,
					CAST(CASE 
						WHEN @Unit = 1 THEN ABS(sm.BaseNettPrice) 
						WHEN @Unit = 2 THEN ABS(sm.BaseNettPrice) * ( SELECT EXP(SUM(LOG(Conversion))) FROM Inventory.UoMConversion WHERE UomId = i.UomId AND Seq <= (SELECT Seq FROM Inventory.UoMConversion WHERE Id = i.UomBuyId))
						WHEN @Unit = 3 THEN ABS(sm.BaseNettPrice) * ( SELECT EXP(SUM(LOG(Conversion))) FROM Inventory.UoMConversion WHERE UomId = i.UomId AND Seq <= (SELECT Seq FROM Inventory.UoMConversion WHERE Id = i.UomSellId))
						END AS decimal(30,15)) AS HPP
				FROM Inventory.StockMutation sm
				LEFT JOIN Inventory.Item i 
					ON i.Id = sm.ItemId
				WHERE sm.Src IN ('RCV', 'DO', 'TS', 'ADJ', 'BB', 'PR', 'CNEE', 'DOF', 'SR')
				AND sm.[Type] = 'OH'
				AND sm.RefCode1 NOT IN (
	                SELECT Code FROM Purchasing.PurchaseReceiveHeader WHERE Mark = 'V' {whEndDate}
	                UNION
	                SELECT Code FROM Sales.SalesDeliveryHeader WHERE Mark = 'V' {whEndDate}
	                UNION
	                SELECT Code FROM Inventory.AdjustmentHeader WHERE Mark = 'V' {whEndDate}
	                UNION
	                SELECT Code FROM Inventory.TransferStockHeader WHERE Mark = 'V' {whEndDate}
	                UNION
	                SELECT Code FROM Purchasing.PurchaseReturnHeader WHERE Mark = 'V' {whEndDate}
	                UNION
	                SELECT Code FROM Sales.SalesReturnHeader WHERE Mark = 'V' {whEndDate}
	            )
				{whSmEndDate} {whSmWarehouse} {whSmItem}
			), cte_sm_all AS (
				SELECT sm.*,
					QtyIn * HPP AS InvIn,
					QtyOut * HPP AS InvOut,
					CAST(0 AS decimal(30,15)) AS InvEnd,
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
					CAST(0 AS bit) AS IsBold,
					CASE 
						WHEN sm.Src = 'RCV' AND rcv.SrcTrans = 1 THEN 'QtyInPO'
						WHEN (sm.Src = 'RCV' AND rcv.SrcTrans = 2) OR sm.Src = 'SR' THEN 'QtyInRtn'
						WHEN sm.Src = 'TS' AND sm.[Type] = 'OH' AND sm.BaseQty > 0 THEN 'QtyInTS'
						WHEN sm.Src = 'CNEE' AND sm.[Type] = 'OH' AND sm.BaseQty > 0 THEN 'QtyInCNEE'
						WHEN sm.Src = 'ADJ' AND sm.BaseQty > 0 THEN 'QtyInADJ'
						WHEN (sm.Src = 'DO' AND do.SrcTrans = 1 AND do.FromDirectInvoice = 0) OR (sm.Src = 'DOF' AND do.FromDirectInvoice = 0) THEN 'QtyOutDO'
						WHEN (sm.Src = 'DO' AND do.FromDirectInvoice = 1) OR (sm.Src = 'DOF' AND do.FromDirectInvoice = 1) THEN 'QtyOutDI'
						WHEN sm.Src = 'PR' OR (sm.Src = 'DO' AND do.SrcTrans = 2 AND do.FromDirectInvoice = 0) THEN 'QtyOutRtn'
						WHEN sm.Src = 'TS' AND sm.[Type] = 'OH' AND sm.BaseQty < 0 THEN 'QtyOutTS'
						WHEN sm.Src = 'CNEE' AND sm.[Type] = 'OH' AND sm.BaseQty < 0 THEN 'QtyOutCNEE'
						WHEN sm.Src = 'ADJ' AND sm.BaseQty < 0 THEN 'QtyOutADJ'
						ELSE 'BB'
						END AS SrcType,
					CASE
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
						END AS Seq
				FROM cte_sm_src sm
				LEFT JOIN Sales.SalesDeliveryHeader do
					ON do.Code = sm.TransCode
				LEFT JOIN Sales.SalesReturnHeader sr
					ON sr.Code = do.TransCode
				LEFT JOIN Purchasing.PurchaseReceiveHeader rcv
					ON rcv.Code = sm.TransCode
			)";

        if (type == 1)
        {
	        var beginQuery = @$"
				, cte_begin_sm_src AS (
					SELECT ItemId,
						CAST(SUM(QtyIn - QtyOut) AS decimal(30,15)) AS QtyBegin,
						CAST(SUM(InvIn - InvOut) AS decimal(30,15)) AS InvBegin
					FROM cte_sm_all
					{whBeginDate}
					GROUP BY ItemId
				)";

	        if (!isSM)
	        {
		        var data = _db.ReportByItems.FromSqlRaw(@$"{baseQuery}
				{beginQuery}
				SELECT i.Id, i.Initial, i.[Name],
					ic.Initial AS CategoryInitial,
					CASE 
						WHEN @Unit = 1 THEN uom_c_bu.UnitEquivalent
						WHEN @Unit = 2 THEN uom_c_b.UnitEquivalent
						WHEN @Unit = 3 THEN uom_c_s.UnitEquivalent
						END AS Unit,
					ISNULL(cte_b_sm.QtyBegin, 0) AS QtyBegin,
					ISNULL(cte_sm.QtyIn, 0) AS QtyIn,
					ISNULL(cte_sm.QtyOut, 0) AS QtyOut,
					ISNULL(cte_b_sm.QtyBegin, 0) + ISNULL(cte_sm.QtyIn, 0) - ISNULL(cte_sm.QtyOut, 0) AS QtyEnd,
					ISNULL(cte_b_sm.InvBegin, 0) AS InvBegin,
					ISNULL(cte_sm.InvIn, 0) AS InvIn,
					ISNULL(cte_sm.InvOut, 0) AS InvOut,
					ISNULL(cte_b_sm.InvBegin, 0) + ISNULL(cte_sm.InvIn, 0) - ISNULL(cte_sm.InvOut, 0) AS InvEnd
				FROM (
					SELECT ItemId,
						SUM(QtyIn) AS QtyIn,
						SUM(QtyOut) AS QtyOut,
						SUM(InvIn) AS InvIn,
						SUM(InvOut) AS InvOut
					FROM cte_sm_all
					WHERE 1=1 {whStartDate}
					GROUP BY ItemId
				) cte_sm
				RIGHT JOIN Inventory.Item i
					ON i.Id = cte_sm.ItemId
				LEFT JOIN cte_begin_sm_src cte_b_sm
					ON cte_b_sm.ItemId = i.Id
				LEFT JOIN Inventory.ItemCategory ic
					ON ic.Id = i.CategoryId
				LEFT JOIN Inventory.UoMConversion uom_c_bu
					ON uom_c_bu.UomId = i.UomId
					AND uom_c_bu.IsBaseUnit = 1
				LEFT JOIN Inventory.UoMConversion uom_c_b
					ON uom_c_b.Id = i.UomBuyId
				LEFT JOIN Inventory.UoMConversion uom_c_s
					ON uom_c_s.Id = i.UomSellId
				WHERE i.Id IN (
					SELECT ItemId
					FROM Inventory.WarehouseQuantity
					WHERE 1=1 {whWarehouse} {whItem}
				)").ToList();

		        return data.AsQueryable().ToDataSourceResult(0, data.Count, null, null);
	        }
	        else
	        {
		        var data = _db.ReportByStockMutations.FromSqlRaw(@$"{baseQuery}
				{beginQuery}
				SELECT NULL AS WarehouseCode, 0 AS ItemId, NULL AS [Date],
					'Nilai Awal' AS TransCode, NULL AS SrcTrans,
					ISNULL(SUM(QtyBegin), 0) AS QtyBegin,
					CAST(0 AS decimal(30,15)) AS QtyIn, CAST(0 AS decimal(30,15)) AS QtyOut,
					ISNULL(SUM(QtyBegin), 0) AS QtyEnd,
					CAST(0 AS decimal(30,15)) AS HPP,
					ISNULL(SUM(InvBegin), 0) AS InvBegin,
					CAST(0 AS decimal(30,15)) AS InvIn, CAST(0 AS decimal(30,15)) AS InvOut,
					ISNULL(SUM(InvBegin), 0) AS InvEnd,
					CAST(1 AS bit) AS IsBold, NULL AS SrcType, NULL AS Seq
				FROM cte_begin_sm_src
				UNION ALL
				SELECT cte_sm.WarehouseCode, cte_sm.ItemId, cte_sm.[Date],
					cte_sm.TransCode, cte_sm.SrcTrans,
					ISNULL(cte_b_sm.QtyBegin, 0) AS QtyBegin,
					cte_sm.QtyIn, cte_sm.QtyOut,
					cte_sm.QtyEnd,
					cte_sm.HPP,
					ISNULL(cte_b_sm.InvBegin, 0) AS InvBegin,
					cte_sm.InvIn, cte_sm.InvOut,
					cte_sm.InvEnd,
					cte_sm.IsBold, cte_sm.SrcType, cte_sm.Seq
				FROM cte_sm_all cte_sm
				LEFT JOIN cte_begin_sm_src cte_b_sm
					ON cte_b_sm.ItemId = cte_sm.ItemId
				WHERE 1=1 {whStartDate}
				ORDER BY [Date], Seq").ToList();

		        decimal balanceQty = 0, balanceInv = 0;
		        bool first = true;
		        foreach (var item in data)
		        {
			        if (first)
			        {
				        balanceQty = item.QtyEnd;
				        balanceInv = item.InvEnd;
				        first = false;
				        continue;
			        }
			        
			        balanceQty += item.QtyIn - item.QtyOut;
				    item.QtyEnd = balanceQty;
				    
				    balanceInv += item.InvIn - item.InvOut;
				    item.InvEnd = balanceInv;
		        }

		        data.Add(new ReportByStockMutation
		        {
			        TransCode = "Total",
			        QtyIn = data.Sum(x => x.QtyIn),
			        QtyOut = data.Sum(x => x.QtyOut),
			        QtyEnd = balanceQty,
			        InvIn = data.Sum(x => x.InvIn),
			        InvOut = data.Sum(x => x.InvOut),
			        InvEnd = balanceInv,
			        IsBold = true
		        });
		        
		        return data.AsQueryable().ToDataSourceResult(0, data.Count, null, null);
	        }
        }

        if (type == 2)
        {
	        var beginQuery = @$"
				, cte_begin_sm_src AS (
					SELECT WarehouseCode,
						CAST(SUM(QtyIn - QtyOut) AS decimal(30,15)) AS QtyBegin,
						CAST(SUM(InvIn - InvOut) AS decimal(30,15)) AS InvBegin
					FROM cte_sm_all
					{whBeginDate}
					GROUP BY WarehouseCode
				)";
	        
	        var data = _db.ReportByWarehouses.FromSqlRaw(@$"{baseQuery}
				{beginQuery}
				SELECT w.Code, w.Initial, w.[Name],
					ISNULL(cte_b_sm.QtyBegin, 0) AS QtyBegin,
					ISNULL(cte_sm.QtyIn, 0) AS QtyIn,
					ISNULL(cte_sm.QtyOut, 0) AS QtyOut,
					ISNULL(cte_b_sm.QtyBegin, 0) + ISNULL(cte_sm.QtyIn, 0) - ISNULL(cte_sm.QtyOut, 0) AS QtyEnd,
					ISNULL(cte_b_sm.InvBegin, 0) AS InvBegin,
					ISNULL(cte_sm.InvIn, 0) AS InvIn,
					ISNULL(cte_sm.InvOut, 0) AS InvOut,
					ISNULL(cte_b_sm.InvBegin, 0) + ISNULL(cte_sm.InvIn, 0) - ISNULL(cte_sm.InvOut, 0) AS InvEnd
				FROM (
					SELECT WarehouseCode,
						SUM(QtyIn) AS QtyIn,
						SUM(QtyOut) AS QtyOut,
						SUM(InvIn) AS InvIn,
						SUM(InvOut) AS InvOut
					FROM cte_sm_all
					WHERE 1=1 {whStartDate}
					GROUP BY WarehouseCode
				) cte_sm
				RIGHT JOIN Inventory.Warehouse w
					ON w.Code = cte_sm.WarehouseCode
				LEFT JOIN cte_begin_sm_src cte_b_sm
					ON cte_b_sm.WarehouseCode = w.Code").ToList();
	        
	        return data.AsQueryable().ToDataSourceResult(0, data.Count, null, null);
        }

        if (type == 3)
        {
	        var beginQuery = @$"
				, cte_begin_sm_src AS (
					SELECT ItemId,
						CAST(SUM(QtyIn - QtyOut) AS decimal(30,15)) AS QtyBegin,
						CAST(SUM(InvIn - InvOut) AS decimal(30,15)) AS InvBegin
					FROM cte_sm_all
					{whBeginDate}
					GROUP BY ItemId
				)";
	        
	        var data = _db.ReportByTypeSMs.FromSqlRaw(@$"{baseQuery}
				{beginQuery}
				SELECT i.Id, i.Initial, i.[Name],
					ic.Initial AS CategoryInitial,
					CASE 
						WHEN @Unit = 1 THEN uom_c_bu.UnitEquivalent
						WHEN @Unit = 2 THEN uom_c_b.UnitEquivalent
						WHEN @Unit = 3 THEN uom_c_s.UnitEquivalent
						END AS Unit,
					ISNULL(cte_b_sm.QtyBegin, 0) AS QtyBegin,
					ISNULL(cte_sm.QtyInPO, 0) AS QtyInPO,
					ISNULL(cte_sm.QtyInRtn, 0) AS QtyInRtn,
					ISNULL(cte_sm.QtyInTS, 0) AS QtyInTS,
					ISNULL(cte_sm.QtyInCNEE, 0) AS QtyInCNEE,
					ISNULL(cte_sm.QtyInADJ, 0) AS QtyInADJ,
					ISNULL(cte_sm.QtyOutDO, 0) AS QtyOutDO,
					ISNULL(cte_sm.QtyOutDI, 0) AS QtyOutDI,
					ISNULL(cte_sm.QtyOutRtn, 0) AS QtyOutRtn,
					ISNULL(cte_sm.QtyOutTS, 0) AS QtyOutTS,
					ISNULL(cte_sm.QtyOutCNEE, 0) AS QtyOutCNEE,
					ISNULL(cte_sm.QtyOutADJ, 0) AS QtyOutADJ,
					ISNULL(cte_b_sm.QtyBegin, 0) + ISNULL(cte_sm.QtyIn, 0) - ISNULL(cte_sm.QtyOut, 0) AS QtyEnd,
					ISNULL(cte_b_sm.InvBegin, 0) AS InvBegin,
					ISNULL(cte_sm.InvInPO, 0) AS InvInPO,
					ISNULL(cte_sm.InvInRtn, 0) AS InvInRtn,
					ISNULL(cte_sm.InvInTS, 0) AS InvInTS,
					ISNULL(cte_sm.InvInCNEE, 0) AS InvInCNEE,
					ISNULL(cte_sm.InvInADJ, 0) AS InvInADJ,
					ISNULL(cte_sm.InvOutDO, 0) AS InvOutDO,
					ISNULL(cte_sm.InvOutDI, 0) AS InvOutDI,
					ISNULL(cte_sm.InvOutRtn, 0) AS InvOutRtn,
					ISNULL(cte_sm.InvOutTS, 0) AS InvOutTS,
					ISNULL(cte_sm.InvOutCNEE, 0) AS InvOutCNEE,
					ISNULL(cte_sm.InvOutADJ, 0) AS InvOutADJ,
					ISNULL(cte_b_sm.InvBegin, 0) + ISNULL(cte_sm.InvIn, 0) - ISNULL(cte_sm.InvOut, 0) AS InvEnd
				FROM (
					SELECT ItemId,
						SUM(QtyIn) AS QtyIn,
						SUM(CASE WHEN SrcType = 'QtyInPO' THEN QtyIn ELSE 0 END) AS QtyInPO,
						SUM(CASE WHEN SrcType = 'QtyInRtn' THEN QtyIn ELSE 0 END) AS QtyInRtn,
						SUM(CASE WHEN SrcType = 'QtyInTS' THEN QtyIn ELSE 0 END) AS QtyInTS,
						SUM(CASE WHEN SrcType = 'QtyInCNEE' THEN QtyIn ELSE 0 END) AS QtyInCNEE,
						SUM(CASE WHEN SrcType = 'QtyInADJ' THEN QtyIn ELSE 0 END) AS QtyInADJ,
						SUM(QtyOut) AS QtyOut,
						SUM(CASE WHEN SrcType = 'QtyOutDO' THEN QtyOut ELSE 0 END) AS QtyOutDO,
						SUM(CASE WHEN SrcType = 'QtyOutDI' THEN QtyOut ELSE 0 END) AS QtyOutDI,
						SUM(CASE WHEN SrcType = 'QtyOutRtn' THEN QtyOut ELSE 0 END) AS QtyOutRtn,
						SUM(CASE WHEN SrcType = 'QtyOutTS' THEN QtyOut ELSE 0 END) AS QtyOutTS,
						SUM(CASE WHEN SrcType = 'QtyOutCNEE' THEN QtyOut ELSE 0 END) AS QtyOutCNEE,
						SUM(CASE WHEN SrcType = 'QtyOutADJ' THEN QtyOut ELSE 0 END) AS QtyOutADJ,
						SUM(InvIn) AS InvIn,
						SUM(CASE WHEN SrcType = 'QtyInPO' THEN InvIn ELSE 0 END) AS InvInPO,
						SUM(CASE WHEN SrcType = 'QtyInRtn' THEN InvIn ELSE 0 END) AS InvInRtn,
						SUM(CASE WHEN SrcType = 'QtyInTS' THEN InvIn ELSE 0 END) AS InvInTS,
						SUM(CASE WHEN SrcType = 'QtyInCNEE' THEN InvIn ELSE 0 END) AS InvInCNEE,
						SUM(CASE WHEN SrcType = 'QtyInADJ' THEN InvIn ELSE 0 END) AS InvInADJ,
						SUM(InvOut) AS InvOut,
						SUM(CASE WHEN SrcType = 'QtyOutDO' THEN InvOut ELSE 0 END) AS InvOutDO,
						SUM(CASE WHEN SrcType = 'QtyOutDI' THEN InvOut ELSE 0 END) AS InvOutDI,
						SUM(CASE WHEN SrcType = 'QtyOutRtn' THEN InvOut ELSE 0 END) AS InvOutRtn,
						SUM(CASE WHEN SrcType = 'QtyOutTS' THEN InvOut ELSE 0 END) AS InvOutTS,
						SUM(CASE WHEN SrcType = 'QtyOutCNEE' THEN InvOut ELSE 0 END) AS InvOutCNEE,
						SUM(CASE WHEN SrcType = 'QtyOutADJ' THEN InvOut ELSE 0 END) AS InvOutADJ
					FROM cte_sm_all
					WHERE 1=1 {whStartDate}
					GROUP BY ItemId
				) cte_sm
				RIGHT JOIN Inventory.Item i
					ON i.Id = cte_sm.ItemId
				LEFT JOIN cte_begin_sm_src cte_b_sm
					ON cte_b_sm.ItemId = i.Id
				LEFT JOIN Inventory.ItemCategory ic
					ON ic.Id = i.CategoryId
				LEFT JOIN Inventory.UoMConversion uom_c_bu
					ON uom_c_bu.UomId = i.UomId
					AND uom_c_bu.IsBaseUnit = 1
				LEFT JOIN Inventory.UoMConversion uom_c_b
					ON uom_c_b.Id = i.UomBuyId
				LEFT JOIN Inventory.UoMConversion uom_c_s
					ON uom_c_s.Id = i.UomSellId
				WHERE i.Id IN (
					SELECT ItemId
					FROM Inventory.WarehouseQuantity
					WHERE 1=1 {whWarehouse} {whItem}
				)").ToList();
	        
	        return data.AsQueryable().ToDataSourceResult(0, data.Count, null, null);
        }

        return new DataSourceResult();
    }
}