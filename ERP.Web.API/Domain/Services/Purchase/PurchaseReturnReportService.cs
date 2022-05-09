using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Purchase;
using Microsoft.EntityFrameworkCore;

namespace ERP.Web.API.Domain.Services.Purchase;

public class PurchaseReturnReportService : IPurchaseReturnReportService
{
    private readonly TenantContext _db;
    public PurchaseReturnReportService(TenantContext db)
    {
        _db = db;
    }

    public DataSourceResult GetData(int type, string startDate, string endDate, string supCode, string status, int? itemId, string code, bool isDetail, int? unitId, int? categoryId)
    {
        var prData = _db.ReportByPRs.FromSqlRaw(@"SELECT pr.[Date], pr.Code, pr.SupCode,
                            pr.SupName, SUM(pr_d.UnitPrice * pr_d.Qty) AS GrossAmount,
                            pr.DPP, pr.TaxAmount, pr.Total,
                            CASE pr.[Type] 
	                            When '1' THEN 'Tukar Memo'
	                            When '2' THEN 'Tukar Barang Sama'
	                            When '3' THEN 'Tukar Barang Beda' End As [Type],
                            CASE pr.Mark
	                            WHEN 'A' THEN 'Aktif'
	                            WHEN 'V' THEN 'Void'
	                            WHEN 'PR' THEN 'Diterima Sebagian'
	                            WHEN 'CMP' THEN 'Diterima Seluruhnya'
	                            WHEN 'CLS' THEN 'Ditutup' END AS [Status]
                            FROM Purchasing.vwPurchaseReturnHeader pr
                            LEFT JOIN Purchasing.vwPurchaseReturnDetail pr_d ON pr.Code = pr_d.Code" +
                                                (string.IsNullOrEmpty(status) ? "" : status.Replace("'", "''").Equals("NV") ? " WHERE pr.Mark != 'V'" : $" WHERE pr.Mark = '{status.Replace("'", "''")}'") +
                                                @" GROUP BY pr.[Date], pr.Code, pr.SupCode, pr.SupName, 
                            pr.DPP, pr.TaxAmount, pr.Total, pr.[Type], pr.Mark").ToList();

        var prDetailData = _db.ReportByDetailPRs.FromSqlRaw(@"SELECT pr.[Date], pr.Code, pr.SupCode, pr.SupName,
                            im.Initial AS ItemInitial, im.[Name] AS ItemName, wh.[Name] AS WarehouseName, pr_d.Qty,
                            pr_d.UnitId, pr_d.UnitName, pr_d.UnitPrice AS GrossAmount,
                            pr_d.DPP, pr_d.TaxAmount, pr_d.NettPrice,
                            pr_d.UnitPrice * pr_d.Qty AS TotalGrossAmount, pr_d.DPP * pr_d.Qty AS TotalDPP,
                            pr_d.TaxAmount * pr_d.Qty AS TotalTaxAmount, pr_d.NettPrice * pr_d.Qty AS TotalNettPrice,
                            CASE pr.[Type] 
	                            When '1' THEN 'Tukar Memo'
	                            When '2' THEN 'Tukar Barang Sama'
	                            When '3' THEN 'Tukar Barang Beda' End As [Type],
                            CASE pr.Mark
	                            WHEN 'A' THEN 'Aktif'
	                            WHEN 'V' THEN 'Void'
	                            WHEN 'PR' THEN 'Diterima Sebagian'
	                            WHEN 'CMP' THEN 'Diterima Seluruhnya'
	                            WHEN 'CLS' THEN 'Ditutup' END AS [Status],
                            ic.Id AS CategoryId, ic.Initial AS CategoryInitial
                            FROM Purchasing.vwPurchaseReturnDetail pr_d
                            LEFT JOIN Purchasing.vwPurchaseReturnHeader pr ON pr.Code = pr_d.Code
                            LEFT JOIN Inventory.Item im ON im.Id = pr_d.ItemId
                            LEFT JOIN Inventory.Warehouse wh ON wh.Code = pr_d.WarehouseCode
                            LEFT JOIN Inventory.ItemCategory ic ON ic.Id = im.CategoryId" +
                                                            (!itemId.HasValue || itemId <= 0 ? "" : $" WHERE pr_d.ItemId = {itemId}")).ToList();

        var itemData = _db.ReportByItemPurchases.FromSqlRaw(@"SELECT im.Initial, im.[Name], 
                            ic.Id AS CategoryId, ic.Initial AS CategoryInitial,
                            uc.Id AS UnitId, uc.UnitEquivalent AS UnitName,
                            CAST (0 AS int) AS TotalTrans, CAST (0 AS decimal) AS Qty, CAST (0 AS decimal) AS SubTotal,
                            CAST (0 AS decimal) AS Disc, CAST (0 AS decimal) AS DiscHeader, CAST (0 AS decimal) AS Dpp,
                            CAST (0 AS decimal) AS TaxAmount, CAST (0 AS decimal) AS Total, CAST (0 AS decimal) AS GrossAmount
                            FROM Inventory.Item im
                            LEFT JOIN Purchasing.PurchaseReturnDetail rtn_d ON rtn_d.ItemId = im.Id
                            LEFT JOIN Inventory.ItemCategory ic ON ic.Id = im.CategoryId
                            LEFT JOIN Inventory.UoMConversion uc ON uc.Id = rtn_d.UnitId
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
                            LEFT JOIN Purchasing.PurchaseReturnDetail rtn_d ON rtn_d.ItemId = im.Id
                            LEFT JOIN Inventory.UoMConversion uc ON uc.Id = rtn_d.UnitId
                            WHERE uc.Id IS NOT NULL" +
                                                                            (!categoryId.HasValue || categoryId <= 0 ? "" : $" AND ic.Id = {categoryId}") +
                                                                            " GROUP BY ic.Id, ic.Initial, ic.[Name], uc.Id, uc.UnitEquivalent").ToList();

        if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
        {
            prData = prData.Where(x => x.Date >= Convert.ToDateTime(startDate) && x.Date <= Convert.ToDateTime(endDate)).ToList();
            prDetailData = prDetailData.Where(x => x.Date >= Convert.ToDateTime(startDate) && x.Date <= Convert.ToDateTime(endDate)).ToList();
        }
        else if (!string.IsNullOrEmpty(startDate))
        {
            prData = prData.Where(x => x.Date >= Convert.ToDateTime(startDate)).ToList();
            prDetailData = prDetailData.Where(x => x.Date >= Convert.ToDateTime(startDate)).ToList();
        }
        else if (!string.IsNullOrEmpty(endDate))
        {
            prData = prData.Where(x => x.Date <= Convert.ToDateTime(endDate)).ToList();
            prDetailData = prDetailData.Where(x => x.Date <= Convert.ToDateTime(endDate)).ToList();
        }

        if (!isDetail)
        {
            if (type == 1)
            {
                if (categoryId.HasValue || categoryId > 0)
                {
                    prDetailData = prDetailData.Where(x => itemCategoryData.Select(y => y.CategoryId).Contains(x.CategoryId)).ToList();
                    if (!itemId.HasValue || itemId <= 0)
                        prData = prData.Where(x => prDetailData.Select(y => y.Code).Contains(x.Code)).ToList();
                }

                if (itemId.HasValue || itemId > 0)
                {
                    prData = prData.Where(x => prDetailData.Select(y => y.Code).Contains(x.Code)).ToList();
                }

                if (!string.IsNullOrWhiteSpace(supCode))
                {
                    prData = prData.Where(x => x.SupCode == supCode).ToList();
                }

                prData = prData.OrderBy(x => x.Date).ToList();

                prData.Add(new Entity.Purchase.ReportByPR
                {
                    Code = "Total",
                    GrossAmount = prData.Sum(x => x.GrossAmount),
                    Dpp = prData.Sum(x => x.Dpp),
                    TaxAmount = prData.Sum(x => x.TaxAmount),
                    Total = prData.Sum(x => x.Total)
                });

                return prData.AsQueryable().ToDataSourceResult(0, prData.Count, null, null);
            }
            else if (type == 2)
            {
                if (!string.IsNullOrEmpty(status))
                {
                    prDetailData = prDetailData.Where(x => prData.Select(y => y.Code).Contains(x.Code)).ToList();
                }

                if (!string.IsNullOrWhiteSpace(supCode))
                {
                    supData = supData.Where(x => x.Code == supCode).ToList();
                }

                if (categoryId.HasValue || categoryId > 0)
                {
                    prDetailData = prDetailData.Where(x => itemCategoryData.Select(y => y.CategoryId).Contains(x.CategoryId)).ToList();
                }

                foreach (var itemSup in supData)
                {
                    itemSup.TotalTrans = prDetailData.Count(x => x.SupCode == itemSup.Code);
                    itemSup.GrossAmount = prDetailData.Where(x => x.SupCode == itemSup.Code).Sum(x => x.TotalGrossAmount);
                    itemSup.Dpp = prDetailData.Where(x => x.SupCode == itemSup.Code).Sum(x => x.TotalDpp);
                    itemSup.TaxAmount = prDetailData.Where(x => x.SupCode == itemSup.Code).Sum(x => x.TotalTaxAmount);
                    itemSup.Total = prDetailData.Where(x => x.SupCode == itemSup.Code).Sum(x => x.TotalNettPrice);
                }

                supData = supData.Where(x => x.TotalTrans > 0).OrderBy(x => x.Code).ToList();

                supData.Add(new Entity.Purchase.ReportBySupplierPurchase
                {
                    Name = "Total",
                    TotalTrans = supData.Sum(x => x.TotalTrans),
                    GrossAmount = supData.Sum(x => x.GrossAmount),
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
                    prDetailData = prDetailData.Where(x => prData.Select(y => y.Code).Contains(x.Code)).ToList();
                }

                if (!string.IsNullOrWhiteSpace(supCode))
                {
                    prDetailData = prDetailData.Where(x => x.SupCode == supCode).ToList();
                }

                if (categoryId.HasValue || categoryId > 0)
                {
                    prDetailData = prDetailData.Where(x => itemCategoryData.Select(y => y.CategoryId).Contains(x.CategoryId)).ToList();
                }

                foreach (var item in itemData)
                {
                    item.TotalTrans = prDetailData.Count(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId);
                    item.Qty = prDetailData.Where(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId).Sum(x => x.Qty);
                    item.GrossAmount = prDetailData.Where(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId).Sum(x => x.TotalGrossAmount);
                    item.Dpp = prDetailData.Where(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId).Sum(x => x.TotalDpp);
                    item.TaxAmount = prDetailData.Where(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId).Sum(x => x.TotalTaxAmount);
                    item.Total = prDetailData.Where(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId).Sum(x => x.TotalNettPrice);
                }

                itemData = itemData.Where(x => x.TotalTrans > 0).OrderBy(x => x.Initial).ToList();

                itemData.Add(new Entity.Purchase.ReportByItemPurchase
                {
                    Name = "Total",
                    TotalTrans = itemData.Sum(x => x.TotalTrans),
                    Qty = itemData.Sum(x => x.Qty),
                    GrossAmount = itemData.Sum(x => x.GrossAmount),
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
                    prDetailData = prDetailData.Where(x => prData.Select(y => y.Code).Contains(x.Code)).ToList();
                }

                if (!string.IsNullOrWhiteSpace(supCode))
                {
                    prDetailData = prDetailData.Where(x => x.SupCode == supCode).ToList();
                }

                foreach (var item in itemCategoryData)
                {
                    item.TotalTrans = prDetailData.Count(x => x.CategoryId == item.CategoryId && x.UnitId == item.UnitId);
                    item.Qty = prDetailData.Where(x => x.CategoryId == item.CategoryId && x.UnitId == item.UnitId).Sum(x => x.Qty);
                    item.GrossAmount = prDetailData.Where(x => x.CategoryId == item.CategoryId && x.UnitId == item.UnitId).Sum(x => x.TotalGrossAmount);
                    item.Dpp = prDetailData.Where(x => x.CategoryId == item.CategoryId && x.UnitId == item.UnitId).Sum(x => x.TotalDpp);
                    item.TaxAmount = prDetailData.Where(x => x.CategoryId == item.CategoryId && x.UnitId == item.UnitId).Sum(x => x.TotalTaxAmount);
                    item.Total = prDetailData.Where(x => x.CategoryId == item.CategoryId && x.UnitId == item.UnitId).Sum(x => x.TotalNettPrice);
                }

                itemCategoryData = itemCategoryData.Where(x => x.TotalTrans > 0).OrderBy(x => x.Initial).ToList();

                itemCategoryData.Add(new Entity.Purchase.ReportByItemCategoryPurchase
                {
                    Name = "Total",
                    TotalTrans = itemCategoryData.Sum(x => x.TotalTrans),
                    Qty = itemCategoryData.Sum(x => x.Qty),
                    GrossAmount = itemCategoryData.Sum(x => x.GrossAmount),
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
                    prDetailData = prDetailData.Where(x => prData.Select(y => y.Code).Contains(x.Code)).ToList();
                }

                if (!string.IsNullOrWhiteSpace(supCode))
                {
                    prDetailData = prDetailData.Where(x => x.SupCode == supCode).ToList();
                }

                if (categoryId.HasValue || categoryId > 0)
                {
                    prDetailData = prDetailData.Where(x => itemCategoryData.Select(y => y.CategoryId).Contains(x.CategoryId)).ToList();
                }

                if (unitId.HasValue || unitId > 0)
                {
                    prDetailData = prDetailData.Where(x => x.UnitId == unitId).ToList();
                }

                prDetailData = prDetailData.OrderBy(x => x.Date).ToList();

                prDetailData.Add(new Entity.Purchase.ReportByDetailPR
                {
                    Code = "Total",
                    Qty = prDetailData.Sum(x => x.Qty),
                    TotalGrossAmount = prDetailData.Sum(x => x.TotalGrossAmount),
                    TotalDpp = prDetailData.Sum(x => x.TotalDpp),
                    TotalTaxAmount = prDetailData.Sum(x => x.TotalTaxAmount),
                    TotalNettPrice = prDetailData.Sum(x => x.TotalNettPrice)
                });

                return prDetailData.AsQueryable().ToDataSourceResult(0, prDetailData.Count, null, null);
            }
        }
        else
        {
            if (type == 1)
            {
                if (!string.IsNullOrWhiteSpace(code))
                {
                    prDetailData = prDetailData.Where(x => x.Code == code).ToList();
                }

                prDetailData = prDetailData.OrderBy(x => x.Date).ToList();

                prDetailData.Add(new Entity.Purchase.ReportByDetailPR
                {
                    Code = "Total",
                    Qty = prDetailData.Sum(x => x.Qty),
                    TotalGrossAmount = prDetailData.Sum(x => x.TotalGrossAmount),
                    TotalDpp = prDetailData.Sum(x => x.TotalDpp),
                    TotalTaxAmount = prDetailData.Sum(x => x.TotalTaxAmount),
                    TotalNettPrice = prDetailData.Sum(x => x.TotalNettPrice)
                });

                return prDetailData.AsQueryable().ToDataSourceResult(0, prDetailData.Count, null, null);
            }
            else
            {
                if (!string.IsNullOrEmpty(status))
                {
                    prDetailData = prDetailData.Where(x => prData.Select(y => y.Code).Contains(x.Code)).ToList();
                }

                if (!string.IsNullOrWhiteSpace(supCode))
                {
                    prDetailData = prDetailData.Where(x => x.SupCode == supCode).ToList();
                }

                if (categoryId.HasValue || categoryId > 0)
                {
                    prDetailData = prDetailData.Where(x => itemCategoryData.Select(y => y.CategoryId).Contains(x.CategoryId)).ToList();
                }

                if (unitId.HasValue || unitId > 0)
                {
                    prDetailData = prDetailData.Where(x => x.UnitId == unitId).ToList();
                }

                prDetailData = prDetailData.OrderBy(x => x.Date).ToList();

                prDetailData.Add(new Entity.Purchase.ReportByDetailPR
                {
                    Code = "Total",
                    Qty = prDetailData.Sum(x => x.Qty),
                    TotalGrossAmount = prDetailData.Sum(x => x.TotalGrossAmount),
                    TotalDpp = prDetailData.Sum(x => x.TotalDpp),
                    TotalTaxAmount = prDetailData.Sum(x => x.TotalTaxAmount),
                    TotalNettPrice = prDetailData.Sum(x => x.TotalNettPrice)
                });

                return prDetailData.AsQueryable().ToDataSourceResult(0, prDetailData.Count, null, null);
            }
        }
    }
}