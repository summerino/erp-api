using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Sales;
using Microsoft.EntityFrameworkCore;

namespace ERP.Web.API.Domain.Services.Sales
{
    public class DeliveryPlanReportService : IDeliveryPlanReportService
    {
        private readonly TenantContext _db;
        public DeliveryPlanReportService(TenantContext db)
        {
            _db = db;
        }

        public DataSourceResult GetData(int type, string startDate, string endDate, string custCode, string status, int? itemId, string code, bool isDetail, int? unitId, int? categoryId)
        {
            var dpData = _db.ReportByDPs.FromSqlRaw(@"SELECT dp_h.[Date], dp_h.Code, dp_h.WarehouseCode, dp_h.WarehouseInitial, so_h.CustCode,
                        im.Initial AS ItemInitial, im.[Name] AS ItemName, 
                        ic.Id AS ItemCategoryId, ic.Initial AS ItemCategoryInitial,
                        dlv_d.UnitId, dlv_d.UnitName,	
                        SUM(dlv_d.Total + ISNULL((dlv_df.UnitPrice * dlv_df.Qty), 0)) AS Total, SUM(dlv_d.Qty + ISNULL(dlv_df.Qty, 0)) AS Qty,
                        CASE dp_h.Mark
                        WHEN 'A' THEN 'Aktif'
                        WHEN 'V' THEN 'Void' END AS [Status]
                        FROM Sales.vwDeliveryPlanHeader dp_h
                        LEFT JOIN Sales.DeliveryPlanDetail dp_d ON dp_h.Code = dp_d.Code
                        LEFT JOIN Sales.vwSalesDeliveryHeader dlv_h ON dlv_h.Code = dp_d.TransCode
                        LEFT JOIN Sales.VwSalesDeliveryDetail dlv_d ON dlv_d.Code = dlv_h.Code
                        LEFT JOIN Sales.SalesDeliveryDetailFreeGood dlv_df ON dlv_df.Code = dlv_h.Code
                        LEFT JOIN Sales.vwSalesOrderHeader so_h ON so_h.Code = dlv_h.TransCode
                        LEFT JOIN Inventory.Item im ON im.Id = dlv_d.ItemId
                        LEFT JOIN Inventory.ItemCategory ic ON ic.Id = im.CategoryId" +
                        (string.IsNullOrEmpty(status) ? "" : status.Replace("'", "''").Equals("NAPR") ? " WHERE dp_h.Mark = 'A' AND dp_h.ApprovedBy IS NULL" :
                        status.Replace("'", "''").Equals("APR") ? " WHERE dp_h.Mark = 'A' AND dp_h.ApprovedBy IS NOT NULL" : $" WHERE dp_h.Mark = '{status.Replace("'", "''")}'") +
                        @" GROUP BY dp_h.[Date], dp_h.Code, dp_h.WarehouseCode, dp_h.WarehouseInitial, so_h.CustCode,
                        im.Initial, im.[Name], ic.Id, ic.Initial,
                        dlv_d.UnitId, dlv_d.UnitName, dp_h.Mark").ToList();

            var dpDetailData = _db.ReportByDetailDPs.FromSqlRaw(@"SELECT dp_h.[Date], dp_h.Code, dp_h.WarehouseCode, dp_h.WarehouseInitial,
                            dp_d.TransCode, 'Normal' AS [Type], so_h.CustCode, so_h.CustName, so_h.SalesName,
                            im.Initial AS ItemInitial, im.[Name] AS ItemName, ic.Id AS ItemCategoryId, ic.Initial AS ItemCategoryInitial,
                            dlv_d.UnitId, dlv_d.UnitName,
                            SUM(dlv_d.Total) AS Total, SUM(dlv_d.Qty) AS Qty,
                            CASE dp_h.Mark
                            WHEN 'A' THEN 'Aktif'
                            WHEN 'V' THEN 'Void' END AS [Status]
                            FROM Sales.DeliveryPlanDetail dp_d
                            LEFT JOIN Sales.vwDeliveryPlanHeader dp_h ON dp_h.Code = dp_d.Code
                            LEFT JOIN Sales.vwSalesDeliveryHeader dlv_h ON dlv_h.Code = dp_d.TransCode
                            LEFT JOIN Sales.VwSalesDeliveryDetail dlv_d ON dlv_d.Code = dlv_h.Code
                            LEFT JOIN Sales.vwSalesOrderHeader so_h ON so_h.Code = dlv_h.TransCode
                            LEFT JOIN Inventory.Item im ON im.Id = dlv_d.ItemId
                            LEFT JOIN Inventory.ItemCategory ic ON ic.Id = im.CategoryId" +
                            (!itemId.HasValue || itemId <= 0 ? "" : $" WHERE dlv_d.ItemId = {itemId}") +
                            @" GROUP BY dp_h.[Date], dp_h.Code, dp_h.WarehouseCode, dp_h.WarehouseInitial,
                            dp_d.TransCode, so_h.CustCode, so_h.CustName, so_h.SalesName,
                            im.Initial, im.[Name], ic.Id, ic.Initial,
                            dlv_d.UnitId, dlv_d.UnitName, dp_h.Mark" +
                            @" UNION ALL
                            SELECT dp_h.[Date], dp_h.Code, dp_h.WarehouseCode, dp_h.WarehouseInitial,
                            dp_d.TransCode, 'Bonus' AS [Type], so_h.CustCode, so_h.CustName, so_h.SalesName,
                            im.Initial AS ItemInitial, im.[Name] AS ItemName, ic.Id AS ItemCategoryId, ic.Initial AS ItemCategoryInitial,
                            dlv_df.UnitId, uom.UnitEquivalent AS UnitName,
                            SUM(ISNULL(dlv_df.Qty * dlv_df.UnitPrice, 0)) AS Total, SUM(ISNULL(dlv_df.Qty, 0)) AS Qty,
                            CASE dp_h.Mark
                            WHEN 'A' THEN 'Aktif'
                            WHEN 'V' THEN 'Void' END AS [Status]
                            FROM Sales.DeliveryPlanDetail dp_d
                            LEFT JOIN Sales.vwDeliveryPlanHeader dp_h ON dp_h.Code = dp_d.Code
                            LEFT JOIN Sales.vwSalesDeliveryHeader dlv_h ON dlv_h.Code = dp_d.TransCode
                            LEFT JOIN Sales.SalesDeliveryDetailFreeGood dlv_df ON dlv_df.Code = dlv_h.Code
                            LEFT JOIN Sales.vwSalesOrderHeader so_h ON so_h.Code = dlv_h.TransCode
                            LEFT JOIN Inventory.Item im ON im.Id = dlv_df.ItemId
                            LEFT JOIN Inventory.ItemCategory ic ON ic.Id = im.CategoryId
                            LEFT JOIN Inventory.UoMConversion uom ON uom.Id = dlv_df.UnitId" +
                            (!itemId.HasValue || itemId <= 0 ? "" : $" WHERE dlv_df.ItemId = {itemId}") +
                            @" GROUP BY dp_h.[Date], dp_h.Code, dp_h.WarehouseCode, dp_h.WarehouseInitial,
                            dp_d.TransCode, so_h.CustCode, so_h.CustName, so_h.SalesName,
                            im.Initial, im.[Name], ic.Id, ic.Initial,
                            dlv_df.UnitId, uom.UnitEquivalent, dp_h.Mark").ToList();

            var itemData = _db.ReportByItemDPs.FromSqlRaw(@"SELECT im.Initial, im.[Name], 
                            ic.Id AS CategoryId, ic.Initial AS CategoryInitial,
                            uc.Id AS UnitId, uc.UnitEquivalent AS UnitName,
                            CAST (0 AS decimal) AS Qty, CAST (0 AS decimal) AS Total
                            FROM Inventory.Item im
                            LEFT JOIN Sales.DeliveryPlanDetailItem dp_di ON dp_di.ItemId = im.Id
                            LEFT JOIN Inventory.ItemCategory ic ON ic.Id = im.CategoryId
                            LEFT JOIN Inventory.UoMConversion uc ON uc.Id = dp_di.UnitId
                            WHERE uc.Id IS NOT NULL
                            GROUP BY im.Initial, im.[Name], ic.Id, ic.Initial, uc.Id, uc.UnitEquivalent").ToList();

            var itemCategoryData = _db.ReportByItemCategoryDPs.FromSqlRaw(@"
                            SELECT ic.Id AS CategoryId, ic.Initial, ic.[Name],
                            uc.Id AS UnitId, uc.UnitEquivalent AS UnitName,
                            CAST (0 AS decimal) AS Qty, CAST (0 AS decimal) AS Total
                            FROM Inventory.ItemCategory ic
                            LEFT JOIN Inventory.Item im ON im.CategoryId = ic.Id 
                            LEFT JOIN Sales.DeliveryPlanDetailItem dp_di ON dp_di.ItemId = im.Id
                            LEFT JOIN Inventory.UoMConversion uc ON uc.Id = dp_di.UnitId
                            WHERE uc.Id IS NOT NULL" +
                            (!categoryId.HasValue || categoryId <= 0 ? "" : $" AND ic.Id = {categoryId}") +
                            " GROUP BY ic.Id, ic.Initial, ic.[Name], uc.Id, uc.UnitEquivalent").ToList();

            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                dpData = dpData.Where(x => x.Date >= Convert.ToDateTime(startDate) && x.Date <= Convert.ToDateTime(endDate)).ToList();
                dpDetailData = dpDetailData.Where(x => x.Date >= Convert.ToDateTime(startDate) && x.Date <= Convert.ToDateTime(endDate)).ToList();
            }
            else if (!string.IsNullOrEmpty(startDate))
            {
                dpData = dpData.Where(x => x.Date >= Convert.ToDateTime(startDate)).ToList();
                dpDetailData = dpDetailData.Where(x => x.Date >= Convert.ToDateTime(startDate)).ToList();
            }
            else if (!string.IsNullOrEmpty(endDate))
            {
                dpData = dpData.Where(x => x.Date <= Convert.ToDateTime(endDate)).ToList();
                dpDetailData = dpDetailData.Where(x => x.Date <= Convert.ToDateTime(endDate)).ToList();
            }

            dpDetailData = dpDetailData.Where(x => x.Qty > 0 && x.Qty.HasValue).ToList();

            if (!isDetail)
            {
                if (type == 1)
                {
                    if (categoryId.HasValue || categoryId > 0)
                    {
                        dpDetailData = dpDetailData.Where(x => itemCategoryData.Select(y => y.CategoryId).Contains(x.ItemCategoryId.Value)).ToList();
                        if (!itemId.HasValue || itemId <= 0)
                            dpData = dpData.Where(x => dpDetailData.Select(y => y.ItemCategoryInitial).Contains(x.ItemCategoryInitial)).ToList();
                    }

                    if (itemId.HasValue || itemId > 0)
                    {
                        dpData = dpData.Where(x => dpDetailData.Select(y => y.ItemInitial).Contains(x.ItemInitial)).ToList();
                    }

                    if (!string.IsNullOrWhiteSpace(custCode))
                    {
                        dpData = dpData.Where(x => x.CustCode == custCode).ToList();
                    }

                    dpData = dpData.Where(x => x.Qty > 0 && x.Qty.HasValue).OrderBy(x => x.Date).ToList();

                    dpData.Add(new Entity.Sales.ReportByDP
                    {
                        Code = "Total",
                        Total = dpData.Sum(x => x.Total)
                    });

                    return dpData.AsQueryable().ToDataSourceResult(0, dpData.Count, null, null);
                }
                else if (type == 2)
                {
                    if (!string.IsNullOrEmpty(status))
                    {
                        dpDetailData = dpDetailData.Where(x => dpData.Select(y => y.Code).Contains(x.Code)).ToList();
                    }

                    if (!string.IsNullOrWhiteSpace(custCode))
                    {
                        dpDetailData = dpDetailData.Where(x => x.CustCode == custCode).ToList();
                    }

                    if (categoryId.HasValue || categoryId > 0)
                    {
                        dpDetailData = dpDetailData.Where(x => itemCategoryData.Select(y => y.CategoryId).Contains(x.ItemCategoryId.Value)).ToList();
                    }

                    foreach (var item in itemData)
                    {
                        item.Qty = dpDetailData.Where(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId).Sum(x => x.Qty.Value);
                        item.Total = dpDetailData.Where(x => x.ItemInitial == item.Initial && x.UnitId == item.UnitId).Sum(x => x.Total.Value);
                    }

                    itemData = itemData.Where(x => x.Total > 0 || x.Qty > 0).OrderBy(x => x.Initial).ToList();

                    itemData.Add(new Entity.Sales.ReportByItemDP
                    {
                        Name = "Total",
                        Total = itemData.Sum(x => x.Total)
                    });

                    return itemData.AsQueryable().ToDataSourceResult(0, itemData.Count, null, null);
                }
                else if (type == 3)
                {
                    if (!string.IsNullOrEmpty(status))
                    {
                        dpDetailData = dpDetailData.Where(x => dpData.Select(y => y.Code).Contains(x.Code)).ToList();
                    }

                    if (!string.IsNullOrWhiteSpace(custCode))
                    {
                        dpDetailData = dpDetailData.Where(x => x.CustCode == custCode).ToList();
                    }

                    foreach (var item in itemCategoryData)
                    {
                        item.Qty = dpDetailData.Where(x => x.ItemCategoryId.Value == item.CategoryId && x.UnitId == item.UnitId).Sum(x => x.Qty.Value);
                        item.Total = dpDetailData.Where(x => x.ItemCategoryId.Value == item.CategoryId && x.UnitId == item.UnitId).Sum(x => x.Total.Value);
                    }

                    itemCategoryData = itemCategoryData.Where(x => x.Total > 0 || x.Qty > 0).OrderBy(x => x.Initial).ToList();

                    itemCategoryData.Add(new Entity.Sales.ReportByItemCategoryDP
                    {
                        Name = "Total",
                        Total = itemCategoryData.Sum(x => x.Total)
                    });

                    return itemCategoryData.AsQueryable().ToDataSourceResult(0, itemCategoryData.Count, null, null);
                }
                else
                {
                    if (!string.IsNullOrEmpty(status))
                    {
                        dpDetailData = dpDetailData.Where(x => dpData.Select(y => y.Code).Contains(x.Code)).ToList();
                    }

                    if (!string.IsNullOrWhiteSpace(custCode))
                    {
                        dpDetailData = dpDetailData.Where(x => x.CustCode == custCode).ToList();
                    }

                    if (categoryId.HasValue || categoryId > 0)
                    {
                        dpDetailData = dpDetailData.Where(x => itemCategoryData.Select(y => y.CategoryId).Contains(x.ItemCategoryId.Value)).ToList();
                    }

                    if (unitId.HasValue || unitId > 0)
                    {
                        dpDetailData = dpDetailData.Where(x => x.UnitId == unitId).ToList();
                    }

                    dpDetailData = dpDetailData.OrderBy(x => x.Date).ToList();

                    return dpDetailData.AsQueryable().ToDataSourceResult(0, dpDetailData.Count, null, null);
                }
            }
            else
            {
                if (type == 1)
                {
                    if (!string.IsNullOrWhiteSpace(code))
                    {
                        dpDetailData = dpDetailData.Where(x => x.Code == code).ToList();
                    }

                    if (!string.IsNullOrWhiteSpace(custCode))
                    {
                        dpDetailData = dpDetailData.Where(x => x.CustCode == custCode).ToList();
                    }

                    dpDetailData = dpDetailData.OrderBy(x => x.Date).ToList();

                    return dpDetailData.AsQueryable().ToDataSourceResult(0, dpDetailData.Count, null, null);
                }
                else
                {
                    if (!string.IsNullOrEmpty(status))
                    {
                        dpDetailData = dpDetailData.Where(x => dpData.Select(y => y.Code).Contains(x.Code)).ToList();
                    }

                    if (!string.IsNullOrWhiteSpace(custCode))
                    {
                        dpDetailData = dpDetailData.Where(x => x.CustCode == custCode).ToList();
                    }

                    if (categoryId.HasValue || categoryId > 0)
                    {
                        dpDetailData = dpDetailData.Where(x => itemCategoryData.Select(y => y.CategoryId).Contains(x.ItemCategoryId.Value)).ToList();
                    }

                    if (unitId.HasValue || unitId > 0)
                    {
                        dpDetailData = dpDetailData.Where(x => x.UnitId == unitId).ToList();
                    }

                    dpDetailData = dpDetailData.OrderBy(x => x.Date).ToList();

                    return dpDetailData.AsQueryable().ToDataSourceResult(0, dpDetailData.Count, null, null);
                }
            }
        }
    }
}
