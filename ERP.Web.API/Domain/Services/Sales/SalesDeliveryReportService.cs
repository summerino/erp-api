using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Sales;
using Microsoft.EntityFrameworkCore;

namespace ERP.Web.API.Domain.Services.Sales;

public class SalesDeliveryReportService : ISalesDeliveryReportService
{
    private readonly TenantContext _db;
    public SalesDeliveryReportService(TenantContext db)
    {
        _db = db;
    }

    public DataSourceResult GetData(int type, int? srcTrans, string startDate, string endDate, string custCode, string status, int? itemId, string code, bool isDetail, int? unitId, int? categoryId)
    {
        var doData = _db.ReportByDOs.FromSqlRaw(@"SELECT do.[Date], do.Code, do.CustCode, do.CustName,
                            CAST(do.SrcTrans AS int) AS SrcTrans, do.TransCode, wh.[Name] AS WarehouseName,
                            SUM(do_d.Qty * do_d.UnitPrice) AS GrossAmount, SUM(do_d.Qty * (do_d.UnitPrice - do_d.Disc - do_d.FinalDiscHeader)) AS SubTotal,
                            SUM(do_d.Qty * do_d.Disc) AS Disc, SUM(do_d.Qty * do_d.FinalDiscHeader) AS DiscHeader,
                            SUM(do_d.DPP * do_d.Qty) AS DPP, SUM(do_d.TaxAmount * do_d.Qty) AS TaxAmount, SUM(do_d.ExemptTaxAmount * do_d.Qty) AS ExemptTaxAmount, SUM(do_d.NettPrice * do_d.Qty) AS Total,
                            CASE do.Mark
	                            WHEN 'A' THEN 'Aktif'
	                            WHEN 'V' THEN 'Void'
	                            WHEN 'INV' THEN 'Difakturkan' END AS [Status]
                            FROM Sales.vwSalesDeliveryHeader do
                            LEFT JOIN Sales.vwSalesDeliveryDetail do_d ON do_d.Code = do.Code
                            LEFT JOIN Inventory.Warehouse wh ON wh.Code = do.WarehouseCode
                            WHERE do.FromDirectInvoice = 0" +
                                                (string.IsNullOrEmpty(status) ? " AND do.Mark <> 'OL'" : status.Replace("'", "''").Equals("NV") ? " AND do.Mark NOT IN ('V', 'OL)" : $" AND do.Mark = '{status.Replace("'", "''")}'") +
                                                " GROUP BY do.[Date], do.Code, do.CustCode, do.CustName, do.SrcTrans, do.TransCode, wh.[Name], do.Mark").ToList();

        var doDetailData = _db.ReportByDetailDOs.FromSqlRaw(@"SELECT do.[Date], do.Code, do.CustCode, do.CustName,
                            CAST(do.SrcTrans AS int) AS SrcTrans, do.TransCode, wh.[Name] AS WarehouseName,
                            im.Initial AS ItemInitial, im.[Name] AS ItemName, do_d.Qty,
                            do_d.UnitId, do_d.UnitName, do_d.UnitPrice AS GrossAmount,
                            do_d.Disc AS Disc, do_d.FinalDiscHeader AS DiscHeader,
                            do_d.UnitPrice - do_d.Disc - do_d.FinalDiscHeader AS SubTotal,
                            do_d.DPP, do_d.TaxAmount, do_d.ExemptTaxAmount, do_d.NettPrice,
                            do_d.UnitPrice * do_d.Qty AS TotalGrossAmount, (do_d.UnitPrice - do_d.Disc - do_d.FinalDiscHeader) * do_d.Qty AS TotalAfterDisc,
                            do_d.Disc * do_d.Qty AS TotalDisc, do_d.FinalDiscHeader * do_d.Qty AS TotalDiscHeader,
                            do_d.DPP * do_d.Qty AS TotalDPP, do_d.TaxAmount * do_d.Qty AS TotalTaxAmount, do_d.ExemptTaxAmount * do_d.Qty AS TotalExemptTaxAmount, do_d.Total, do_d.NettPrice * do_d.Qty AS TotalNettPrice,
                            CASE do.Mark
                                WHEN 'A' THEN 'Aktif'
                                WHEN 'V' THEN 'Void'
                                WHEN 'INV' THEN 'Difakturkan' END AS [Status],
                            ic.Id AS CategoryId, ic.Initial AS CategoryInitial
                            FROM Sales.vwSalesDeliveryDetail do_d
                            LEFT JOIN Sales.vwSalesDeliveryHeader do ON do.Code = do_d.Code
                            LEFT JOIN Inventory.Item im ON im.Id = do_d.ItemId
                            LEFT JOIN Inventory.ItemCategory ic ON ic.Id = im.CategoryId
                            LEFT JOIN Inventory.Warehouse wh ON wh.Code = do.WarehouseCode
                            WHERE do.FromDirectInvoice = 0" +
                                                            (!itemId.HasValue || itemId <= 0 ? "" : $" AND do_d.ItemId = {itemId}")).ToList();

        var itemData = _db.ReportByItemSales.FromSqlRaw(@"SELECT im.Initial, im.[Name], 
                            ic.Id AS CategoryId, ic.Initial AS CategoryInitial,
                            uc.Id AS UnitId, uc.UnitEquivalent AS UnitName,
                            CAST (0 AS int) AS TotalTrans, CAST (0 AS decimal) AS Qty, CAST (0 AS decimal) AS SubTotal,
                            CAST (0 AS decimal) AS Disc, CAST (0 AS decimal) AS DiscHeader, CAST (0 AS decimal) AS Dpp,
                            CAST (0 AS decimal) AS TaxAmount, CAST (0 AS decimal) AS ExemptTaxAmount, CAST (0 AS decimal) AS Total, CAST (0 AS decimal) AS GrossAmount
                            FROM Inventory.Item im
                            LEFT JOIN Sales.SalesDeliveryDetail do_d ON do_d.ItemId = im.Id
                            LEFT JOIN Sales.SalesDeliveryHeader do ON do.Code = do_d.Code
                            LEFT JOIN Inventory.ItemCategory ic ON ic.Id = im.CategoryId
                            LEFT JOIN Inventory.UoMConversion uc ON uc.Id = do_d.UnitId
                            WHERE uc.Id IS NOT NULL AND do.FromDirectInvoice = 0
                            GROUP BY im.Initial, im.[Name], ic.Id, ic.Initial, uc.Id, uc.UnitEquivalent").ToList();

        var custData = _db.ReportByCustomerSales.FromSqlRaw(@"SELECT cs.Code, cs.[Name], CAST (0 AS int) AS TotalTrans,
                            CAST (0 AS decimal) AS SubTotal, CAST (0 AS decimal) AS GrossAmount, 
                            CAST (0 AS decimal) AS Disc, CAST (0 AS decimal) AS DiscHeader,
                            CAST (0 AS decimal) AS Dpp, CAST (0 AS decimal) AS TaxAmount, CAST (0 AS decimal) AS ExemptTaxAmount, CAST (0 AS decimal) AS Total
                            FROM General.Customer cs
                            WHERE cs.IsActive = 1
                            GROUP BY cs.Code, cs.[Name]").ToList();

        var itemCategoryData = _db.ReportByItemCategorySales.FromSqlRaw(@"SELECT ic.Id AS CategoryId, ic.Initial, ic.[Name],
                            uc.Id AS UnitId, uc.UnitEquivalent AS UnitName,
                            CAST (0 AS int) AS TotalTrans, CAST (0 AS decimal) AS Qty, CAST (0 AS decimal) AS SubTotal,
                            CAST (0 AS decimal) AS Disc, CAST (0 AS decimal) AS DiscHeader, CAST (0 AS decimal) AS Dpp,
                            CAST (0 AS decimal) AS TaxAmount, CAST (0 AS decimal) AS ExemptTaxAmount, CAST (0 AS decimal) AS Total, CAST (0 AS decimal) AS GrossAmount
                            FROM Inventory.ItemCategory ic
                            LEFT JOIN Inventory.Item im ON im.CategoryId = ic.Id 
                            LEFT JOIN Sales.SalesDeliveryDetail do_d ON do_d.ItemId = im.Id
                            LEFT JOIN Sales.SalesDeliveryHeader do ON do.Code = do_d.Code
                            LEFT JOIN Inventory.UoMConversion uc ON uc.Id = do_d.UnitId
                            WHERE uc.Id IS NOT NULL AND do.FromDirectInvoice = 0" +
                                                                        (!categoryId.HasValue || categoryId <= 0 ? "" : $" AND ic.Id = {categoryId}") +
                                                                        " GROUP BY ic.Id, ic.Initial, ic.[Name], uc.Id, uc.UnitEquivalent").ToList();

        if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
        {
            doData = doData.Where(x => x.Date >= Convert.ToDateTime(startDate) && x.Date <= Convert.ToDateTime(endDate)).ToList();
            doDetailData = doDetailData.Where(x => x.Date >= Convert.ToDateTime(startDate) && x.Date <= Convert.ToDateTime(endDate)).ToList();
        }
        else if (!string.IsNullOrEmpty(startDate))
        {
            doData = doData.Where(x => x.Date >= Convert.ToDateTime(startDate)).ToList();
            doDetailData = doDetailData.Where(x => x.Date >= Convert.ToDateTime(startDate)).ToList();
        }
        else if (!string.IsNullOrEmpty(endDate))
        {
            doData = doData.Where(x => x.Date <= Convert.ToDateTime(endDate)).ToList();
            doDetailData = doDetailData.Where(x => x.Date <= Convert.ToDateTime(endDate)).ToList();
        }

        if (srcTrans.HasValue || srcTrans > 0)
        {
            if (srcTrans.Value == 1 || srcTrans.Value == 2)
            {
                doData = doData.Where(x => x.SrcTrans == srcTrans.Value).ToList();
                doDetailData = doDetailData.Where(x => x.SrcTrans == srcTrans.Value).ToList();
            }
        }

        if (!isDetail)
        {
            if (type == 1)
            {
                if (categoryId.HasValue || categoryId > 0)
                {
                    doDetailData = doDetailData.Where(x => itemCategoryData.Select(y => y.CategoryId).Contains(x.CategoryId)).ToList();
                    if (!itemId.HasValue || itemId <= 0)
                        doData = doData.Where(x => doDetailData.Select(y => y.Code).Contains(x.Code)).ToList();
                }

                if (itemId.HasValue || itemId > 0)
                {
                    doData = doData.Where(x => doDetailData.Select(y => y.Code).Contains(x.Code)).ToList();
                }

                if (!string.IsNullOrWhiteSpace(custCode))
                {
                    doData = doData.Where(x => x.CustCode == custCode).ToList();
                }

                doData = doData.OrderBy(x => x.Date).ToList();

                doData.Add(new Entity.Sales.ReportByDO
                {
                    Code = "Total",
                    GrossAmount = doData.Sum(x => x.GrossAmount),
                    Disc = doData.Sum(x => x.Disc),
                    DiscHeader = doData.Sum(x => x.DiscHeader),
                    SubTotal = doData.Sum(x => x.SubTotal),
                    Dpp = doData.Sum(x => x.Dpp),
                    TaxAmount = doData.Sum(x => x.TaxAmount),
                    ExemptTaxAmount = doData.Sum(x => x.ExemptTaxAmount),
                    Total = doData.Sum(x => x.Total)
                });

                return doData.AsQueryable().ToDataSourceResult(0, doData.Count, null, null);
            }
            else if (type == 2)
            {
                if (!string.IsNullOrEmpty(status))
                {
                    doDetailData = doDetailData.Where(x => doData.Select(y => y.Code).Contains(x.Code)).ToList();
                }

                if (!string.IsNullOrWhiteSpace(custCode))
                {
                    custData = custData.Where(x => x.Code == custCode).ToList();
                }

                if (categoryId.HasValue || categoryId > 0)
                {
                    doDetailData = doDetailData.Where(x => itemCategoryData.Select(y => y.CategoryId).Contains(x.CategoryId)).ToList();
                }

                foreach (var itemCust in custData)
                {
                    itemCust.TotalTrans = doDetailData.Count(x => x.CustCode == itemCust.Code);
                    itemCust.GrossAmount = doDetailData.Where(x => x.CustCode == itemCust.Code).Sum(x => x.TotalGrossAmount);
                    itemCust.Disc = doDetailData.Where(x => x.CustCode == itemCust.Code).Sum(x => x.TotalDisc);
                    itemCust.DiscHeader = doDetailData.Where(x => x.CustCode == itemCust.Code).Sum(x => x.TotalDiscHeader);
                    itemCust.SubTotal = doDetailData.Where(x => x.CustCode == itemCust.Code).Sum(x => x.TotalAfterDisc);
                    itemCust.Dpp = doDetailData.Where(x => x.CustCode == itemCust.Code).Sum(x => x.TotalDpp);
                    itemCust.TaxAmount = doDetailData.Where(x => x.CustCode == itemCust.Code).Sum(x => x.TotalTaxAmount);
                    itemCust.ExemptTaxAmount = doDetailData.Where(x => x.CustCode == itemCust.Code).Sum(x => x.TotalExemptTaxAmount);
                    itemCust.Total = doDetailData.Where(x => x.CustCode == itemCust.Code).Sum(x => x.TotalNettPrice);
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
                    ExemptTaxAmount = custData.Sum(x => x.ExemptTaxAmount),
                    Total = custData.Sum(x => x.Total)
                });

                return custData.AsQueryable().ToDataSourceResult(0, custData.Count, null, null);
            }
            else if (type == 3)
            {
                if (!string.IsNullOrEmpty(status))
                {
                    doDetailData = doDetailData.Where(x => doData.Select(y => y.Code).Contains(x.Code)).ToList();
                }

                if (!string.IsNullOrWhiteSpace(custCode))
                {
                    doDetailData = doDetailData.Where(x => x.CustCode == custCode).ToList();
                }

                if (categoryId.HasValue || categoryId > 0)
                {
                    doDetailData = doDetailData.Where(x => itemCategoryData.Select(y => y.CategoryId).Contains(x.CategoryId)).ToList();
                }

                foreach (var item in itemData)
                {
                    item.TotalTrans = doDetailData.Count(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId);
                    item.Qty = doDetailData.Where(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId).Sum(x => x.Qty);
                    item.GrossAmount = doDetailData.Where(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId).Sum(x => x.TotalGrossAmount);
                    item.Disc = doDetailData.Where(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId).Sum(x => x.TotalDisc);
                    item.DiscHeader = doDetailData.Where(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId).Sum(x => x.TotalDiscHeader);
                    item.SubTotal = doDetailData.Where(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId).Sum(x => x.TotalAfterDisc);
                    item.Dpp = doDetailData.Where(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId).Sum(x => x.TotalDpp);
                    item.TaxAmount = doDetailData.Where(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId).Sum(x => x.TotalTaxAmount);
                    item.ExemptTaxAmount = doDetailData.Where(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId).Sum(x => x.TotalExemptTaxAmount);
                    item.Total = doDetailData.Where(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId).Sum(x => x.TotalNettPrice);
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
                    ExemptTaxAmount = itemData.Sum(x => x.ExemptTaxAmount),
                    Total = itemData.Sum(x => x.Total)
                });

                return itemData.AsQueryable().ToDataSourceResult(0, itemData.Count, null, null);
            }
            else if (type == 4)
            {
                if (!string.IsNullOrEmpty(status))
                {
                    doDetailData = doDetailData.Where(x => doData.Select(y => y.Code).Contains(x.Code)).ToList();
                }

                if (!string.IsNullOrWhiteSpace(custCode))
                {
                    doDetailData = doDetailData.Where(x => x.CustCode == custCode).ToList();
                }

                foreach (var item in itemCategoryData)
                {
                    item.TotalTrans = doDetailData.Count(x => x.CategoryId == item.CategoryId && x.UnitId == item.UnitId);
                    item.Qty = doDetailData.Where(x => x.CategoryId == item.CategoryId && x.UnitId == item.UnitId).Sum(x => x.Qty);
                    item.GrossAmount = doDetailData.Where(x => x.CategoryId == item.CategoryId && x.UnitId == item.UnitId).Sum(x => x.TotalGrossAmount);
                    item.Disc = doDetailData.Where(x => x.CategoryId == item.CategoryId && x.UnitId == item.UnitId).Sum(x => x.TotalDisc);
                    item.DiscHeader = doDetailData.Where(x => x.CategoryId == item.CategoryId && x.UnitId == item.UnitId).Sum(x => x.TotalDiscHeader);
                    item.SubTotal = doDetailData.Where(x => x.CategoryId == item.CategoryId && x.UnitId == item.UnitId).Sum(x => x.TotalAfterDisc);
                    item.Dpp = doDetailData.Where(x => x.CategoryId == item.CategoryId && x.UnitId == item.UnitId).Sum(x => x.TotalDpp);
                    item.TaxAmount = doDetailData.Where(x => x.CategoryId == item.CategoryId && x.UnitId == item.UnitId).Sum(x => x.TotalTaxAmount);
                    item.ExemptTaxAmount = doDetailData.Where(x => x.CategoryId == item.CategoryId && x.UnitId == item.UnitId).Sum(x => x.TotalExemptTaxAmount);
                    item.Total = doDetailData.Where(x => x.CategoryId == item.CategoryId && x.UnitId == item.UnitId).Sum(x => x.TotalNettPrice);
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
                    ExemptTaxAmount = itemCategoryData.Sum(x => x.ExemptTaxAmount),
                    Total = itemCategoryData.Sum(x => x.Total)
                });

                return itemCategoryData.AsQueryable().ToDataSourceResult(0, itemCategoryData.Count, null, null);
            }
            else
            {
                if (!string.IsNullOrEmpty(status))
                {
                    doDetailData = doDetailData.Where(x => doData.Select(y => y.Code).Contains(x.Code)).ToList();
                }

                if (!string.IsNullOrWhiteSpace(custCode))
                {
                    doDetailData = doDetailData.Where(x => x.CustCode == custCode).ToList();
                }

                if (categoryId.HasValue || categoryId > 0)
                {
                    doDetailData = doDetailData.Where(x => itemCategoryData.Select(y => y.CategoryId).Contains(x.CategoryId)).ToList();
                }

                if (unitId.HasValue || unitId > 0)
                {
                    doDetailData = doDetailData.Where(x => x.UnitId == unitId).ToList();
                }

                doDetailData = doDetailData.OrderBy(x => x.Date).ToList();

                doDetailData.Add(new Entity.Sales.ReportByDetailDO
                {
                    Code = "Total",
                    Qty = doDetailData.Sum(x => x.Qty),
                    TotalGrossAmount = doDetailData.Sum(x => x.TotalGrossAmount),
                    TotalDisc = doDetailData.Sum(x => x.TotalDisc),
                    TotalDiscHeader = doDetailData.Sum(x => x.TotalDiscHeader),
                    TotalAfterDisc = doDetailData.Sum(x => x.TotalAfterDisc),
                    TotalDpp = doDetailData.Sum(x => x.TotalDpp),
                    TotalTaxAmount = doDetailData.Sum(x => x.TotalTaxAmount),
                    TotalExemptTaxAmount = doDetailData.Sum(x => x.TotalExemptTaxAmount),
                    TotalNettPrice = doDetailData.Sum(x => x.TotalNettPrice)
                });

                return doDetailData.AsQueryable().ToDataSourceResult(0, doDetailData.Count, null, null);
            }
        }
        else
        {
            if (type == 1)
            {
                if (!string.IsNullOrWhiteSpace(code))
                {
                    doDetailData = doDetailData.Where(x => x.Code == code).ToList();
                }

                doDetailData = doDetailData.OrderBy(x => x.Date).ToList();

                doDetailData.Add(new Entity.Sales.ReportByDetailDO
                {
                    Code = "Total",
                    Qty = doDetailData.Sum(x => x.Qty),
                    TotalGrossAmount = doDetailData.Sum(x => x.TotalGrossAmount),
                    TotalDisc = doDetailData.Sum(x => x.TotalDisc),
                    TotalDiscHeader = doDetailData.Sum(x => x.TotalDiscHeader),
                    TotalAfterDisc = doDetailData.Sum(x => x.TotalAfterDisc),
                    TotalDpp = doDetailData.Sum(x => x.TotalDpp),
                    TotalTaxAmount = doDetailData.Sum(x => x.TotalTaxAmount),
                    TotalExemptTaxAmount = doDetailData.Sum(x => x.TotalExemptTaxAmount),
                    TotalNettPrice = doDetailData.Sum(x => x.TotalNettPrice)
                });

                return doDetailData.AsQueryable().ToDataSourceResult(0, doDetailData.Count, null, null);
            }
            else
            {
                if (!string.IsNullOrEmpty(status))
                {
                    doDetailData = doDetailData.Where(x => doData.Select(y => y.Code).Contains(x.Code)).ToList();
                }

                if (!string.IsNullOrWhiteSpace(custCode))
                {
                    doDetailData = doDetailData.Where(x => x.CustCode == custCode).ToList();
                }

                if (categoryId.HasValue || categoryId > 0)
                {
                    doDetailData = doDetailData.Where(x => itemCategoryData.Select(y => y.CategoryId).Contains(x.CategoryId)).ToList();
                }

                if (unitId.HasValue || unitId > 0)
                {
                    doDetailData = doDetailData.Where(x => x.UnitId == unitId).ToList();
                }

                doDetailData = doDetailData.OrderBy(x => x.Date).ToList();

                doDetailData.Add(new Entity.Sales.ReportByDetailDO
                {
                    Code = "Total",
                    Qty = doDetailData.Sum(x => x.Qty),
                    TotalGrossAmount = doDetailData.Sum(x => x.TotalGrossAmount),
                    TotalDisc = doDetailData.Sum(x => x.TotalDisc),
                    TotalDiscHeader = doDetailData.Sum(x => x.TotalDiscHeader),
                    TotalAfterDisc = doDetailData.Sum(x => x.TotalAfterDisc),
                    TotalDpp = doDetailData.Sum(x => x.TotalDpp),
                    TotalTaxAmount = doDetailData.Sum(x => x.TotalTaxAmount),
                    TotalExemptTaxAmount = doDetailData.Sum(x => x.TotalExemptTaxAmount),
                    TotalNettPrice = doDetailData.Sum(x => x.TotalNettPrice)
                });

                return doDetailData.AsQueryable().ToDataSourceResult(0, doDetailData.Count, null, null);
            }
        }
    }
}