using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Sales;
using Microsoft.EntityFrameworkCore;

namespace ERP.Web.API.Domain.Services.Sales
{
    public class SalesOrderReportService : ISalesOrderReportService
    {
        private readonly TenantContext _db;
        public SalesOrderReportService(TenantContext db)
        {
            _db = db;
        }

        public DataSourceResult GetData(int type, string startDate, string endDate, string custCode, string status, int? itemId, string code, bool isDetail, int? unitId, int? categoryId)
        {
            var soData = _db.ReportBySOs.FromSqlRaw(@"SELECT so.[Date], so.Code, so.CustCode, so.CustName,
                            SUM(so_d.Qty * so_d.UnitPrice) AS GrossAmount, SUM(so_d.Qty * (so_d.UnitPrice - so_d.Disc - so_d.FinalDiscHeader)) AS SubTotal,
                            SUM(so_d.Qty * so_d.Disc) AS Disc, SUM(so_d.Qty * so_d.FinalDiscHeader) AS DiscHeader,
                            so.DPP, so.TaxAmount, so.Total,
                            CASE so.Mark
	                            WHEN 'A' THEN 'Aktif'
	                            WHEN 'V' THEN 'Void'
	                            WHEN 'PS' THEN 'Dikirim Sebagian'
	                            WHEN 'CMP' THEN 'Dikirim Seluruhnya'
	                            WHEN 'CLS' THEN 'Ditutup' END AS [Status]
                            FROM Sales.vwSalesOrderHeader so
                            LEFT JOIN Sales.vwSalesOrderDetail so_d ON so_d.Code = so.Code
                            WHERE so.FromDirectInvoice = 0" +
                            (string.IsNullOrEmpty(status) ? "" : $" AND so.Mark = '{status.Replace("'", "''")}'") +
                            " GROUP BY so.[Date], so.Code, so.CustCode, so.CustName, so.DPP, so.TaxAmount, so.Total, so.Mark").ToList();

            var soDetailData = _db.ReportByDetailSOs.FromSqlRaw(@"SELECT so.[Date], so.Code, so.CustCode,so.CustName,
                            im.Initial AS ItemInitial, im.[Name] AS ItemName, so_d.Qty,
                            so_d.UnitId, so_d.UnitName, so_d.UnitPrice AS GrossAmount,
                            so_d.Disc AS Disc, so_d.FinalDiscHeader AS DiscHeader,
                            so_d.UnitPrice - so_d.Disc - so_d.FinalDiscHeader AS SubTotal,
                            so_d.DPP, so_d.TaxAmount, so_d.NettPrice,
                            so_d.UnitPrice * so_d.Qty AS TotalGrossAmount, (so_d.UnitPrice - so_d.Disc - so_d.FinalDiscHeader) * so_d.Qty AS TotalAfterDisc,
                            so_d.Disc * so_d.Qty AS TotalDisc, so_d.FinalDiscHeader * so_d.Qty AS TotalDiscHeader,
                            so_d.DPP * so_d.Qty AS TotalDPP, so_d.TaxAmount * so_d.Qty AS TotalTaxAmount, so_d.Total, so_d.NettPrice * so_d.Qty AS TotalNettPrice,
                            CASE so.Mark
                                WHEN 'A' THEN 'Aktif'
                                WHEN 'V' THEN 'Void'
                                WHEN 'PS' THEN 'Dikirim Sebagian'
                                WHEN 'CMP' THEN 'Dikirim Seluruhnya'
                                WHEN 'CLS' THEN 'Ditutup' END AS [Status],
                            ic.Id AS CategoryId, ic.Initial AS CategoryInitial
                            FROM Sales.vwSalesOrderDetail so_d
                            LEFT JOIN Sales.vwSalesOrderHeader so ON so.Code = so_d.Code
                            LEFT JOIN Inventory.Item im ON im.Id = so_d.ItemId
                            LEFT JOIN Inventory.ItemCategory ic ON ic.Id = im.CategoryId
                            WHERE so.FromDirectInvoice = 0" +
                            (!itemId.HasValue || itemId <= 0 ? "" : $" AND so_d.ItemId = {itemId}")).ToList();

            var itemData = _db.ReportByItemSales.FromSqlRaw(@"SELECT im.Initial, im.[Name], 
                            ic.Id AS CategoryId, ic.Initial AS CategoryInitial,
                            uc.Id AS UnitId, uc.UnitEquivalent AS UnitName,
                            CAST (0 AS int) AS TotalTrans, CAST (0 AS decimal) AS Qty, CAST (0 AS decimal) AS SubTotal,
                            CAST (0 AS decimal) AS Disc, CAST (0 AS decimal) AS DiscHeader, CAST (0 AS decimal) AS Dpp,
                            CAST (0 AS decimal) AS TaxAmount, CAST (0 AS decimal) AS Total, CAST (0 AS decimal) AS GrossAmount
                            FROM Inventory.Item im
                            LEFT JOIN Sales.SalesOrderDetail so_d ON so_d.ItemId = im.Id
                            LEFT JOIN Sales.SalesOrderHeader so ON so.Code = so_d.Code
                            LEFT JOIN Inventory.ItemCategory ic ON ic.Id = im.CategoryId
                            LEFT JOIN Inventory.UoMConversion uc ON uc.Id = so_d.UnitId
                            WHERE uc.Id IS NOT NULL AND so.FromDirectInvoice = 0
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
                            LEFT JOIN Sales.SalesOrderDetail so_d ON so_d.ItemId = im.Id
                            LEFT JOIN Sales.SalesOrderHeader so ON so.Code = so_d.Code
                            LEFT JOIN Inventory.UoMConversion uc ON uc.Id = so_d.UnitId
                            WHERE uc.Id IS NOT NULL AND so.FromDirectInvoice = 0" +
                            (!categoryId.HasValue || categoryId <= 0 ? "" : $" AND ic.Id = {categoryId}") +
                            " GROUP BY ic.Id, ic.Initial, ic.[Name], uc.Id, uc.UnitEquivalent").ToList();

            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                soData = soData.Where(x => x.Date >= Convert.ToDateTime(startDate) && x.Date <= Convert.ToDateTime(endDate)).ToList();
                soDetailData = soDetailData.Where(x => x.Date >= Convert.ToDateTime(startDate) && x.Date <= Convert.ToDateTime(endDate)).ToList();
            }
            else if (!string.IsNullOrEmpty(startDate))
            {
                soData = soData.Where(x => x.Date >= Convert.ToDateTime(startDate)).ToList();
                soDetailData = soDetailData.Where(x => x.Date >= Convert.ToDateTime(startDate)).ToList();
            }
            else if (!string.IsNullOrEmpty(endDate))
            {
                soData = soData.Where(x => x.Date <= Convert.ToDateTime(endDate)).ToList();
                soDetailData = soDetailData.Where(x => x.Date <= Convert.ToDateTime(endDate)).ToList();
            }

            if (!isDetail)
            {
                if (type == 1)
                {
                    if (categoryId.HasValue || categoryId > 0)
                    {
                        soDetailData = soDetailData.Where(x => itemCategoryData.Select(y => y.CategoryId).Contains(x.CategoryId)).ToList();
                        if (!itemId.HasValue || itemId <= 0)
                            soData = soData.Where(x => soDetailData.Select(y => y.Code).Contains(x.Code)).ToList();
                    }

                    if (itemId.HasValue || itemId > 0)
                    {
                        soData = soData.Where(x => soDetailData.Select(y => y.Code).Contains(x.Code)).ToList();
                    }

                    if (!string.IsNullOrWhiteSpace(custCode))
                    {
                        soData = soData.Where(x => x.CustCode == custCode).ToList();
                    }

                    soData = soData.OrderBy(x => x.Date).ToList();

                    soData.Add(new Entity.Sales.ReportBySO
                    {
                        Code = "Total",
                        GrossAmount = soData.Sum(x => x.GrossAmount),
                        Disc = soData.Sum(x => x.Disc),
                        DiscHeader = soData.Sum(x => x.DiscHeader),
                        SubTotal = soData.Sum(x => x.SubTotal),
                        Dpp = soData.Sum(x => x.Dpp),
                        TaxAmount = soData.Sum(x => x.TaxAmount),
                        Total = soData.Sum(x => x.Total)
                    });

                    return soData.AsQueryable().ToDataSourceResult(0, soData.Count, null, null);
                }
                else if (type == 2)
                {
                    if (!string.IsNullOrEmpty(status))
                    {
                        soDetailData = soDetailData.Where(x => soData.Select(y => y.Code).Contains(x.Code)).ToList();
                    }

                    if (!string.IsNullOrWhiteSpace(custCode))
                    {
                        custData = custData.Where(x => x.Code == custCode).ToList();
                    }

                    if (categoryId.HasValue || categoryId > 0)
                    {
                        soDetailData = soDetailData.Where(x => itemCategoryData.Select(y => y.CategoryId).Contains(x.CategoryId)).ToList();
                    }

                    foreach (var itemCust in custData)
                    {
                        itemCust.TotalTrans = soDetailData.Count(x => x.CustCode == itemCust.Code);
                        itemCust.GrossAmount = soDetailData.Where(x => x.CustCode == itemCust.Code).Sum(x => x.TotalGrossAmount);
                        itemCust.Disc = soDetailData.Where(x => x.CustCode == itemCust.Code).Sum(x => x.TotalDisc);
                        itemCust.DiscHeader = soDetailData.Where(x => x.CustCode == itemCust.Code).Sum(x => x.TotalDiscHeader);
                        itemCust.SubTotal = soDetailData.Where(x => x.CustCode == itemCust.Code).Sum(x => x.TotalAfterDisc);
                        itemCust.Dpp = soDetailData.Where(x => x.CustCode == itemCust.Code).Sum(x => x.TotalDpp);
                        itemCust.TaxAmount = soDetailData.Where(x => x.CustCode == itemCust.Code).Sum(x => x.TotalTaxAmount);
                        itemCust.Total = soDetailData.Where(x => x.CustCode == itemCust.Code).Sum(x => x.Total);
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
                        soDetailData = soDetailData.Where(x => soData.Select(y => y.Code).Contains(x.Code)).ToList();
                    }

                    if (!string.IsNullOrWhiteSpace(custCode))
                    {
                        soDetailData = soDetailData.Where(x => x.CustCode == custCode).ToList();
                    }

                    if (categoryId.HasValue || categoryId > 0)
                    {
                        soDetailData = soDetailData.Where(x => itemCategoryData.Select(y => y.CategoryId).Contains(x.CategoryId)).ToList();
                    }

                    foreach (var item in itemData)
                    {
                        item.TotalTrans = soDetailData.Count(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId);
                        item.Qty = soDetailData.Where(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId).Sum(x => x.Qty);
                        item.GrossAmount = soDetailData.Where(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId).Sum(x => x.TotalGrossAmount);
                        item.Disc = soDetailData.Where(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId).Sum(x => x.TotalDisc);
                        item.DiscHeader = soDetailData.Where(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId).Sum(x => x.TotalDiscHeader);
                        item.SubTotal = soDetailData.Where(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId).Sum(x => x.TotalAfterDisc);
                        item.Dpp = soDetailData.Where(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId).Sum(x => x.TotalDpp);
                        item.TaxAmount = soDetailData.Where(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId).Sum(x => x.TotalTaxAmount);
                        item.Total = soDetailData.Where(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId).Sum(x => x.Total);
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
                        soDetailData = soDetailData.Where(x => soData.Select(y => y.Code).Contains(x.Code)).ToList();
                    }

                    if (!string.IsNullOrWhiteSpace(custCode))
                    {
                        soDetailData = soDetailData.Where(x => x.CustCode == custCode).ToList();
                    }

                    foreach (var item in itemCategoryData)
                    {
                        item.TotalTrans = soDetailData.Count(x => x.CategoryId == item.CategoryId && x.UnitId == item.UnitId);
                        item.Qty = soDetailData.Where(x => x.CategoryId == item.CategoryId && x.UnitId == item.UnitId).Sum(x => x.Qty);
                        item.GrossAmount = soDetailData.Where(x => x.CategoryId == item.CategoryId && x.UnitId == item.UnitId).Sum(x => x.TotalGrossAmount);
                        item.Disc = soDetailData.Where(x => x.CategoryId == item.CategoryId && x.UnitId == item.UnitId).Sum(x => x.TotalDisc);
                        item.DiscHeader = soDetailData.Where(x => x.CategoryId == item.CategoryId && x.UnitId == item.UnitId).Sum(x => x.TotalDiscHeader);
                        item.SubTotal = soDetailData.Where(x => x.CategoryId == item.CategoryId && x.UnitId == item.UnitId).Sum(x => x.TotalAfterDisc);
                        item.Dpp = soDetailData.Where(x => x.CategoryId == item.CategoryId && x.UnitId == item.UnitId).Sum(x => x.TotalDpp);
                        item.TaxAmount = soDetailData.Where(x => x.CategoryId == item.CategoryId && x.UnitId == item.UnitId).Sum(x => x.TotalTaxAmount);
                        item.Total = soDetailData.Where(x => x.CategoryId == item.CategoryId && x.UnitId == item.UnitId).Sum(x => x.Total);
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
                        soDetailData = soDetailData.Where(x => soData.Select(y => y.Code).Contains(x.Code)).ToList();
                    }

                    if (!string.IsNullOrWhiteSpace(custCode))
                    {
                        soDetailData = soDetailData.Where(x => x.CustCode == custCode).ToList();
                    }

                    if (categoryId.HasValue || categoryId > 0)
                    {
                        soDetailData = soDetailData.Where(x => itemCategoryData.Select(y => y.CategoryId).Contains(x.CategoryId)).ToList();
                    }

                    if (unitId.HasValue || unitId > 0)
                    {
                        soDetailData = soDetailData.Where(x => x.UnitId == unitId).ToList();
                    }

                    soDetailData = soDetailData.OrderBy(x => x.Date).ToList();

                    soDetailData.Add(new Entity.Sales.ReportByDetailSO
                    {
                        Code = "Total",
                        Qty = soDetailData.Sum(x => x.Qty),
                        TotalGrossAmount = soDetailData.Sum(x => x.TotalGrossAmount),
                        TotalDisc = soDetailData.Sum(x => x.TotalDisc),
                        TotalDiscHeader = soDetailData.Sum(x => x.TotalDiscHeader),
                        TotalAfterDisc = soDetailData.Sum(x => x.TotalAfterDisc),
                        TotalDpp = soDetailData.Sum(x => x.TotalDpp),
                        TotalTaxAmount = soDetailData.Sum(x => x.TotalTaxAmount),
                        TotalNettPrice = soDetailData.Sum(x => x.TotalNettPrice)
                    });

                    return soDetailData.AsQueryable().ToDataSourceResult(0, soDetailData.Count, null, null);
                }
            }
            else
            {
                if (type == 1)
                {
                    if (!string.IsNullOrWhiteSpace(code))
                    {
                        soDetailData = soDetailData.Where(x => x.Code == code).ToList();
                    }

                    soDetailData = soDetailData.OrderBy(x => x.Date).ToList();

                    soDetailData.Add(new Entity.Sales.ReportByDetailSO
                    {
                        Code = "Total",
                        Qty = soDetailData.Sum(x => x.Qty),
                        TotalGrossAmount = soDetailData.Sum(x => x.TotalGrossAmount),
                        TotalDisc = soDetailData.Sum(x => x.TotalDisc),
                        TotalDiscHeader = soDetailData.Sum(x => x.TotalDiscHeader),
                        TotalAfterDisc = soDetailData.Sum(x => x.TotalAfterDisc),
                        TotalDpp = soDetailData.Sum(x => x.TotalDpp),
                        TotalTaxAmount = soDetailData.Sum(x => x.TotalTaxAmount),
                        TotalNettPrice = soDetailData.Sum(x => x.TotalNettPrice)
                    });

                    return soDetailData.AsQueryable().ToDataSourceResult(0, soDetailData.Count, null, null);
                }
                else
                {
                    if (!string.IsNullOrEmpty(status))
                    {
                        soDetailData = soDetailData.Where(x => soData.Select(y => y.Code).Contains(x.Code)).ToList();
                    }

                    if (!string.IsNullOrWhiteSpace(custCode))
                    {
                        soDetailData = soDetailData.Where(x => x.CustCode == custCode).ToList();
                    }

                    if (categoryId.HasValue || categoryId > 0)
                    {
                        soDetailData = soDetailData.Where(x => itemCategoryData.Select(y => y.CategoryId).Contains(x.CategoryId)).ToList();
                    }

                    if (unitId.HasValue || unitId > 0)
                    {
                        soDetailData = soDetailData.Where(x => x.UnitId == unitId).ToList();
                    }

                    soDetailData = soDetailData.OrderBy(x => x.Date).ToList();

                    soDetailData.Add(new Entity.Sales.ReportByDetailSO
                    {
                        Code = "Total",
                        Qty = soDetailData.Sum(x => x.Qty),
                        TotalGrossAmount = soDetailData.Sum(x => x.TotalGrossAmount),
                        TotalDisc = soDetailData.Sum(x => x.TotalDisc),
                        TotalDiscHeader = soDetailData.Sum(x => x.TotalDiscHeader),
                        TotalAfterDisc = soDetailData.Sum(x => x.TotalAfterDisc),
                        TotalDpp = soDetailData.Sum(x => x.TotalDpp),
                        TotalTaxAmount = soDetailData.Sum(x => x.TotalTaxAmount),
                        TotalNettPrice = soDetailData.Sum(x => x.TotalNettPrice)
                    });

                    return soDetailData.AsQueryable().ToDataSourceResult(0, soDetailData.Count, null, null);
                }
            }
        }
    }
}
