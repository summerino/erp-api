using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Purchase;
using Microsoft.EntityFrameworkCore;

namespace ERP.Web.API.Domain.Services.Purchase
{
    public class PurchaseReceiveReportService : IPurchaseReceiveReportService
    {
        private readonly TenantContext _db;
        public PurchaseReceiveReportService(TenantContext db)
        {
            _db = db;
        }
        public DataSourceResult GetData(int type, int? srcTrans, string startDate, string endDate, string supCode, string status, int? itemId, string code, bool isDetail, int? unitId, int? categoryId)
        {
            var rcvData = _db.ReportByRCVs.FromSqlRaw(@"WITH cte_rcv_detail AS (SELECT rcv_d.Code, rcv_d.Qty, rcv_d.UnitPrice,
                            CASE rcv_d.[Type]
	                            WHEN 0 THEN rcv_d.Disc
	                            WHEN 1 THEN rcv_d.UnitPrice END AS Disc,
                            CASE rcv_d.[Type]
	                            WHEN 0 THEN rcv_d.FinalDiscHeader
	                            WHEN 1 THEN CAST(0 AS decimal) END AS FinalDiscHeader,
                            CASE rcv_d.[Type]
	                            WHEN 0 THEN rcv_d.TaxAmount
	                            WHEN 1 THEN CAST(0 AS decimal) END AS TaxAmount,
                            CASE rcv_d.[Type]
	                            WHEN 0 THEN rcv_d.NettPrice
	                            WHEN 1 THEN CAST(0 AS decimal) END AS NettPrice,
                            CASE rcv_d.[Type]
	                            WHEN 0 THEN rcv_d.Total
	                            WHEN 1 THEN CAST(0 AS decimal) END AS Total,
                            CASE rcv_d.[Type]
	                            WHEN 0 THEN rcv_d.DPP
	                            WHEN 1 THEN CAST(0 AS decimal) END AS DPP
                            FROM Purchasing.PurchaseReceiveDetail rcv_d)
                            SELECT rcv.[Date], rcv.Code, CAST(rcv.SrcTrans AS int) AS SrcTrans,
                            rcv.TransCode, rcv.RefNo, rcv.SupCode, rcv.SupName, rcv.TaxInvoiceNo, rcv.TaxInvoiceDate,
                            SUM(rcv_d.Qty * rcv_d.UnitPrice) AS GrossAmount, SUM(rcv_d.Qty * (rcv_d.UnitPrice - rcv_d.Disc - rcv_d.FinalDiscHeader)) AS SubTotal,
                            SUM(rcv_d.Qty * (rcv_d.Disc + rcv_d.FinalDiscHeader)) AS Disc, SUM(rcv_d.Qty * rcv_d.DPP) AS DPP, SUM(rcv_d.Qty * rcv_d.TaxAmount) AS TaxAmount, SUM(rcv_d.Total) AS Total,
                            CASE rcv.Mark
	                            WHEN 'A' THEN 'Aktif'
	                            WHEN 'V' THEN 'Void'
	                            WHEN 'INV' THEN 'Sudah Difakturkan' END AS [Status]
                            FROM Purchasing.vwPurchaseReceiveHeader rcv
                            LEFT JOIN cte_rcv_detail rcv_d ON rcv.Code = rcv_d.Code" +
                            (string.IsNullOrEmpty(status) ? "" : $" WHERE rcv.Mark = '{status.Replace("'", "''")}'") +
                            @" GROUP BY rcv.[Date], rcv.Code, rcv.SrcTrans,
                            rcv.TransCode, rcv.RefNo, rcv.SupCode,
                            rcv.SupName, rcv.TaxInvoiceNo, rcv.TaxInvoiceDate, rcv.Mark").ToList();

            var rcvDetailData = _db.ReportByDetailRCVs.FromSqlRaw(@"SELECT rcv.[Date], rcv.Code, rcv.SupCode, rcv.SupName, CAST(rcv.SrcTrans AS int) AS SrcTrans, rcv.TransCode, rcv.RefNo,
                            im.Initial AS ItemInitial, im.[Name] AS ItemName, wh.[Name] AS WarehouseName, rcv_d.Qty,  rcv_d.UnitId, rcv_d.UnitName,
                            rcv_d.UnitPrice AS GrossAmount,
                            CASE rcv_d.[Type]
	                            WHEN 0 THEN rcv_d.Disc
	                            WHEN 1 THEN rcv_d.UnitPrice END AS Disc,
                            CASE rcv_d.[Type]
	                            WHEN 0 THEN rcv_d.FinalDiscHeader
	                            WHEN 1 THEN CAST(0 AS decimal) END AS DiscHeader,
                            CASE rcv_d.[Type]
	                            WHEN 0 THEN rcv_d.TaxAmount
	                            WHEN 1 THEN CAST(0 AS decimal) END AS TaxAmount,
                            CASE rcv_d.[Type]
	                            WHEN 0 THEN rcv_d.UnitPrice - rcv_d.Disc - rcv_d.FinalDiscHeader
	                            WHEN 1 THEN CAST(0 AS decimal) END AS SubTotal,
                            CASE rcv_d.[Type]
	                            WHEN 0 THEN rcv_d.DPP
	                            WHEN 1 THEN CAST(0 AS decimal) END AS DPP,
                            CASE rcv_d.[Type]
	                            WHEN 0 THEN rcv_d.NettPrice
	                            WHEN 1 THEN CAST(0 AS decimal) END AS NettPrice,
                            rcv_d.UnitPrice * rcv_d.Qty AS TotalGrossAmount,
                            CASE rcv_d.[Type]
	                            WHEN 0 THEN rcv_d.Disc * rcv_d.Qty
	                            WHEN 1 THEN rcv_d.UnitPrice * rcv_d.Qty END AS TotalDisc,
                            CASE rcv_d.[Type]
	                            WHEN 0 THEN rcv_d.FinalDiscHeader * rcv_d.Qty
	                            WHEN 1 THEN CAST(0 AS decimal) END AS TotalDiscHeader,
                            CASE rcv_d.[Type]
	                            WHEN 0 THEN (rcv_d.UnitPrice - rcv_d.Disc - rcv_d.FinalDiscHeader) * rcv_d.Qty
	                            WHEN 1 THEN CAST(0 AS decimal) END AS TotalAfterDisc,
                            CASE rcv_d.[Type]
	                            WHEN 0 THEN rcv_d.DPP * rcv_d.Qty
	                            WHEN 1 THEN CAST(0 AS decimal) END AS TotalDPP,
                            CASE rcv_d.[Type]
	                            WHEN 0 THEN rcv_d.TaxAmount * rcv_d.Qty
	                            WHEN 1 THEN CAST(0 AS decimal) END AS TotalTaxAmount,
                            CASE rcv_d.[Type]
	                            WHEN 0 THEN rcv_d.NettPrice * rcv_d.Qty
	                            WHEN 1 THEN CAST(0 AS decimal) END AS TotalNettPrice,
                            CASE rcv.Mark
	                            WHEN 'A' THEN 'Aktif'
	                            WHEN 'V' THEN 'Void'
	                            WHEN 'INV' THEN 'Sudah Difakturkan' END AS [Status],
                            ic.Id AS CategoryId, ic.Initial AS CategoryInitial, rcv.TaxInvoiceNo, rcv.TaxInvoiceDate
                            FROM Purchasing.vwPurchaseReceiveDetail rcv_d
                            LEFT JOIN Purchasing.vwPurchaseReceiveHeader rcv on rcv.Code = rcv_d.Code
                            LEFT JOIN Inventory.Item im on im.Id = rcv_d.ItemId
                            LEFT JOIN Inventory.Warehouse wh on wh.Code = rcv_d.WarehouseCode
                            LEFT JOIN Inventory.ItemCategory ic ON ic.Id = im.CategoryId" +
                            (!itemId.HasValue || itemId <= 0 ? "" : $" WHERE rcv_d.ItemId = {itemId}")).ToList();

            var itemData = _db.ReportByItemPurchases.FromSqlRaw(@"SELECT im.Initial, im.[Name], 
                            ic.Id AS CategoryId, ic.Initial AS CategoryInitial,
                            uc.Id AS UnitId, uc.UnitEquivalent AS UnitName,
                            CAST (0 AS int) AS TotalTrans, CAST (0 AS decimal) AS Qty, CAST (0 AS decimal) AS SubTotal,
                            CAST (0 AS decimal) AS Disc, CAST (0 AS decimal) AS DiscHeader, CAST (0 AS decimal) AS Dpp,
                            CAST (0 AS decimal) AS TaxAmount, CAST (0 AS decimal) AS Total, CAST (0 AS decimal) AS GrossAmount
                            FROM Inventory.Item im
                            LEFT JOIN Purchasing.PurchaseReceiveDetail rcv_d ON rcv_d.ItemId = im.Id
                            LEFT JOIN Inventory.ItemCategory ic ON ic.Id = im.CategoryId
                            LEFT JOIN Inventory.UoMConversion uc ON uc.Id = rcv_d.UnitId
                            WHERE uc.Id IS NOT NULL
                            GROUP BY im.Initial, im.[Name], ic.Id, ic.Initial, uc.Id, uc.UnitEquivalent").ToList();

            var supData = _db.ReportBySupplierPurchases.FromSqlRaw(@"SELECT sp.Code, sp.[Name], CAST (0 AS int) AS TotalTrans,
                            CAST (0 AS decimal) AS SubTotal, CAST (0 AS decimal) AS GrossAmount, 
                            CAST (0 AS decimal) AS Disc, CAST (0 AS decimal) AS DiscHeader,
                            CAST (0 AS decimal) AS Dpp, CAST (0 AS decimal) AS TaxAmount, CAST (0 AS decimal) AS Total
                            FROM General.Supplier sp
                            WHERE sp.IsActive = 1
                            GROUP BY sp.Code, sp.[Name]").ToList();

            var itemCategoryData = _db.ReportByItemCategoryPurchases.FromSqlRaw(@"SELECT ic.Id AS CategoryId, ic.Initial, ic.[Name],
                            uc.Id AS UnitId, uc.UnitEquivalent AS UnitName,
                            CAST (0 AS int) AS TotalTrans, CAST (0 AS decimal) AS Qty, CAST (0 AS decimal) AS SubTotal,
                            CAST (0 AS decimal) AS Disc, CAST (0 AS decimal) AS DiscHeader, CAST (0 AS decimal) AS Dpp,
                            CAST (0 AS decimal) AS TaxAmount, CAST (0 AS decimal) AS Total, CAST (0 AS decimal) AS GrossAmount
                            FROM Inventory.ItemCategory ic
                            LEFT JOIN Inventory.Item im ON im.CategoryId = ic.Id 
                            LEFT JOIN Purchasing.PurchaseReceiveDetail rcv_d ON rcv_d.ItemId = im.Id
                            LEFT JOIN Inventory.UoMConversion uc ON uc.Id = rcv_d.UnitId
                            WHERE uc.Id IS NOT NULL" +
                            (!categoryId.HasValue || categoryId <= 0 ? "" : $" AND ic.Id = {categoryId}") +
                            " GROUP BY ic.Id, ic.Initial, ic.[Name], uc.Id, uc.UnitEquivalent").ToList();

            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                rcvData = rcvData.Where(x => x.Date >= Convert.ToDateTime(startDate) && x.Date <= Convert.ToDateTime(endDate)).ToList();
                rcvDetailData = rcvDetailData.Where(x => x.Date >= Convert.ToDateTime(startDate) && x.Date <= Convert.ToDateTime(endDate)).ToList();
            }
            else if (!string.IsNullOrEmpty(startDate))
            {
                rcvData = rcvData.Where(x => x.Date >= Convert.ToDateTime(startDate)).ToList();
                rcvDetailData = rcvDetailData.Where(x => x.Date >= Convert.ToDateTime(startDate)).ToList();
            }
            else if (!string.IsNullOrEmpty(endDate))
            {
                rcvData = rcvData.Where(x => x.Date <= Convert.ToDateTime(endDate)).ToList();
                rcvDetailData = rcvDetailData.Where(x => x.Date <= Convert.ToDateTime(endDate)).ToList();
            }

            if (srcTrans.HasValue || srcTrans > 0)
            {
                if (srcTrans.Value == 1 || srcTrans.Value == 2)
                {
                    rcvData = rcvData.Where(x => x.SrcTrans == srcTrans.Value).ToList();
                    rcvDetailData = rcvDetailData.Where(x => x.SrcTrans == srcTrans.Value).ToList();
                }
            }

            if (!isDetail)
            {
                if (type == 1)
                {
                    if (categoryId.HasValue || categoryId > 0)
                    {
                        rcvDetailData = rcvDetailData.Where(x => itemCategoryData.Select(y => y.CategoryId).Contains(x.CategoryId)).ToList();
                        if (!itemId.HasValue || itemId <= 0)
                            rcvData = rcvData.Where(x => rcvDetailData.Select(y => y.Code).Contains(x.Code)).ToList();
                    }

                    if (itemId.HasValue || itemId > 0)
                    {
                        rcvData = rcvData.Where(x => rcvDetailData.Select(y => y.Code).Contains(x.Code)).ToList();
                    }

                    if (!string.IsNullOrWhiteSpace(supCode))
                    {
                        rcvData = rcvData.Where(x => x.SupCode == supCode).ToList();
                    }

                    rcvData = rcvData.OrderBy(x => x.Date).ToList();

                    rcvData.Add(new Entity.Purchase.ReportByRCV
                    {
                        Code = "Total",
                        GrossAmount = rcvData.Sum(x => x.GrossAmount),
                        SubTotal = rcvData.Sum(x => x.SubTotal),
                        Disc = rcvData.Sum(x => x.Disc),
                        Dpp = rcvData.Sum(x => x.Dpp),
                        TaxAmount = rcvData.Sum(x => x.TaxAmount),
                        Total = rcvData.Sum(x => x.Total)
                    });

                    return rcvData.AsQueryable().ToDataSourceResult(0, rcvData.Count, null, null);
                }
                else if (type == 2)
                {
                    if (!string.IsNullOrEmpty(status))
                    {
                        rcvDetailData = rcvDetailData.Where(x => rcvData.Select(y => y.Code).Contains(x.Code)).ToList();
                    }

                    if (!string.IsNullOrWhiteSpace(supCode))
                    {
                        supData = supData.Where(x => x.Code == supCode).ToList();
                    }

                    if (categoryId.HasValue || categoryId > 0)
                    {
                        rcvDetailData = rcvDetailData.Where(x => itemCategoryData.Select(y => y.CategoryId).Contains(x.CategoryId)).ToList();
                    }

                    foreach (var itemSup in supData)
                    {
                        itemSup.TotalTrans = rcvDetailData.Count(x => x.SupCode == itemSup.Code);
                        itemSup.GrossAmount = rcvDetailData.Where(x => x.SupCode == itemSup.Code).Sum(x => x.TotalGrossAmount);
                        itemSup.SubTotal = rcvDetailData.Where(x => x.SupCode == itemSup.Code).Sum(x => x.TotalAfterDisc);
                        itemSup.Disc = rcvDetailData.Where(x => x.SupCode == itemSup.Code).Sum(x => x.TotalDisc + x.TotalDiscHeader);
                        itemSup.Dpp = rcvDetailData.Where(x => x.SupCode == itemSup.Code).Sum(x => x.TotalDpp);
                        itemSup.TaxAmount = rcvDetailData.Where(x => x.SupCode == itemSup.Code).Sum(x => x.TotalTaxAmount);
                        itemSup.Total = rcvDetailData.Where(x => x.SupCode == itemSup.Code).Sum(x => x.TotalNettPrice);
                    }

                    supData = supData.Where(x => x.TotalTrans > 0).OrderBy(x => x.Code).ToList();

                    supData.Add(new Entity.Purchase.ReportBySupplierPurchase
                    {
                        Name = "Total",
                        TotalTrans = supData.Sum(x => x.TotalTrans),
                        GrossAmount = supData.Sum(x => x.GrossAmount),
                        SubTotal = supData.Sum(x => x.SubTotal),
                        Disc = supData.Sum(x => x.Disc),
                        Dpp = supData.Sum(x => x.Dpp),
                        TaxAmount = supData.Sum(x => x.TaxAmount),
                        Total = supData.Sum(x => x.Total)
                    });

                    return supData.AsQueryable().ToDataSourceResult(0, supData.Count, null, null);
                }
                else if (type == 3)
                {
                    if (!string.IsNullOrEmpty(status))
                    {
                        rcvDetailData = rcvDetailData.Where(x => rcvData.Select(y => y.Code).Contains(x.Code)).ToList();
                    }

                    if (!string.IsNullOrWhiteSpace(supCode))
                    {
                        rcvDetailData = rcvDetailData.Where(x => x.SupCode == supCode).ToList();
                    }

                    if (categoryId.HasValue || categoryId > 0)
                    {
                        rcvDetailData = rcvDetailData.Where(x => itemCategoryData.Select(y => y.CategoryId).Contains(x.CategoryId)).ToList();
                    }

                    foreach (var item in itemData)
                    {
                        item.TotalTrans = rcvDetailData.Count(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId);
                        item.Qty = rcvDetailData.Where(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId).Sum(x => x.Qty);
                        item.GrossAmount = rcvDetailData.Where(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId).Sum(x => x.TotalGrossAmount);
                        item.SubTotal = rcvDetailData.Where(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId).Sum(x => x.TotalAfterDisc);
                        item.Disc = rcvDetailData.Where(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId).Sum(x => x.TotalDisc);
                        item.DiscHeader = rcvDetailData.Where(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId).Sum(x => x.TotalDiscHeader);
                        item.Dpp = rcvDetailData.Where(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId).Sum(x => x.TotalDpp);
                        item.TaxAmount = rcvDetailData.Where(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId).Sum(x => x.TotalTaxAmount);
                        item.Total = rcvDetailData.Where(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId).Sum(x => x.TotalNettPrice);
                    }

                    itemData = itemData.Where(x => x.TotalTrans > 0).OrderBy(x => x.Initial).ToList();

                    itemData.Add(new Entity.Purchase.ReportByItemPurchase
                    {
                        Name = "Total",
                        TotalTrans = itemData.Sum(x => x.TotalTrans),
                        Qty = itemData.Sum(x => x.Qty),
                        GrossAmount = itemData.Sum(x => x.GrossAmount),
                        SubTotal = itemData.Sum(x => x.SubTotal),
                        Disc = itemData.Sum(x => x.Disc),
                        DiscHeader = itemData.Sum(x => x.DiscHeader),
                        Dpp = itemData.Sum(x => x.Dpp),
                        TaxAmount = itemData.Sum(x => x.TaxAmount),
                        Total = itemData.Sum(x => x.Total)
                    });

                    return itemData.AsQueryable().ToDataSourceResult(0, itemData.Count, null, null);
                }
                else if (type == 4)
                {
                    if (!string.IsNullOrEmpty(status))
                    {
                        rcvDetailData = rcvDetailData.Where(x => rcvData.Select(y => y.Code).Contains(x.Code)).ToList();
                    }

                    if (!string.IsNullOrWhiteSpace(supCode))
                    {
                        rcvDetailData = rcvDetailData.Where(x => x.SupCode == supCode).ToList();
                    }

                    foreach (var item in itemCategoryData)
                    {
                        item.TotalTrans = rcvDetailData.Count(x => x.CategoryId == item.CategoryId && x.UnitId == item.UnitId);
                        item.Qty = rcvDetailData.Where(x => x.CategoryId == item.CategoryId && x.UnitId == item.UnitId).Sum(x => x.Qty);
                        item.GrossAmount = rcvDetailData.Where(x => x.CategoryId == item.CategoryId && x.UnitId == item.UnitId).Sum(x => x.TotalGrossAmount);
                        item.SubTotal = rcvDetailData.Where(x => x.CategoryId == item.CategoryId && x.UnitId == item.UnitId).Sum(x => x.TotalAfterDisc);
                        item.Disc = rcvDetailData.Where(x => x.CategoryId == item.CategoryId && x.UnitId == item.UnitId).Sum(x => x.TotalDisc);
                        item.DiscHeader = rcvDetailData.Where(x => x.CategoryId == item.CategoryId && x.UnitId == item.UnitId).Sum(x => x.TotalDiscHeader);
                        item.Dpp = rcvDetailData.Where(x => x.CategoryId == item.CategoryId && x.UnitId == item.UnitId).Sum(x => x.TotalDpp);
                        item.TaxAmount = rcvDetailData.Where(x => x.CategoryId == item.CategoryId && x.UnitId == item.UnitId).Sum(x => x.TotalTaxAmount);
                        item.Total = rcvDetailData.Where(x => x.CategoryId == item.CategoryId && x.UnitId == item.UnitId).Sum(x => x.TotalNettPrice);
                    }

                    itemCategoryData = itemCategoryData.Where(x => x.TotalTrans > 0).OrderBy(x => x.Initial).ToList();

                    itemCategoryData.Add(new Entity.Purchase.ReportByItemCategoryPurchase
                    {
                        Name = "Total",
                        TotalTrans = itemCategoryData.Sum(x => x.TotalTrans),
                        Qty = itemCategoryData.Sum(x => x.Qty),
                        GrossAmount = itemCategoryData.Sum(x => x.GrossAmount),
                        SubTotal = itemCategoryData.Sum(x => x.SubTotal),
                        Disc = itemCategoryData.Sum(x => x.Disc),
                        DiscHeader = itemCategoryData.Sum(x => x.DiscHeader),
                        Dpp = itemCategoryData.Sum(x => x.Dpp),
                        TaxAmount = itemCategoryData.Sum(x => x.TaxAmount),
                        Total = itemCategoryData.Sum(x => x.Total)
                    });

                    return itemCategoryData.AsQueryable().ToDataSourceResult(0, itemCategoryData.Count, null, null);
                }
                else
                {
                    if (!string.IsNullOrEmpty(status))
                    {
                        rcvDetailData = rcvDetailData.Where(x => rcvData.Select(y => y.Code).Contains(x.Code)).ToList();
                    }

                    if (!string.IsNullOrWhiteSpace(supCode))
                    {
                        rcvDetailData = rcvDetailData.Where(x => x.SupCode == supCode).ToList();
                    }

                    if (categoryId.HasValue || categoryId > 0)
                    {
                        rcvDetailData = rcvDetailData.Where(x => itemCategoryData.Select(y => y.CategoryId).Contains(x.CategoryId)).ToList();
                    }

                    if (unitId.HasValue || unitId > 0)
                    {
                        rcvDetailData = rcvDetailData.Where(x => x.UnitId == unitId).ToList();
                    }

                    rcvDetailData = rcvDetailData.OrderBy(x => x.Date).ToList();

                    rcvDetailData.Add(new Entity.Purchase.ReportByDetailRCV
                    {
                        Code = "Total",
                        Qty = rcvDetailData.Sum(x => x.Qty),
                        TotalGrossAmount = rcvDetailData.Sum(x => x.TotalGrossAmount),
                        TotalDisc = rcvDetailData.Sum(x => x.TotalDisc),
                        TotalDiscHeader = rcvDetailData.Sum(x => x.TotalDiscHeader),
                        TotalAfterDisc = rcvDetailData.Sum(x => x.TotalAfterDisc),
                        TotalDpp = rcvDetailData.Sum(x => x.TotalDpp),
                        TotalTaxAmount = rcvDetailData.Sum(x => x.TotalTaxAmount),
                        TotalNettPrice = rcvDetailData.Sum(x => x.TotalNettPrice)
                    });

                    return rcvDetailData.AsQueryable().ToDataSourceResult(0, rcvDetailData.Count, null, null);
                }
            }
            else
            {
                if (type == 1)
                {
                    if (!string.IsNullOrWhiteSpace(code))
                    {
                        rcvDetailData = rcvDetailData.Where(x => x.Code == code).ToList();
                    }

                    rcvDetailData = rcvDetailData.OrderBy(x => x.Date).ToList();

                    rcvDetailData.Add(new Entity.Purchase.ReportByDetailRCV
                    {
                        Code = "Total",
                        Qty = rcvDetailData.Sum(x => x.Qty),
                        TotalGrossAmount = rcvDetailData.Sum(x => x.TotalGrossAmount),
                        TotalDisc = rcvDetailData.Sum(x => x.TotalDisc),
                        TotalDiscHeader = rcvDetailData.Sum(x => x.TotalDiscHeader),
                        TotalAfterDisc = rcvDetailData.Sum(x => x.TotalAfterDisc),
                        TotalDpp = rcvDetailData.Sum(x => x.TotalDpp),
                        TotalTaxAmount = rcvDetailData.Sum(x => x.TotalTaxAmount),
                        TotalNettPrice = rcvDetailData.Sum(x => x.TotalNettPrice)
                    });

                    return rcvDetailData.AsQueryable().ToDataSourceResult(0, rcvDetailData.Count, null, null);
                }
                else
                {
                    if (!string.IsNullOrEmpty(status))
                    {
                        rcvDetailData = rcvDetailData.Where(x => rcvData.Select(y => y.Code).Contains(x.Code)).ToList();
                    }

                    if (!string.IsNullOrWhiteSpace(supCode))
                    {
                        rcvDetailData = rcvDetailData.Where(x => x.SupCode == supCode).ToList();
                    }

                    if (categoryId.HasValue || categoryId > 0)
                    {
                        rcvDetailData = rcvDetailData.Where(x => itemCategoryData.Select(y => y.CategoryId).Contains(x.CategoryId)).ToList();
                    }

                    if (unitId.HasValue || unitId > 0)
                    {
                        rcvDetailData = rcvDetailData.Where(x => x.UnitId == unitId).ToList();
                    }

                    rcvDetailData = rcvDetailData.OrderBy(x => x.Date).ToList();

                    rcvDetailData.Add(new Entity.Purchase.ReportByDetailRCV
                    {
                        Code = "Total",
                        Qty = rcvDetailData.Sum(x => x.Qty),
                        TotalGrossAmount = rcvDetailData.Sum(x => x.TotalGrossAmount),
                        TotalDisc = rcvDetailData.Sum(x => x.TotalDisc),
                        TotalDiscHeader = rcvDetailData.Sum(x => x.TotalDiscHeader),
                        TotalAfterDisc = rcvDetailData.Sum(x => x.TotalAfterDisc),
                        TotalDpp = rcvDetailData.Sum(x => x.TotalDpp),
                        TotalTaxAmount = rcvDetailData.Sum(x => x.TotalTaxAmount),
                        TotalNettPrice = rcvDetailData.Sum(x => x.TotalNettPrice)
                    });

                    return rcvDetailData.AsQueryable().ToDataSourceResult(0, rcvDetailData.Count, null, null);
                }
            }
        }
    }
}
