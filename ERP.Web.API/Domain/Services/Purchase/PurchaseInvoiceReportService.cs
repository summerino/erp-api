using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Purchase;
using Microsoft.EntityFrameworkCore;

namespace ERP.Web.API.Domain.Services.Purchase;

public class PurchaseInvoiceReportService : IPurchaseInvoiceReportService
{
    private readonly TenantContext _db;
    public PurchaseInvoiceReportService(TenantContext db)
    {
        _db = db;
    }

    public DataSourceResult GetData(int type, string startDate, string endDate, string supCode, string status, int? itemId, string code, bool isDetail, int? unitId, int? categoryId)
    {
        var invData = _db.ReportByPInvs.FromSqlRaw(@"WITH cte_rcv_detail AS (SELECT rcv_d.Code, rcv_d.Qty, rcv_d.UnitPrice,
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
                            ,cte_header AS (SELECT inv.[Date], inv.DueDate, inv.Code,
                            inv.POCode AS OrderCode, inv.SupCode, inv.SupName, inv.RefNo,
                            SUM(rcv_d.Qty * rcv_d.UnitPrice) AS GrossAmount, SUM(rcv_d.Qty * (rcv_d.UnitPrice - rcv_d.Disc - rcv_d.FinalDiscHeader)) AS SubTotal,
                            SUM(rcv_d.Qty * rcv_d.Disc) AS Disc, SUM(rcv_d.Qty * rcv_d.FinalDiscHeader) AS DiscHeader,
                             SUM(rcv_d.Qty * rcv_d.DPP) AS DPP, SUM(rcv_d.Qty * rcv_d.TaxAmount) AS TaxAmount, SUM(rcv_d.Total) AS Total,
                            CASE inv.Mark
	                            WHEN 'A' THEN 'Aktif'
	                            WHEN 'V' THEN 'Void'
	                            WHEN 'PP' THEN 'Aktif'
	                            WHEN 'CMP' THEN 'Aktif' END AS [Status]
                            FROM Purchasing.vwPurchaseInvoiceHeader inv
                            LEFT JOIN Purchasing.PurchaseInvoiceDetail inv_d ON inv.Code = inv_d.Code
                            LEFT JOIN Purchasing.vwPurchaseReceiveHeader rcv ON inv_d.RcvCode = rcv.Code
                            LEFT JOIN cte_rcv_detail rcv_d ON rcv.Code = rcv_d.Code" +
                                                   (string.IsNullOrEmpty(status) ? "" : status.Replace("'", "''").Equals("A") ? $" WHERE inv.Mark IN ('A','PP','CMP')" : $" WHERE inv.Mark = '{status.Replace("'", "''")}'") +
                                                   @" GROUP BY inv.[Date], inv.DueDate, inv.Code, inv.POCode, inv.SupCode, inv.SupName, inv.RefNo, inv.Mark)
                            SELECT ch.[Date], ch.DueDate, ch.Code, 
                            ch.OrderCode, ch.SupCode, ch.SupName, ch.RefNo,
                            SUM(ch.GrossAmount) AS GrossAmount, SUM(ch.SubTotal) AS SubTotal,
                            SUM(ch.Disc + ch.DiscHeader) AS Disc, SUM(ch.DPP) AS DPP,
                            SUM(ch.TaxAmount) AS TaxAmount, SUM(ch.Total) AS Total, ch.[Status]
                            FROM cte_header ch
                            GROUP BY ch.[Date], ch.DueDate, ch.Code, ch.OrderCode, ch.SupCode, ch.SupName, ch.RefNo, ch.[Status]").ToList();

        var invDetailData = _db.ReportByDetailPInvs.FromSqlRaw(@"WITH cte_rcv_detail AS (SELECT rcv_d.Code, rcv_d.Qty,
                            rcv_d.UnitPrice, rcv_d.ItemId, rcv_d.UnitId, rcv_d.UnitName,
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
                            FROM Purchasing.vwPurchaseReceiveDetail rcv_d)
                            SELECT inv.[Date], inv.DueDate, inv.Code, inv.SupCode, inv.SupName, inv.RefNo,
                            inv_d.RcvCode, im.Initial AS ItemInitial, im.[Name] AS ItemName, rcv_d.Qty,
                            rcv_d.UnitId, rcv_d.UnitName, rcv_d.UnitPrice AS GrossAmount,
                            rcv_d.Disc AS Disc, rcv_d.FinalDiscHeader AS DiscHeader,
                            rcv_d.UnitPrice - rcv_d.Disc - rcv_d.FinalDiscHeader AS SubTotal,
                            rcv_d.DPP, rcv_d.TaxAmount, rcv_d.NettPrice,
                            rcv_d.UnitPrice * rcv_d.Qty AS TotalGrossAmount, (rcv_d.UnitPrice - rcv_d.Disc - rcv_d.FinalDiscHeader) * rcv_d.Qty AS TotalAfterDisc,
                            rcv_d.Disc * rcv_d.Qty AS TotalDisc, rcv_d.FinalDiscHeader * rcv_d.Qty AS TotalDiscHeader,
                            rcv_d.DPP * rcv_d.Qty AS TotalDPP, rcv_d.TaxAmount * rcv_d.Qty AS TotalTaxAmount, rcv_d.Total, rcv_d.NettPrice * rcv_d.Qty AS TotalNettPrice,
                            CASE inv.Mark
	                            WHEN 'A' THEN 'Aktif'
	                            WHEN 'V' THEN 'Void'
	                            WHEN 'PP' THEN 'Aktif'
	                            WHEN 'CMP' THEN 'Aktif' END AS [Status],
                            ic.Id AS CategoryId, ic.Initial AS CategoryInitial,
                            inv.POCode AS OrderCode, rcv.TaxInvoiceDate, rcv.TaxInvoiceNo, rcv.Code AS DoCode
                            FROM Purchasing.PurchaseInvoiceDetail inv_d
                            LEFT JOIN Purchasing.vwPurchaseInvoiceHeader inv ON inv.Code = inv_d.Code
                            LEFT JOIN Purchasing.vwPurchaseReceiveHeader rcv ON inv_d.RcvCode = rcv.Code  
                            LEFT JOIN cte_rcv_detail rcv_d on rcv_d.Code = rcv.Code
                            LEFT JOIN Inventory.Item im ON im.Id = rcv_d.ItemId
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
                            LEFT JOIN Purchasing.PurchaseInvoiceDetail inv_d ON inv_d.RcvCode = rcv_d.Code
                            LEFT JOIN Inventory.ItemCategory ic ON ic.Id = im.CategoryId
                            LEFT JOIN Inventory.UoMConversion uc ON uc.Id = rcv_d.UnitId
                            WHERE uc.Id IS NOT NULL
                            GROUP BY im.Initial, im.[Name], ic.Id, ic.Initial, uc.Id, uc.UnitEquivalent").ToList();

        var supData = _db.ReportBySupplierPurchases.FromSqlRaw(@"SELECT sp.Code, sp.[Name], CAST (0 AS int) AS TotalTrans,
                            CAST (0 AS decimal) AS SubTotal, CAST (0 AS decimal) AS GrossAmount, CAST (0 AS decimal) AS Disc,
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
                            LEFT JOIN Purchasing.PurchaseInvoiceDetail inv_d ON inv_d.RcvCode = rcv_d.Code
                            LEFT JOIN Inventory.UoMConversion uc ON uc.Id = rcv_d.UnitId
                            WHERE uc.Id IS NOT NULL" +
                                                                            (!categoryId.HasValue || categoryId <= 0 ? "" : $" AND ic.Id = {categoryId}") +
                                                                            " GROUP BY ic.Id, ic.Initial, ic.[Name], uc.Id, uc.UnitEquivalent").ToList();

        if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
        {
            invData = invData.Where(x => x.Date >= Convert.ToDateTime(startDate) && x.Date <= Convert.ToDateTime(endDate)).ToList();
            invDetailData = invDetailData.Where(x => x.Date >= Convert.ToDateTime(startDate) && x.Date <= Convert.ToDateTime(endDate)).ToList();
        }
        else if (!string.IsNullOrEmpty(startDate))
        {
            invData = invData.Where(x => x.Date >= Convert.ToDateTime(startDate)).ToList();
            invDetailData = invDetailData.Where(x => x.Date >= Convert.ToDateTime(startDate)).ToList();
        }
        else if (!string.IsNullOrEmpty(endDate))
        {
            invData = invData.Where(x => x.Date <= Convert.ToDateTime(endDate)).ToList();
            invDetailData = invDetailData.Where(x => x.Date <= Convert.ToDateTime(endDate)).ToList();
        }

        if (!isDetail)
        {
            if (type == 1)
            {
                if (categoryId.HasValue || categoryId > 0)
                {
                    invDetailData = invDetailData.Where(x => itemCategoryData.Select(y => y.CategoryId).Contains(x.CategoryId)).ToList();
                    if (!itemId.HasValue || itemId <= 0)
                        invData = invData.Where(x => invDetailData.Select(y => y.Code).Contains(x.Code)).ToList();
                }

                if (itemId.HasValue || itemId > 0)
                {
                    invData = invData.Where(x => invDetailData.Select(y => y.Code).Contains(x.Code)).ToList();
                }

                if (!string.IsNullOrWhiteSpace(supCode))
                {
                    invData = invData.Where(x => x.SupCode == supCode).ToList();
                }

                invData = invData.OrderBy(x => x.Date).ToList();

                invData.Add(new Entity.Purchase.ReportByPInv
                {
                    Code = "Total",
                    GrossAmount = invData.Sum(x => x.GrossAmount),
                    SubTotal = invData.Sum(x => x.SubTotal),
                    Disc = invData.Sum(x => x.Disc),
                    Dpp = invData.Sum(x => x.Dpp),
                    TaxAmount = invData.Sum(x => x.TaxAmount),
                    Total = invData.Sum(x => x.Total)
                });

                return invData.AsQueryable().ToDataSourceResult(0, invData.Count, null, null);
            }
            else if (type == 2)
            {
                if (!string.IsNullOrEmpty(status))
                {
                    invDetailData = invDetailData.Where(x => invData.Select(y => y.Code).Contains(x.Code)).ToList();
                }

                if (!string.IsNullOrWhiteSpace(supCode))
                {
                    supData = supData.Where(x => x.Code == supCode).ToList();
                }

                if (categoryId.HasValue || categoryId > 0)
                {
                    invDetailData = invDetailData.Where(x => itemCategoryData.Select(y => y.CategoryId).Contains(x.CategoryId)).ToList();
                }

                foreach (var itemSup in supData)
                {
                    itemSup.TotalTrans = invDetailData.Count(x => x.SupCode == itemSup.Code);
                    itemSup.GrossAmount = invDetailData.Where(x => x.SupCode == itemSup.Code).Sum(x => x.TotalGrossAmount);
                    itemSup.SubTotal = invDetailData.Where(x => x.SupCode == itemSup.Code).Sum(x => x.TotalAfterDisc);
                    itemSup.Disc = invDetailData.Where(x => x.SupCode == itemSup.Code).Sum(x => x.TotalDisc + x.TotalDiscHeader);
                    itemSup.Dpp = invDetailData.Where(x => x.SupCode == itemSup.Code).Sum(x => x.TotalDpp);
                    itemSup.TaxAmount = invDetailData.Where(x => x.SupCode == itemSup.Code).Sum(x => x.TotalTaxAmount);
                    itemSup.Total = invDetailData.Where(x => x.SupCode == itemSup.Code).Sum(x => x.TotalNettPrice);
                }

                supData = supData.Where(x => x.TotalTrans > 0).OrderBy(x => x.Code).ToList();

                supData.Add(new Entity.Purchase.ReportBySupplierPurchase
                {
                    Name = "Total",
                    TotalTrans = supData.Sum(x => x.TotalTrans),
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
                    invDetailData = invDetailData.Where(x => invData.Select(y => y.Code).Contains(x.Code)).ToList();
                }

                if (!string.IsNullOrWhiteSpace(supCode))
                {
                    invDetailData = invDetailData.Where(x => x.SupCode == supCode).ToList();
                }

                if (categoryId.HasValue || categoryId > 0)
                {
                    invDetailData = invDetailData.Where(x => itemCategoryData.Select(y => y.CategoryId).Contains(x.CategoryId)).ToList();
                }

                foreach (var item in itemData)
                {
                    item.TotalTrans = invDetailData.Count(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId);
                    item.Qty = invDetailData.Where(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId).Sum(x => x.Qty);
                    item.GrossAmount = invDetailData.Where(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId).Sum(x => x.TotalGrossAmount);
                    item.SubTotal = invDetailData.Where(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId).Sum(x => x.TotalAfterDisc);
                    item.Disc = invDetailData.Where(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId).Sum(x => x.TotalDisc);
                    item.DiscHeader = invDetailData.Where(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId).Sum(x => x.TotalDiscHeader);
                    item.Dpp = invDetailData.Where(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId).Sum(x => x.TotalDpp);
                    item.TaxAmount = invDetailData.Where(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId).Sum(x => x.TotalTaxAmount);
                    item.Total = invDetailData.Where(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId).Sum(x => x.TotalNettPrice);
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
                    invDetailData = invDetailData.Where(x => invData.Select(y => y.Code).Contains(x.Code)).ToList();
                }

                if (!string.IsNullOrWhiteSpace(supCode))
                {
                    invDetailData = invDetailData.Where(x => x.SupCode == supCode).ToList();
                }

                foreach (var item in itemCategoryData)
                {
                    item.TotalTrans = invDetailData.Count(x => x.CategoryId == item.CategoryId && x.UnitId == item.UnitId);
                    item.Qty = invDetailData.Where(x => x.CategoryId == item.CategoryId && x.UnitId == item.UnitId).Sum(x => x.Qty);
                    item.GrossAmount = invDetailData.Where(x => x.CategoryId == item.CategoryId && x.UnitId == item.UnitId).Sum(x => x.TotalGrossAmount);
                    item.SubTotal = invDetailData.Where(x => x.CategoryId == item.CategoryId && x.UnitId == item.UnitId).Sum(x => x.TotalAfterDisc);
                    item.Disc = invDetailData.Where(x => x.CategoryId == item.CategoryId && x.UnitId == item.UnitId).Sum(x => x.TotalDisc);
                    item.DiscHeader = invDetailData.Where(x => x.CategoryId == item.CategoryId && x.UnitId == item.UnitId).Sum(x => x.TotalDiscHeader);
                    item.Dpp = invDetailData.Where(x => x.CategoryId == item.CategoryId && x.UnitId == item.UnitId).Sum(x => x.TotalDpp);
                    item.TaxAmount = invDetailData.Where(x => x.CategoryId == item.CategoryId && x.UnitId == item.UnitId).Sum(x => x.TotalTaxAmount);
                    item.Total = invDetailData.Where(x => x.CategoryId == item.CategoryId && x.UnitId == item.UnitId).Sum(x => x.TotalNettPrice);
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
                    invDetailData = invDetailData.Where(x => invData.Select(y => y.Code).Contains(x.Code)).ToList();
                }

                if (!string.IsNullOrWhiteSpace(supCode))
                {
                    invDetailData = invDetailData.Where(x => x.SupCode == supCode).ToList();
                }

                if (categoryId.HasValue || categoryId > 0)
                {
                    invDetailData = invDetailData.Where(x => itemCategoryData.Select(y => y.CategoryId).Contains(x.CategoryId)).ToList();
                }

                if (unitId.HasValue || unitId > 0)
                {
                    invDetailData = invDetailData.Where(x => x.UnitId == unitId).ToList();
                }

                invDetailData = invDetailData.OrderBy(x => x.Date).ToList();

                invDetailData.Add(new Entity.Purchase.ReportByDetailPInv
                {
                    Code = "Total",
                    Qty = invDetailData.Sum(x => x.Qty),
                    TotalGrossAmount = invDetailData.Sum(x => x.TotalGrossAmount),
                    TotalDisc = invDetailData.Sum(x => x.TotalDisc),
                    TotalDiscHeader = invDetailData.Sum(x => x.TotalDiscHeader),
                    TotalAfterDisc = invDetailData.Sum(x => x.TotalAfterDisc),
                    TotalDpp = invDetailData.Sum(x => x.TotalDpp),
                    TotalTaxAmount = invDetailData.Sum(x => x.TotalTaxAmount),
                    TotalNettPrice = invDetailData.Sum(x => x.TotalNettPrice)
                });

                return invDetailData.AsQueryable().ToDataSourceResult(0, invDetailData.Count, null, null);
            }
        }
        else
        {
            if (type == 1)
            {
                if (!string.IsNullOrWhiteSpace(code))
                {
                    invDetailData = invDetailData.Where(x => x.Code == code).ToList();
                }

                invDetailData = invDetailData.OrderBy(x => x.Date).ToList();

                invDetailData.Add(new Entity.Purchase.ReportByDetailPInv
                {
                    Code = "Total",
                    Qty = invDetailData.Sum(x => x.Qty),
                    TotalGrossAmount = invDetailData.Sum(x => x.TotalGrossAmount),
                    TotalDisc = invDetailData.Sum(x => x.TotalDisc),
                    TotalDiscHeader = invDetailData.Sum(x => x.TotalDiscHeader),
                    TotalAfterDisc = invDetailData.Sum(x => x.TotalAfterDisc),
                    TotalDpp = invDetailData.Sum(x => x.TotalDpp),
                    TotalTaxAmount = invDetailData.Sum(x => x.TotalTaxAmount),
                    TotalNettPrice = invDetailData.Sum(x => x.TotalNettPrice)
                });

                return invDetailData.AsQueryable().ToDataSourceResult(0, invDetailData.Count, null, null);
            }
            else
            {
                if (!string.IsNullOrEmpty(status))
                {
                    invDetailData = invDetailData.Where(x => invData.Select(y => y.Code).Contains(x.Code)).ToList();
                }

                if (!string.IsNullOrWhiteSpace(supCode))
                {
                    invDetailData = invDetailData.Where(x => x.SupCode == supCode).ToList();
                }

                if (categoryId.HasValue || categoryId > 0)
                {
                    invDetailData = invDetailData.Where(x => itemCategoryData.Select(y => y.CategoryId).Contains(x.CategoryId)).ToList();
                }

                if (unitId.HasValue || unitId > 0)
                {
                    invDetailData = invDetailData.Where(x => x.UnitId == unitId).ToList();
                }

                invDetailData = invDetailData.OrderBy(x => x.Date).ToList();

                invDetailData.Add(new Entity.Purchase.ReportByDetailPInv
                {
                    Code = "Total",
                    Qty = invDetailData.Sum(x => x.Qty),
                    TotalGrossAmount = invDetailData.Sum(x => x.TotalGrossAmount),
                    TotalDisc = invDetailData.Sum(x => x.TotalDisc),
                    TotalDiscHeader = invDetailData.Sum(x => x.TotalDiscHeader),
                    TotalAfterDisc = invDetailData.Sum(x => x.TotalAfterDisc),
                    TotalDpp = invDetailData.Sum(x => x.TotalDpp),
                    TotalTaxAmount = invDetailData.Sum(x => x.TotalTaxAmount),
                    TotalNettPrice = invDetailData.Sum(x => x.TotalNettPrice)
                });

                return invDetailData.AsQueryable().ToDataSourceResult(0, invDetailData.Count, null, null);
            }
        }
    }
}