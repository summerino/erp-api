using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Purchase;
using Microsoft.EntityFrameworkCore;

namespace ERP.Web.API.Domain.Services.Purchase
{
    public class PurchaseOrderReportService : IPurchaseOrderReportService
    {
        private readonly TenantContext _db;
        public PurchaseOrderReportService(TenantContext db)
        {
            _db = db;
        }
        public DataSourceResult GetData(int type, string startDate, string endDate, string supCode, string status, int? itemId, string code, bool isDetail, int? unitId, int? categoryId)
        {
            var poData = _db.ReportByPOs.FromSqlRaw(@"SELECT po.[Date], po.Code, po.SupCode,
                            po.SupName, SUM(po_d.Qty * po_d.UnitPrice) AS SubTotal, SUM((po_d.Disc * po_d.Qty) + (po_d.FinalDiscHeader * po_d.Qty)) AS Disc, 
                            po.DPP, po.TaxAmount, po.Total,
                            CASE po.Mark
	                            WHEN 'A' THEN 'Aktif'
	                            WHEN 'V' THEN 'Void'
	                            WHEN 'PR' THEN 'Diterima Sebagian'
	                            WHEN 'CMP' THEN 'Diterima Seluruhnya'
	                            WHEN 'CLS' THEN 'Ditutup' END AS [Status]
                            FROM Purchasing.vwPurchaseOrderHeader po
                            LEFT JOIN Purchasing.vwPurchaseOrderDetail po_d ON po.Code = po_d.Code" +
                            (string.IsNullOrEmpty(status) ? "" : $" WHERE po.Mark = '{status.Replace("'", "''")}'") +
                            " GROUP BY po.[Date], po.Code, po.SupCode, po.SupName, po.SubTotal, po.DPP, po.TaxAmount, po.Total, po.Mark").ToList();

            var poDetailData = _db.ReportByDetailPOs.FromSqlRaw(@"SELECT po.[Date], po.Code, po.SupCode, po.SupName,
                            im.Initial AS ItemInitial, im.[Name] AS ItemName, po_d.Qty,
                            po_d.UnitPrice * po_d.Qty AS SubTotal, po_d.UnitId, po_d.UnitName,
                            po_d.Disc * po_d.Qty AS Disc, po_d.FinalDiscHeader * po_d.Qty AS DiscHeader,
                            po_d.DPP * po_d.Qty AS DPP, po_d.TaxAmount * po_d.Qty AS TaxAmount, po_d.Total,
                            CASE po.Mark
	                            WHEN 'A' THEN 'Aktif'
	                            WHEN 'V' THEN 'Void'
	                            WHEN 'PR' THEN 'Diterima Sebagian'
	                            WHEN 'CMP' THEN 'Diterima Seluruhnya'
	                            WHEN 'CLS' THEN 'Ditutup' END AS [Status],
                            ic.Id AS CategoryId, ic.Initial AS CategoryInitial
                            FROM Purchasing.vwPurchaseOrderDetail po_d
                            LEFT JOIN Purchasing.vwPurchaseOrderHeader po ON po.Code = po_d.Code
                            LEFT JOIN Inventory.Item im ON im.Id = po_d.ItemId
                            LEFT JOIN Inventory.ItemCategory ic ON ic.Id = im.CategoryId" +
                            (!itemId.HasValue || itemId <= 0  ? "" : $" WHERE po_d.ItemId = {itemId}")).ToList();

            var itemData = _db.ReportByItemPurchases.FromSqlRaw(@"SELECT im.Initial, im.[Name], 
                            ic.Id AS CategoryId, ic.Initial AS CategoryInitial,
                            uc.Id AS UnitId, uc.UnitEquivalent AS UnitName,
                            CAST (0 AS int) AS TotalTrans, CAST (0 AS decimal) AS Qty, CAST (0 AS decimal) AS SubTotal,
                            CAST (0 AS decimal) AS Disc, CAST (0 AS decimal) AS DiscHeader, CAST (0 AS decimal) AS Dpp,
                            CAST (0 AS decimal) AS TaxAmount, CAST (0 AS decimal) AS Total
                            FROM Inventory.Item im
                            LEFT JOIN Purchasing.PurchaseOrderDetail po_d ON po_d.ItemId = im.Id
                            LEFT JOIN Inventory.ItemCategory ic ON ic.Id = im.CategoryId
                            LEFT JOIN Inventory.UoMConversion uc ON uc.Id = po_d.UnitId
                            WHERE uc.Id IS NOT NULL
                            GROUP BY im.Initial, im.[Name], ic.Id, ic.Initial, uc.Id, uc.UnitEquivalent").ToList();

            var supData = _db.ReportBySupplierPurchases.FromSqlRaw(@"SELECT sp.Code, sp.[Name], CAST (0 AS int) AS TotalTrans, CAST (0 AS decimal) AS SubTotal,
                            CAST (0 AS decimal) AS Disc, CAST (0 AS decimal) AS Dpp, CAST (0 AS decimal) AS TaxAmount, CAST (0 AS decimal) AS Total
                            FROM General.Supplier sp
                            WHERE sp.IsActive = 1
                            GROUP BY sp.Code, sp.[Name]").ToList();

            var itemCategoryData = _db.ReportByItemCategoryPurchases.FromSqlRaw(@"SELECT ic.Id AS CategoryId, ic.Initial, ic.[Name],
                            uc.Id AS UnitId, uc.UnitEquivalent AS UnitName,
                            CAST (0 AS int) AS TotalTrans, CAST (0 AS decimal) AS Qty, CAST (0 AS decimal) AS SubTotal,
                            CAST (0 AS decimal) AS Disc, CAST (0 AS decimal) AS DiscHeader, CAST (0 AS decimal) AS Dpp,
                            CAST (0 AS decimal) AS TaxAmount, CAST (0 AS decimal) AS Total
                            FROM Inventory.ItemCategory ic
                            LEFT JOIN Inventory.Item im ON im.CategoryId = ic.Id 
                            LEFT JOIN Purchasing.PurchaseOrderDetail po_d ON po_d.ItemId = im.Id
                            LEFT JOIN Inventory.UoMConversion uc ON uc.Id = po_d.UnitId
                            WHERE uc.Id IS NOT NULL" +
                            (!categoryId.HasValue || categoryId <= 0 ? "" : $" AND ic.Id = {categoryId}") +
                            " GROUP BY ic.Id, ic.Initial, ic.[Name], uc.Id, uc.UnitEquivalent").ToList();

            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                poData = poData.Where(x => x.Date >= Convert.ToDateTime(startDate) && x.Date <= Convert.ToDateTime(endDate)).ToList();
                poDetailData = poDetailData.Where(x => x.Date >= Convert.ToDateTime(startDate) && x.Date <= Convert.ToDateTime(endDate)).ToList();
            }
            else if (!string.IsNullOrEmpty(startDate))
            {
                poData = poData.Where(x => x.Date >= Convert.ToDateTime(startDate)).ToList();
                poDetailData = poDetailData.Where(x => x.Date >= Convert.ToDateTime(startDate)).ToList();
            }
            else if (!string.IsNullOrEmpty(endDate))
            {
                poData = poData.Where(x => x.Date <= Convert.ToDateTime(endDate)).ToList();
                poDetailData = poDetailData.Where(x => x.Date <= Convert.ToDateTime(endDate)).ToList();
            }

            if (!isDetail)
            {
                if (type == 1)
                {
                    if (categoryId.HasValue || categoryId > 0)
                    {
                        poDetailData = poDetailData.Where(x => itemCategoryData.Select(y => y.CategoryId).Contains(x.CategoryId)).ToList();
                        if (!itemId.HasValue || itemId <= 0)
                            poData = poData.Where(x => poDetailData.Select(y => y.Code).Contains(x.Code)).ToList();
                    }

                    if (itemId.HasValue || itemId > 0)
                    {
                        poData = poData.Where(x => poDetailData.Select(y => y.Code).Contains(x.Code)).ToList();
                    }

                    if (!string.IsNullOrWhiteSpace(supCode))
                    {
                        poData = poData.Where(x => x.SupCode == supCode).ToList();
                    }

                    poData = poData.OrderBy(x => x.Date).ToList();

                    poData.Add(new Entity.Purchase.ReportByPO
                    {
                        Code = "Total",
                        SubTotal = poData.Sum(x => x.SubTotal),
                        Disc = poData.Sum(x => x.Disc),
                        Dpp = poData.Sum(x => x.Dpp),
                        TaxAmount = poData.Sum(x => x.TaxAmount),
                        Total = poData.Sum(x => x.Total)
                    });

                    return poData.AsQueryable().ToDataSourceResult(0, poData.Count, null, null);
                }
                else if (type == 2)
                {
                    if (!string.IsNullOrEmpty(status))
                    {
                        poDetailData = poDetailData.Where(x => poData.Select(y => y.Code).Contains(x.Code)).ToList();
                    }

                    if (!string.IsNullOrWhiteSpace(supCode))
                    {
                        supData = supData.Where(x => x.Code == supCode).ToList();
                    }

                    if (categoryId.HasValue || categoryId > 0)
                    {
                        poDetailData = poDetailData.Where(x => itemCategoryData.Select(y => y.CategoryId).Contains(x.CategoryId)).ToList();
                    }

                    foreach (var itemSup in supData)
                    {
                        itemSup.TotalTrans = poDetailData.Count(x => x.SupCode == itemSup.Code);
                        itemSup.SubTotal = poDetailData.Where(x => x.SupCode == itemSup.Code).Sum(x => x.SubTotal);
                        itemSup.Disc = poDetailData.Where(x => x.SupCode == itemSup.Code).Sum(x => x.Disc);
                        itemSup.Dpp = poDetailData.Where(x => x.SupCode == itemSup.Code).Sum(x => x.Dpp);
                        itemSup.TaxAmount = poDetailData.Where(x => x.SupCode == itemSup.Code).Sum(x => x.TaxAmount);
                        itemSup.Total = poDetailData.Where(x => x.SupCode == itemSup.Code).Sum(x => x.Total);
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
                        poDetailData = poDetailData.Where(x => poData.Select(y => y.Code).Contains(x.Code)).ToList();
                    }

                    if (!string.IsNullOrWhiteSpace(supCode))
                    {
                        poDetailData = poDetailData.Where(x => x.SupCode == supCode).ToList();
                    }

                    if (categoryId.HasValue || categoryId > 0)
                    {
                        poDetailData = poDetailData.Where(x => itemCategoryData.Select(y => y.CategoryId).Contains(x.CategoryId)).ToList();
                    }

                    foreach (var item in itemData)
                    {
                        item.TotalTrans = poDetailData.Count(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId);
                        item.Qty = poDetailData.Where(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId).Sum(x => x.Qty);
                        item.SubTotal = poDetailData.Where(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId).Sum(x => x.SubTotal);
                        item.Disc = poDetailData.Where(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId).Sum(x => x.Disc);
                        item.DiscHeader = poDetailData.Where(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId).Sum(x => x.DiscHeader);
                        item.Dpp = poDetailData.Where(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId).Sum(x => x.Dpp);
                        item.TaxAmount = poDetailData.Where(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId).Sum(x => x.TaxAmount);
                        item.Total = poDetailData.Where(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId).Sum(x => x.Total);
                    }

                    itemData = itemData.Where(x => x.TotalTrans > 0).OrderBy(x => x.Initial).ToList();

                    itemData.Add(new Entity.Purchase.ReportByItemPurchase
                    {
                        Name = "Total",
                        TotalTrans = itemData.Sum(x => x.TotalTrans),
                        Qty = itemData.Sum(x => x.Qty),
                        SubTotal = itemData.Sum(x => x.SubTotal),
                        Disc = itemData.Sum(x => x.Disc),
                        DiscHeader = itemData.Sum(x => x.DiscHeader),
                        Dpp = itemData.Sum(x => x.Dpp),
                        TaxAmount = itemData.Sum(x => x.TaxAmount),
                        Total = itemData.Sum(x => x.Total)
                    });

                    return itemData.AsQueryable().ToDataSourceResult(0, itemData.Count, null, null);
                }
                else
                {
                    if (!string.IsNullOrEmpty(status))
                    {
                        poDetailData = poDetailData.Where(x => poData.Select(y => y.Code).Contains(x.Code)).ToList();
                    }

                    if (!string.IsNullOrWhiteSpace(supCode))
                    {
                        poDetailData = poDetailData.Where(x => x.SupCode == supCode).ToList();
                    }

                    foreach (var item in itemCategoryData)
                    {
                        item.TotalTrans = poDetailData.Count(x => x.CategoryId == item.CategoryId && x.UnitId == item.UnitId);
                        item.Qty = poDetailData.Where(x => x.CategoryId == item.CategoryId && x.UnitId == item.UnitId).Sum(x => x.Qty);
                        item.SubTotal = poDetailData.Where(x => x.CategoryId == item.CategoryId && x.UnitId == item.UnitId).Sum(x => x.SubTotal);
                        item.Disc = poDetailData.Where(x => x.CategoryId == item.CategoryId && x.UnitId == item.UnitId).Sum(x => x.Disc);
                        item.DiscHeader = poDetailData.Where(x => x.CategoryId == item.CategoryId && x.UnitId == item.UnitId).Sum(x => x.DiscHeader);
                        item.Dpp = poDetailData.Where(x => x.CategoryId == item.CategoryId && x.UnitId == item.UnitId).Sum(x => x.Dpp);
                        item.TaxAmount = poDetailData.Where(x => x.CategoryId == item.CategoryId && x.UnitId == item.UnitId).Sum(x => x.TaxAmount);
                        item.Total = poDetailData.Where(x => x.CategoryId == item.CategoryId && x.UnitId == item.UnitId).Sum(x => x.Total);
                    }

                    itemCategoryData = itemCategoryData.Where(x => x.TotalTrans > 0).OrderBy(x => x.Initial).ToList();

                    itemCategoryData.Add(new Entity.Purchase.ReportByItemCategoryPurchase
                    {
                        Name = "Total",
                        TotalTrans = itemCategoryData.Sum(x => x.TotalTrans),
                        Qty = itemCategoryData.Sum(x => x.Qty),
                        SubTotal = itemCategoryData.Sum(x => x.SubTotal),
                        Disc = itemCategoryData.Sum(x => x.Disc),
                        DiscHeader = itemCategoryData.Sum(x => x.DiscHeader),
                        Dpp = itemCategoryData.Sum(x => x.Dpp),
                        TaxAmount = itemCategoryData.Sum(x => x.TaxAmount),
                        Total = itemCategoryData.Sum(x => x.Total)
                    });

                    return itemCategoryData.AsQueryable().ToDataSourceResult(0, itemCategoryData.Count, null, null);
                }
            }
            else
            {
                if (type == 1)
                {
                    if (!string.IsNullOrWhiteSpace(code))
                    {
                        poDetailData = poDetailData.Where(x => x.Code == code).ToList();
                    }

                    poDetailData = poDetailData.OrderBy(x => x.Date).ToList();

                    poDetailData.Add(new Entity.Purchase.ReportByDetailPO
                    {
                        Code = "Total",
                        Qty = poDetailData.Sum(x => x.Qty),
                        SubTotal = poDetailData.Sum(x => x.SubTotal),
                        Disc = poDetailData.Sum(x => x.Disc),
                        DiscHeader = poDetailData.Sum(x => x.DiscHeader),
                        Dpp = poDetailData.Sum(x => x.Dpp),
                        TaxAmount = poDetailData.Sum(x => x.TaxAmount),
                        Total = poDetailData.Sum(x => x.Total)
                    });

                    return poDetailData.AsQueryable().ToDataSourceResult(0, poDetailData.Count, null, null);
                }
                else
                {
                    if (!string.IsNullOrEmpty(status))
                    {
                        poDetailData = poDetailData.Where(x => poData.Select(y => y.Code).Contains(x.Code)).ToList();
                    }

                    if (!string.IsNullOrWhiteSpace(supCode))
                    {
                        poDetailData = poDetailData.Where(x => x.SupCode == supCode).ToList();
                    }

                    if (categoryId.HasValue || categoryId > 0)
                    {
                        poDetailData = poDetailData.Where(x => itemCategoryData.Select(y => y.CategoryId).Contains(x.CategoryId)).ToList();
                    }

                    if (unitId.HasValue || unitId > 0)
                    {
                        poDetailData = poDetailData.Where(x => x.UnitId == unitId).ToList();
                    }

                    poDetailData = poDetailData.OrderBy(x => x.Date).ToList();

                    poDetailData.Add(new Entity.Purchase.ReportByDetailPO
                    {
                        Code = "Total",
                        Qty = poDetailData.Sum(x => x.Qty),
                        SubTotal = poDetailData.Sum(x => x.SubTotal),
                        Disc = poDetailData.Sum(x => x.Disc),
                        DiscHeader = poDetailData.Sum(x => x.DiscHeader),
                        Dpp = poDetailData.Sum(x => x.Dpp),
                        TaxAmount = poDetailData.Sum(x => x.TaxAmount),
                        Total = poDetailData.Sum(x => x.Total)
                    });

                    return poDetailData.AsQueryable().ToDataSourceResult(0, poDetailData.Count, null, null);
                }
            }
        }
    }
}
