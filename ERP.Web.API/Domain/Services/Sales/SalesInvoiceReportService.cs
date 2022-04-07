using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Sales;
using Microsoft.EntityFrameworkCore;

namespace ERP.Web.API.Domain.Services.Sales;

public class SalesInvoiceReportService : ISalesInvoiceReportService
{
    private readonly TenantContext _db;
    public SalesInvoiceReportService(TenantContext db)
    {
        _db = db;
    }
    public DataSourceResult GetData(int type, string startDate, string endDate, string custCode, string status, int? itemId, string code, bool isDetail, int? unitId, int? categoryId)
    {
        var siData = _db.ReportBySIs.FromSqlRaw(@"WITH cte_header AS (SELECT inv.[Date], inv.DueDate, inv.Code,
                            inv.SOCode AS OrderCode, inv.CustCode, inv.CustName,
                            SUM(dlv_d.Qty * dlv_d.UnitPrice) AS GrossAmount, SUM(dlv_d.Qty * (dlv_d.UnitPrice - dlv_d.Disc - dlv_d.FinalDiscHeader)) AS SubTotal,
                            SUM(dlv_d.Qty * dlv_d.Disc) AS Disc, SUM(dlv_d.Qty * dlv_d.FinalDiscHeader) AS DiscHeader,
                            dlv.DPP, dlv.TaxAmount, dlv.Total,
                            CASE inv.Mark
	                            WHEN 'A' THEN 'Aktif'
	                            WHEN 'V' THEN 'Void'
	                            WHEN 'PP' THEN 'Aktif'
	                            WHEN 'CMP' THEN 'Aktif' END AS [Status]
                            FROM Sales.vwSalesInvoiceHeader inv
                            LEFT JOIN Sales.SalesInvoiceDetail inv_d ON inv.Code = inv_d.Code
                            LEFT JOIN Sales.vwSalesDeliveryHeader dlv ON inv_d.DOCode = dlv.Code
                            LEFT JOIN Sales.SalesDeliveryDetail dlv_d ON dlv.Code = dlv_d.Code" +
                                                (string.IsNullOrEmpty(status) ? "" : status == "A" ? $" WHERE inv.Mark IN('A', 'PP', 'CMP')" : $" WHERE inv.Mark = '{status.Replace("'", "''")}'") +
                                                @" GROUP BY inv.[Date], inv.DueDate, inv.Code, inv.SOCode, inv.CustCode, inv.CustName, dlv.DPP, dlv.TaxAmount, dlv.Total, inv.Mark)
                            SELECT ch.[Date], ch.DueDate, ch.Code, 
                            ch.OrderCode, ch.CustCode, ch.CustName,
                            SUM(ch.GrossAmount) AS GrossAmount, SUM(ch.SubTotal) AS SubTotal,
                            SUM(ch.Disc) AS Disc, SUM(ch.DiscHeader) AS DiscHeader, SUM(ch.DPP) AS DPP,
                            SUM(ch.TaxAmount) AS TaxAmount, SUM(ch.Total) AS Total, ch.[Status]
                            FROM cte_header ch
                            GROUP BY ch.[Date], ch.DueDate, ch.Code, ch.OrderCode, ch.CustCode, ch.CustName, ch.[Status]").ToList();

        var siDetailData = _db.ReportByDetailSIs.FromSqlRaw(@"SELECT inv.[Date], inv.DueDate, inv.Code, inv.CustCode,inv.CustName,
                            im.Initial AS ItemInitial, im.[Name] AS ItemName, dlv_d.Qty,
                            dlv_d.UnitId, dlv_d.UnitName, dlv_d.UnitPrice AS GrossAmount,
                            dlv_d.Disc AS Disc, dlv_d.FinalDiscHeader AS DiscHeader,
                            dlv_d.UnitPrice - dlv_d.Disc - dlv_d.FinalDiscHeader AS SubTotal,
                            dlv_d.DPP, dlv_d.TaxAmount, dlv_d.NettPrice,
                            dlv_d.UnitPrice * dlv_d.Qty AS TotalGrossAmount, (dlv_d.UnitPrice - dlv_d.Disc - dlv_d.FinalDiscHeader) * dlv_d.Qty AS TotalAfterDisc,
                            dlv_d.Disc * dlv_d.Qty AS TotalDisc, dlv_d.FinalDiscHeader * dlv_d.Qty AS TotalDiscHeader,
                            dlv_d.DPP * dlv_d.Qty AS TotalDPP, dlv_d.TaxAmount * dlv_d.Qty AS TotalTaxAmount, dlv_d.Total, dlv_d.NettPrice * dlv_d.Qty AS TotalNettPrice,
                            CASE inv.Mark
	                            WHEN 'A' THEN 'Aktif'
	                            WHEN 'V' THEN 'Void'
	                            WHEN 'PP' THEN 'Aktif'
	                            WHEN 'CMP' THEN 'Aktif' END AS [Status],
                            ic.Id AS CategoryId, ic.Initial AS CategoryInitial,
                            inv.SOCode AS OrderCode, dlv.TaxInvoiceDate, dlv.TaxInvoiceNo, dlv.Code AS DoCode
                            FROM Sales.SalesInvoiceDetail inv_d
                            LEFT JOIN Sales.vwSalesInvoiceHeader inv ON inv.Code = inv_d.Code
                            LEFT JOIN Sales.vwSalesDeliveryHeader dlv ON inv_d.DOCode = dlv.Code  
                            LEFT JOIN Sales.vwSalesDeliveryDetail dlv_d on dlv_d.Code = dlv.Code
                            LEFT JOIN Inventory.Item im ON im.Id = dlv_d.ItemId
                            LEFT JOIN Inventory.ItemCategory ic ON ic.Id = im.CategoryId" +
                                                            (!itemId.HasValue || itemId <= 0 ? "" : $" WHERE dlv_d.ItemId = {itemId}")).ToList();

        var itemData = _db.ReportByItemSales.FromSqlRaw(@"SELECT im.Initial, im.[Name], 
                            ic.Id AS CategoryId, ic.Initial AS CategoryInitial,
                            uc.Id AS UnitId, uc.UnitEquivalent AS UnitName,
                            CAST (0 AS int) AS TotalTrans, CAST (0 AS decimal) AS Qty, CAST (0 AS decimal) AS SubTotal,
                            CAST (0 AS decimal) AS Disc, CAST (0 AS decimal) AS DiscHeader, CAST (0 AS decimal) AS Dpp,
                            CAST (0 AS decimal) AS TaxAmount, CAST (0 AS decimal) AS Total, CAST (0 AS decimal) AS GrossAmount
                            FROM Inventory.Item im
                            LEFT JOIN Sales.SalesDeliveryDetail dlv_d ON dlv_d.ItemId = im.Id
                            LEFT JOIN Sales.SalesInvoiceDetail inv_d ON inv_d.DOCode = dlv_d.Code
                            LEFT JOIN Inventory.ItemCategory ic ON ic.Id = im.CategoryId
                            LEFT JOIN Inventory.UoMConversion uc ON uc.Id = dlv_d.UnitId
                            WHERE uc.Id IS NOT NULL
                            GROUP BY im.Initial, im.[Name], ic.Id, ic.Initial, uc.Id, uc.UnitEquivalent").ToList();

        var custData = _db.ReportByCustomerSales.FromSqlRaw(@"SELECT cs.Code, cs.[Name], CAST (0 AS int) AS TotalTrans,
                            CAST (0 AS decimal) AS SubTotal, CAST (0 AS decimal) AS GrossAmount, 
                            CAST (0 AS decimal) AS Disc, CAST (0 AS decimal) AS DiscHeader,
                            CAST (0 AS decimal) AS Dpp, CAST (0 AS decimal) AS TaxAmount, CAST (0 AS decimal) AS Total
                            FROM General.Customer cs
                            WHERE cs.IsActive = 1
                            GROUP BY cs.Code, cs.[Name]").ToList();

        var itemCategoryData = _db.ReportByItemCategorySales.FromSqlRaw(@"SELECT ic.Id AS CategoryId, ic.Initial, ic.[Name],
                            uc.Id AS UnitId, uc.UnitEquivalent AS UnitName,
                            CAST (0 AS int) AS TotalTrans, CAST (0 AS decimal) AS Qty, CAST (0 AS decimal) AS SubTotal,
                            CAST (0 AS decimal) AS Disc, CAST (0 AS decimal) AS DiscHeader, CAST (0 AS decimal) AS Dpp,
                            CAST (0 AS decimal) AS TaxAmount, CAST (0 AS decimal) AS Total, CAST (0 AS decimal) AS GrossAmount
                            FROM Inventory.ItemCategory ic
                            LEFT JOIN Inventory.Item im ON im.CategoryId = ic.Id 
                            LEFT JOIN Sales.SalesDeliveryDetail dlv_d ON dlv_d.ItemId = im.Id
                            LEFT JOIN Sales.SalesInvoiceDetail inv_d ON inv_d.DOCode = dlv_d.Code
                            LEFT JOIN Inventory.UoMConversion uc ON uc.Id = dlv_d.UnitId
                            WHERE uc.Id IS NOT NULL" +
                                                                        (!categoryId.HasValue || categoryId <= 0 ? "" : $" AND ic.Id = {categoryId}") +
                                                                        " GROUP BY ic.Id, ic.Initial, ic.[Name], uc.Id, uc.UnitEquivalent").ToList();

        if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
        {
            siData = siData.Where(x => x.Date >= Convert.ToDateTime(startDate) && x.Date <= Convert.ToDateTime(endDate)).ToList();
            siDetailData = siDetailData.Where(x => x.Date >= Convert.ToDateTime(startDate) && x.Date <= Convert.ToDateTime(endDate)).ToList();
        }
        else if (!string.IsNullOrEmpty(startDate))
        {
            siData = siData.Where(x => x.Date >= Convert.ToDateTime(startDate)).ToList();
            siDetailData = siDetailData.Where(x => x.Date >= Convert.ToDateTime(startDate)).ToList();
        }
        else if (!string.IsNullOrEmpty(endDate))
        {
            siData = siData.Where(x => x.Date <= Convert.ToDateTime(endDate)).ToList();
            siDetailData = siDetailData.Where(x => x.Date <= Convert.ToDateTime(endDate)).ToList();
        }

        if (!isDetail)
        {
            if (type == 1)
            {
                if (categoryId.HasValue || categoryId > 0)
                {
                    siDetailData = siDetailData.Where(x => itemCategoryData.Select(y => y.CategoryId).Contains(x.CategoryId)).ToList();
                    if (!itemId.HasValue || itemId <= 0)
                        siData = siData.Where(x => siDetailData.Select(y => y.Code).Contains(x.Code)).ToList();
                }

                if (itemId.HasValue || itemId > 0)
                {
                    siData = siData.Where(x => siDetailData.Select(y => y.Code).Contains(x.Code)).ToList();
                }

                if (!string.IsNullOrWhiteSpace(custCode))
                {
                    siData = siData.Where(x => x.CustCode == custCode).ToList();
                }

                siData = siData.OrderBy(x => x.Date).ToList();

                siData.Add(new Entity.Sales.ReportBySI
                {
                    Code = "Total",
                    GrossAmount = siData.Sum(x => x.GrossAmount),
                    Disc = siData.Sum(x => x.Disc),
                    DiscHeader = siData.Sum(x => x.DiscHeader),
                    SubTotal = siData.Sum(x => x.SubTotal),
                    Dpp = siData.Sum(x => x.Dpp),
                    TaxAmount = siData.Sum(x => x.TaxAmount),
                    Total = siData.Sum(x => x.Total)
                });

                return siData.AsQueryable().ToDataSourceResult(0, siData.Count, null, null);
            }
            else if (type == 2)
            {
                if (!string.IsNullOrEmpty(status))
                {
                    siDetailData = siDetailData.Where(x => siData.Select(y => y.Code).Contains(x.Code)).ToList();
                }

                if (!string.IsNullOrWhiteSpace(custCode))
                {
                    custData = custData.Where(x => x.Code == custCode).ToList();
                }

                if (categoryId.HasValue || categoryId > 0)
                {
                    siDetailData = siDetailData.Where(x => itemCategoryData.Select(y => y.CategoryId).Contains(x.CategoryId)).ToList();
                }

                foreach (var itemCust in custData)
                {
                    itemCust.TotalTrans = siDetailData.Count(x => x.CustCode == itemCust.Code);
                    itemCust.GrossAmount = siDetailData.Where(x => x.CustCode == itemCust.Code).Sum(x => x.TotalGrossAmount);
                    itemCust.Disc = siDetailData.Where(x => x.CustCode == itemCust.Code).Sum(x => x.TotalDisc);
                    itemCust.DiscHeader = siDetailData.Where(x => x.CustCode == itemCust.Code).Sum(x => x.TotalDiscHeader);
                    itemCust.SubTotal = siDetailData.Where(x => x.CustCode == itemCust.Code).Sum(x => x.TotalAfterDisc);
                    itemCust.Dpp = siDetailData.Where(x => x.CustCode == itemCust.Code).Sum(x => x.TotalDpp);
                    itemCust.TaxAmount = siDetailData.Where(x => x.CustCode == itemCust.Code).Sum(x => x.TotalTaxAmount);
                    itemCust.Total = siDetailData.Where(x => x.CustCode == itemCust.Code).Sum(x => x.Total);
                }

                custData = custData.Where(x => x.TotalTrans > 0).OrderBy(x => x.Code).ToList();

                custData.Add(new Entity.Sales.ReportByCustomerSales
                {
                    Name = "Total",
                    TotalTrans = custData.Sum(x => x.TotalTrans),
                    GrossAmount = custData.Sum(x => x.GrossAmount),
                    Disc = custData.Sum(x => x.Disc),
                    DiscHeader = custData.Sum(x => x.DiscHeader),
                    SubTotal = custData.Sum(x => x.SubTotal),
                    Dpp = custData.Sum(x => x.Dpp),
                    TaxAmount = custData.Sum(x => x.TaxAmount),
                    Total = custData.Sum(x => x.Total)
                });

                return custData.AsQueryable().ToDataSourceResult(0, custData.Count, null, null);
            }
            else if (type == 3)
            {
                if (!string.IsNullOrEmpty(status))
                {
                    siDetailData = siDetailData.Where(x => siData.Select(y => y.Code).Contains(x.Code)).ToList();
                }

                if (!string.IsNullOrWhiteSpace(custCode))
                {
                    siDetailData = siDetailData.Where(x => x.CustCode == custCode).ToList();
                }

                if (categoryId.HasValue || categoryId > 0)
                {
                    siDetailData = siDetailData.Where(x => itemCategoryData.Select(y => y.CategoryId).Contains(x.CategoryId)).ToList();
                }

                foreach (var item in itemData)
                {
                    item.TotalTrans = siDetailData.Count(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId);
                    item.Qty = siDetailData.Where(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId).Sum(x => x.Qty);
                    item.GrossAmount = siDetailData.Where(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId).Sum(x => x.TotalGrossAmount);
                    item.Disc = siDetailData.Where(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId).Sum(x => x.TotalDisc);
                    item.DiscHeader = siDetailData.Where(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId).Sum(x => x.TotalDiscHeader);
                    item.SubTotal = siDetailData.Where(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId).Sum(x => x.TotalAfterDisc);
                    item.Dpp = siDetailData.Where(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId).Sum(x => x.TotalDpp);
                    item.TaxAmount = siDetailData.Where(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId).Sum(x => x.TotalTaxAmount);
                    item.Total = siDetailData.Where(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId).Sum(x => x.Total);
                }

                itemData = itemData.Where(x => x.TotalTrans > 0).OrderBy(x => x.Initial).ToList();

                itemData.Add(new Entity.Sales.ReportByItemSales
                {
                    Name = "Total",
                    TotalTrans = itemData.Sum(x => x.TotalTrans),
                    Qty = itemData.Sum(x => x.Qty),
                    GrossAmount = itemData.Sum(x => x.GrossAmount),
                    Disc = itemData.Sum(x => x.Disc),
                    DiscHeader = itemData.Sum(x => x.DiscHeader),
                    SubTotal = itemData.Sum(x => x.SubTotal),
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
                    siDetailData = siDetailData.Where(x => siData.Select(y => y.Code).Contains(x.Code)).ToList();
                }

                if (!string.IsNullOrWhiteSpace(custCode))
                {
                    siDetailData = siDetailData.Where(x => x.CustCode == custCode).ToList();
                }

                foreach (var item in itemCategoryData)
                {
                    item.TotalTrans = siDetailData.Count(x => x.CategoryId == item.CategoryId && x.UnitId == item.UnitId);
                    item.Qty = siDetailData.Where(x => x.CategoryId == item.CategoryId && x.UnitId == item.UnitId).Sum(x => x.Qty);
                    item.GrossAmount = siDetailData.Where(x => x.CategoryId == item.CategoryId && x.UnitId == item.UnitId).Sum(x => x.TotalGrossAmount);
                    item.Disc = siDetailData.Where(x => x.CategoryId == item.CategoryId && x.UnitId == item.UnitId).Sum(x => x.TotalDisc);
                    item.DiscHeader = siDetailData.Where(x => x.CategoryId == item.CategoryId && x.UnitId == item.UnitId).Sum(x => x.TotalDiscHeader);
                    item.SubTotal = siDetailData.Where(x => x.CategoryId == item.CategoryId && x.UnitId == item.UnitId).Sum(x => x.TotalAfterDisc);
                    item.Dpp = siDetailData.Where(x => x.CategoryId == item.CategoryId && x.UnitId == item.UnitId).Sum(x => x.TotalDpp);
                    item.TaxAmount = siDetailData.Where(x => x.CategoryId == item.CategoryId && x.UnitId == item.UnitId).Sum(x => x.TotalTaxAmount);
                    item.Total = siDetailData.Where(x => x.CategoryId == item.CategoryId && x.UnitId == item.UnitId).Sum(x => x.Total);
                }

                itemCategoryData = itemCategoryData.Where(x => x.TotalTrans > 0).OrderBy(x => x.Initial).ToList();

                itemCategoryData.Add(new Entity.Sales.ReportByItemCategorySales
                {
                    Name = "Total",
                    TotalTrans = itemCategoryData.Sum(x => x.TotalTrans),
                    Qty = itemCategoryData.Sum(x => x.Qty),
                    GrossAmount = itemCategoryData.Sum(x => x.GrossAmount),
                    Disc = itemCategoryData.Sum(x => x.Disc),
                    DiscHeader = itemCategoryData.Sum(x => x.DiscHeader),
                    SubTotal = itemCategoryData.Sum(x => x.SubTotal),
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
                    siDetailData = siDetailData.Where(x => siData.Select(y => y.Code).Contains(x.Code)).ToList();
                }

                if (!string.IsNullOrWhiteSpace(custCode))
                {
                    siDetailData = siDetailData.Where(x => x.CustCode == custCode).ToList();
                }

                if (categoryId.HasValue || categoryId > 0)
                {
                    siDetailData = siDetailData.Where(x => itemCategoryData.Select(y => y.CategoryId).Contains(x.CategoryId)).ToList();
                }

                if (unitId.HasValue || unitId > 0)
                {
                    siDetailData = siDetailData.Where(x => x.UnitId == unitId).ToList();
                }

                siDetailData = siDetailData.OrderBy(x => x.Date).ToList();

                siDetailData.Add(new Entity.Sales.ReportByDetailSI
                {
                    Code = "Total",
                    Qty = siDetailData.Sum(x => x.Qty),
                    TotalGrossAmount = siDetailData.Sum(x => x.TotalGrossAmount),
                    TotalDisc = siDetailData.Sum(x => x.TotalDisc),
                    TotalDiscHeader = siDetailData.Sum(x => x.TotalDiscHeader),
                    TotalAfterDisc = siDetailData.Sum(x => x.TotalAfterDisc),
                    TotalDpp = siDetailData.Sum(x => x.TotalDpp),
                    TotalTaxAmount = siDetailData.Sum(x => x.TotalTaxAmount),
                    TotalNettPrice = siDetailData.Sum(x => x.TotalNettPrice)
                });

                return siDetailData.AsQueryable().ToDataSourceResult(0, siDetailData.Count, null, null);
            }
        }
        else
        {
            if (type == 1)
            {
                if (!string.IsNullOrWhiteSpace(code))
                {
                    siDetailData = siDetailData.Where(x => x.Code == code).ToList();
                }

                siDetailData = siDetailData.OrderBy(x => x.Date).ToList();

                siDetailData.Add(new Entity.Sales.ReportByDetailSI
                {
                    Code = "Total",
                    Qty = siDetailData.Sum(x => x.Qty),
                    TotalGrossAmount = siDetailData.Sum(x => x.TotalGrossAmount),
                    TotalDisc = siDetailData.Sum(x => x.TotalDisc),
                    TotalDiscHeader = siDetailData.Sum(x => x.TotalDiscHeader),
                    TotalAfterDisc = siDetailData.Sum(x => x.TotalAfterDisc),
                    TotalDpp = siDetailData.Sum(x => x.TotalDpp),
                    TotalTaxAmount = siDetailData.Sum(x => x.TotalTaxAmount),
                    TotalNettPrice = siDetailData.Sum(x => x.TotalNettPrice)
                });

                return siDetailData.AsQueryable().ToDataSourceResult(0, siDetailData.Count, null, null);
            }
            else
            {
                if (!string.IsNullOrEmpty(status))
                {
                    siDetailData = siDetailData.Where(x => siData.Select(y => y.Code).Contains(x.Code)).ToList();
                }

                if (!string.IsNullOrWhiteSpace(custCode))
                {
                    siDetailData = siDetailData.Where(x => x.CustCode == custCode).ToList();
                }

                if (categoryId.HasValue || categoryId > 0)
                {
                    siDetailData = siDetailData.Where(x => itemCategoryData.Select(y => y.CategoryId).Contains(x.CategoryId)).ToList();
                }

                if (unitId.HasValue || unitId > 0)
                {
                    siDetailData = siDetailData.Where(x => x.UnitId == unitId).ToList();
                }

                siDetailData = siDetailData.OrderBy(x => x.Date).ToList();

                siDetailData.Add(new Entity.Sales.ReportByDetailSI
                {
                    Code = "Total",
                    Qty = siDetailData.Sum(x => x.Qty),
                    TotalGrossAmount = siDetailData.Sum(x => x.TotalGrossAmount),
                    TotalDisc = siDetailData.Sum(x => x.TotalDisc),
                    TotalDiscHeader = siDetailData.Sum(x => x.TotalDiscHeader),
                    TotalAfterDisc = siDetailData.Sum(x => x.TotalAfterDisc),
                    TotalDpp = siDetailData.Sum(x => x.TotalDpp),
                    TotalTaxAmount = siDetailData.Sum(x => x.TotalTaxAmount),
                    TotalNettPrice = siDetailData.Sum(x => x.TotalNettPrice)
                });

                return siDetailData.AsQueryable().ToDataSourceResult(0, siDetailData.Count, null, null);
            }
        }
    }
}