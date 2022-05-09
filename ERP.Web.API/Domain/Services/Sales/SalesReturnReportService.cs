using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Sales;
using Microsoft.EntityFrameworkCore;

namespace ERP.Web.API.Domain.Services.Sales;

public class SalesReturnReportService : ISalesReturnReportService
{
    private readonly TenantContext _db;
    public SalesReturnReportService(TenantContext db)
    {
        _db = db;
    }
    public DataSourceResult GetData(int type, string startDate, string endDate, string custCode, string status, int? itemId, string code, bool isDetail, int? unitId, int? categoryId)
    {
        var srData = _db.ReportBySRs.FromSqlRaw(@"SELECT sr.[Date], sr.Code, sr.CustCode, sr.CustName,
                            SUM(sr_d.Qty * (sr_d.UnitPrice - sr_d.Disc)) AS SubTotal, SUM(sr_d.Qty * sr_d.DPP) AS DPP,
                            SUM(sr_d.Qty * sr_d.TaxAmount) AS TaxAmount, SUM(sr_d.Qty * sr_d.NettPrice) AS Total,
                            CASE sr.[Type]
	                            WHEN '1' THEN 'Tukar Memo'
	                            WHEN '2' THEN 'Tukar Barang Sama'
	                            WHEN '3' THEN 'Tukar Barang Beda' END AS TypeName,
                            CASE sr.Mark
	                            WHEN 'A' THEN 'Aktif'
	                            WHEN 'V' THEN 'Void'
	                            WHEN 'PS' THEN 'Dikirim Sebagian'
	                            WHEN 'CMP' THEN 'Dikirim Seluruhnya'
                                WHEN 'CLS' THEN 'Ditutup' END AS [Status]
                            FROM Sales.vwSalesReturnHeader sr
                            LEFT JOIN Sales.vwSalesReturnDetail sr_d ON sr_d.Code = sr.Code" +
                                                (string.IsNullOrEmpty(status) ? "" : status.Replace("'", "''").Equals("NV") ? " WHERE sr.Mark != 'V'" : $" WHERE sr.Mark = '{status.Replace("'", "''")}'") +
                                                " GROUP BY sr.[Date], sr.Code, sr.CustCode, sr.CustName, sr.Mark, sr.[Type]").ToList();

        var srDetailData = _db.ReportByDetailSRs.FromSqlRaw(@"SELECT sr.[Date], sr.Code, sr.CustCode,sr.CustName,
                            im.Initial AS ItemInitial, im.[Name] AS ItemName, sr_d.Qty,
                            sr_d.UnitId, sr_d.UnitName, sr_d.UnitPrice AS GrossAmount,
                            sr_d.DPP, sr_d.TaxAmount, sr_d.NettPrice,
                            sr_d.UnitPrice * sr_d.Qty AS TotalGrossAmount, sr_d.DPP * sr_d.Qty AS TotalDPP,
                            sr_d.TaxAmount * sr_d.Qty AS TotalTaxAmount, sr_d.NettPrice * sr_d.Qty AS TotalNettPrice,
                            CASE sr.Mark
                                WHEN 'A' THEN 'Aktif'
                                WHEN 'V' THEN 'Void'
                                WHEN 'PS' THEN 'Dikirim Sebagian'
                                WHEN 'CMP' THEN 'Dikirim Seluruhnya'
                                WHEN 'CLS' THEN 'Ditutup' END AS [Status],
                            ic.Id AS CategoryId, ic.Initial AS CategoryInitial,
                            CASE sr.[Type]
	                            WHEN '1' THEN 'Tukar Memo'
	                            WHEN '2' THEN 'Tukar Barang Sama'
	                            WHEN '3' THEN 'Tukar Barang Beda' END AS TypeName,
                            wh.[Name] AS WarehouseIn
                            FROM Sales.vwSalesReturnDetail sr_d
                            LEFT JOIN Sales.vwSalesReturnHeader sr ON sr.Code = sr_d.Code
                            LEFT JOIN Inventory.Item im ON im.Id = sr_d.ItemId
                            LEFT JOIN Inventory.ItemCategory ic ON ic.Id = im.CategoryId
                            LEFT JOIN Inventory.Warehouse wh ON wh.Code = sr.WarehouseCode" +
                                                            (!itemId.HasValue || itemId <= 0 ? "" : $" WHERE sr_d.ItemId = {itemId}")).ToList();

        var itemData = _db.ReportByItemSales.FromSqlRaw(@"SELECT im.Initial, im.[Name], 
                            ic.Id AS CategoryId, ic.Initial AS CategoryInitial,
                            uc.Id AS UnitId, uc.UnitEquivalent AS UnitName,
                            CAST (0 AS int) AS TotalTrans, CAST (0 AS decimal) AS Qty, CAST (0 AS decimal) AS SubTotal,
                            CAST (0 AS decimal) AS Disc, CAST (0 AS decimal) AS DiscHeader, CAST (0 AS decimal) AS Dpp,
                            CAST (0 AS decimal) AS TaxAmount, CAST (0 AS decimal) AS Total, CAST (0 AS decimal) AS GrossAmount
                            FROM Inventory.Item im
                            LEFT JOIN Sales.SalesReturnDetail sr_d ON sr_d.ItemId = im.Id
                            LEFT JOIN Inventory.ItemCategory ic ON ic.Id = im.CategoryId
                            LEFT JOIN Inventory.UoMConversion uc ON uc.Id = sr_d.UnitId
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
                            LEFT JOIN Sales.SalesReturnDetail sr_d ON sr_d.ItemId = im.Id
                            LEFT JOIN Inventory.UoMConversion uc ON uc.Id = sr_d.UnitId
                            WHERE uc.Id IS NOT NULL" +
                                                                        (!categoryId.HasValue || categoryId <= 0 ? "" : $" AND ic.Id = {categoryId}") +
                                                                        " GROUP BY ic.Id, ic.Initial, ic.[Name], uc.Id, uc.UnitEquivalent").ToList();

        if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
        {
            srData = srData.Where(x => x.Date >= Convert.ToDateTime(startDate) && x.Date <= Convert.ToDateTime(endDate)).ToList();
            srDetailData = srDetailData.Where(x => x.Date >= Convert.ToDateTime(startDate) && x.Date <= Convert.ToDateTime(endDate)).ToList();
        }
        else if (!string.IsNullOrEmpty(startDate))
        {
            srData = srData.Where(x => x.Date >= Convert.ToDateTime(startDate)).ToList();
            srDetailData = srDetailData.Where(x => x.Date >= Convert.ToDateTime(startDate)).ToList();
        }
        else if (!string.IsNullOrEmpty(endDate))
        {
            srData = srData.Where(x => x.Date <= Convert.ToDateTime(endDate)).ToList();
            srDetailData = srDetailData.Where(x => x.Date <= Convert.ToDateTime(endDate)).ToList();
        }

        if (!isDetail)
        {
            if (type == 1)
            {
                if (categoryId.HasValue || categoryId > 0)
                {
                    srDetailData = srDetailData.Where(x => itemCategoryData.Select(y => y.CategoryId).Contains(x.CategoryId)).ToList();
                    if (!itemId.HasValue || itemId <= 0)
                        srData = srData.Where(x => srDetailData.Select(y => y.Code).Contains(x.Code)).ToList();
                }

                if (itemId.HasValue || itemId > 0)
                {
                    srData = srData.Where(x => srDetailData.Select(y => y.Code).Contains(x.Code)).ToList();
                }

                if (!string.IsNullOrWhiteSpace(custCode))
                {
                    srData = srData.Where(x => x.CustCode == custCode).ToList();
                }

                srData = srData.OrderBy(x => x.Date).ToList();

                srData.Add(new Entity.Sales.ReportBySR
                {
                    Code = "Total",
                    SubTotal = srData.Sum(x => x.SubTotal),
                    Dpp = srData.Sum(x => x.Dpp),
                    TaxAmount = srData.Sum(x => x.TaxAmount),
                    Total = srData.Sum(x => x.Total)
                });

                return srData.AsQueryable().ToDataSourceResult(0, srData.Count, null, null);
            }
            else if (type == 2)
            {
                if (!string.IsNullOrEmpty(status))
                {
                    srDetailData = srDetailData.Where(x => srData.Select(y => y.Code).Contains(x.Code)).ToList();
                }

                if (!string.IsNullOrWhiteSpace(custCode))
                {
                    custData = custData.Where(x => x.Code == custCode).ToList();
                }

                if (categoryId.HasValue || categoryId > 0)
                {
                    srDetailData = srDetailData.Where(x => itemCategoryData.Select(y => y.CategoryId).Contains(x.CategoryId)).ToList();
                }

                foreach (var itemCust in custData)
                {
                    itemCust.TotalTrans = srDetailData.Count(x => x.CustCode == itemCust.Code);
                    itemCust.SubTotal = srDetailData.Where(x => x.CustCode == itemCust.Code).Sum(x => x.TotalGrossAmount);
                    itemCust.Dpp = srDetailData.Where(x => x.CustCode == itemCust.Code).Sum(x => x.TotalDpp);
                    itemCust.TaxAmount = srDetailData.Where(x => x.CustCode == itemCust.Code).Sum(x => x.TotalTaxAmount);
                    itemCust.Total = srDetailData.Where(x => x.CustCode == itemCust.Code).Sum(x => x.TotalNettPrice);
                }

                custData = custData.Where(x => x.TotalTrans > 0).OrderBy(x => x.Code).ToList();

                custData.Add(new Entity.Sales.ReportByCustomerSales
                {
                    Name = "Total",
                    TotalTrans = custData.Sum(x => x.TotalTrans),
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
                    srDetailData = srDetailData.Where(x => srData.Select(y => y.Code).Contains(x.Code)).ToList();
                }

                if (!string.IsNullOrWhiteSpace(custCode))
                {
                    srDetailData = srDetailData.Where(x => x.CustCode == custCode).ToList();
                }

                if (categoryId.HasValue || categoryId > 0)
                {
                    srDetailData = srDetailData.Where(x => itemCategoryData.Select(y => y.CategoryId).Contains(x.CategoryId)).ToList();
                }

                foreach (var item in itemData)
                {
                    item.TotalTrans = srDetailData.Count(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId);
                    item.Qty = srDetailData.Where(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId).Sum(x => x.Qty);
                    item.SubTotal = srDetailData.Where(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId).Sum(x => x.TotalGrossAmount);
                    item.Dpp = srDetailData.Where(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId).Sum(x => x.TotalDpp);
                    item.TaxAmount = srDetailData.Where(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId).Sum(x => x.TotalTaxAmount);
                    item.Total = srDetailData.Where(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId).Sum(x => x.TotalNettPrice);
                }

                itemData = itemData.Where(x => x.TotalTrans > 0).OrderBy(x => x.Initial).ToList();

                itemData.Add(new Entity.Sales.ReportByItemSales
                {
                    Name = "Total",
                    TotalTrans = itemData.Sum(x => x.TotalTrans),
                    Qty = itemData.Sum(x => x.Qty),
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
                    srDetailData = srDetailData.Where(x => srData.Select(y => y.Code).Contains(x.Code)).ToList();
                }

                if (!string.IsNullOrWhiteSpace(custCode))
                {
                    srDetailData = srDetailData.Where(x => x.CustCode == custCode).ToList();
                }

                foreach (var item in itemCategoryData)
                {
                    item.TotalTrans = srDetailData.Count(x => x.CategoryId == item.CategoryId && x.UnitId == item.UnitId);
                    item.Qty = srDetailData.Where(x => x.CategoryId == item.CategoryId && x.UnitId == item.UnitId).Sum(x => x.Qty);
                    item.SubTotal = srDetailData.Where(x => x.CategoryId == item.CategoryId && x.UnitId == item.UnitId).Sum(x => x.TotalGrossAmount);
                    item.Dpp = srDetailData.Where(x => x.CategoryId == item.CategoryId && x.UnitId == item.UnitId).Sum(x => x.TotalDpp);
                    item.TaxAmount = srDetailData.Where(x => x.CategoryId == item.CategoryId && x.UnitId == item.UnitId).Sum(x => x.TotalTaxAmount);
                    item.Total = srDetailData.Where(x => x.CategoryId == item.CategoryId && x.UnitId == item.UnitId).Sum(x => x.TotalNettPrice);
                }

                itemCategoryData = itemCategoryData.Where(x => x.TotalTrans > 0).OrderBy(x => x.Initial).ToList();

                itemCategoryData.Add(new Entity.Sales.ReportByItemCategorySales
                {
                    Name = "Total",
                    TotalTrans = itemCategoryData.Sum(x => x.TotalTrans),
                    Qty = itemCategoryData.Sum(x => x.Qty),
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
                    srDetailData = srDetailData.Where(x => srData.Select(y => y.Code).Contains(x.Code)).ToList();
                }

                if (!string.IsNullOrWhiteSpace(custCode))
                {
                    srDetailData = srDetailData.Where(x => x.CustCode == custCode).ToList();
                }

                if (categoryId.HasValue || categoryId > 0)
                {
                    srDetailData = srDetailData.Where(x => itemCategoryData.Select(y => y.CategoryId).Contains(x.CategoryId)).ToList();
                }

                if (unitId.HasValue || unitId > 0)
                {
                    srDetailData = srDetailData.Where(x => x.UnitId == unitId).ToList();
                }

                srDetailData = srDetailData.OrderBy(x => x.Date).ToList();

                srDetailData.Add(new Entity.Sales.ReportByDetailSR
                {
                    Code = "Total",
                    Qty = srDetailData.Sum(x => x.Qty),
                    TotalGrossAmount = srDetailData.Sum(x => x.TotalGrossAmount),
                    TotalDpp = srDetailData.Sum(x => x.TotalDpp),
                    TotalTaxAmount = srDetailData.Sum(x => x.TotalTaxAmount),
                    TotalNettPrice = srDetailData.Sum(x => x.TotalNettPrice)
                });

                return srDetailData.AsQueryable().ToDataSourceResult(0, srDetailData.Count, null, null);
            }
        }
        else
        {
            if (type == 1)
            {
                if (!string.IsNullOrWhiteSpace(code))
                {
                    srDetailData = srDetailData.Where(x => x.Code == code).ToList();
                }

                srDetailData = srDetailData.OrderBy(x => x.Date).ToList();

                srDetailData.Add(new Entity.Sales.ReportByDetailSR
                {
                    Code = "Total",
                    Qty = srDetailData.Sum(x => x.Qty),
                    TotalGrossAmount = srDetailData.Sum(x => x.TotalGrossAmount),
                    TotalDpp = srDetailData.Sum(x => x.TotalDpp),
                    TotalTaxAmount = srDetailData.Sum(x => x.TotalTaxAmount),
                    TotalNettPrice = srDetailData.Sum(x => x.TotalNettPrice)
                });

                return srDetailData.AsQueryable().ToDataSourceResult(0, srDetailData.Count, null, null);
            }
            else
            {
                if (!string.IsNullOrEmpty(status))
                {
                    srDetailData = srDetailData.Where(x => srData.Select(y => y.Code).Contains(x.Code)).ToList();
                }

                if (!string.IsNullOrWhiteSpace(custCode))
                {
                    srDetailData = srDetailData.Where(x => x.CustCode == custCode).ToList();
                }

                if (categoryId.HasValue || categoryId > 0)
                {
                    srDetailData = srDetailData.Where(x => itemCategoryData.Select(y => y.CategoryId).Contains(x.CategoryId)).ToList();
                }

                if (unitId.HasValue || unitId > 0)
                {
                    srDetailData = srDetailData.Where(x => x.UnitId == unitId).ToList();
                }

                srDetailData = srDetailData.OrderBy(x => x.Date).ToList();

                srDetailData.Add(new Entity.Sales.ReportByDetailSR
                {
                    Code = "Total",
                    Qty = srDetailData.Sum(x => x.Qty),
                    TotalGrossAmount = srDetailData.Sum(x => x.TotalGrossAmount),
                    TotalDpp = srDetailData.Sum(x => x.TotalDpp),
                    TotalTaxAmount = srDetailData.Sum(x => x.TotalTaxAmount),
                    TotalNettPrice = srDetailData.Sum(x => x.TotalNettPrice)
                });

                return srDetailData.AsQueryable().ToDataSourceResult(0, srDetailData.Count, null, null);
            }
        }
    }
}