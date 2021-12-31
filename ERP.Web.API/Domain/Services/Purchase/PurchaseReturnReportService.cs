using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Purchase;
using Microsoft.EntityFrameworkCore;

namespace ERP.Web.API.Domain.Services.Purchase
{
    public class PurchaseReturnReportService : IPurchaseReturnReportService
    {
        private readonly TenantContext _db;
        public PurchaseReturnReportService(TenantContext db)
        {
            _db = db;
        }

        public DataSourceResult GetData(int type, string startDate, string endDate, string supCode, string status, int? itemId, string code, bool isDetail)
        {
            var prData = _db.ReportByPRs.FromSqlRaw(@"SELECT pr.[Date], pr.Code, pr.SupCode,
                            pr.SupName, pr.SubTotal,
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
                            FROM Purchasing.vwPurchaseReturnHeader pr" +
                        (string.IsNullOrEmpty(status) ? "" : $" WHERE pr.Mark = '{status.Replace("'", "''")}'")).ToList();

            var prDetailData = _db.ReportByDetailPRs.FromSqlRaw(@"SELECT pr.[Date], pr.Code, pr.SupCode, pr.SupName,
                                im.Initial AS ItemInitial, im.[Name] AS ItemName, wh.[Name] AS WarehouseName,
                                pr_d.Qty, pr_d.Total AS SubTotal, pr_d.UnitName AS Unit,
                                pr_d.DPP, pr_d.TaxAmount, pr_d.Total,
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
                                FROM Purchasing.vwPurchaseReturnDetail pr_d
                                LEFT JOIN Purchasing.vwPurchaseReturnHeader pr ON pr.Code = pr_d.Code
                                LEFT JOIN Inventory.Item im ON im.Id = pr_d.ItemId
                                LEFT JOIN Inventory.Warehouse wh ON wh.Code = pr_d.WarehouseCode" +
                        (!itemId.HasValue || itemId <= 0 ? "" : $" WHERE pr_d.ItemId = {itemId}")).ToList();

            var itemData = _db.ReportByItemPOs.FromSqlRaw(@"SELECT im.Initial, im.[Name], CAST (0 AS int) AS TotalTrans, CAST (0 AS decimal) AS Qty,
                        CAST (0 AS decimal) AS SubTotal, CAST (0 AS decimal) AS Disc, CAST (0 AS decimal) AS DiscHeader,
                        CAST (0 AS decimal) AS Dpp, CAST (0 AS decimal) AS TaxAmount, CAST (0 AS decimal) AS Total
                        FROM Inventory.Item im GROUP BY im.Initial, im.[Name]").ToList();

            var supData = _db.ReportBySupplierPOs.FromSqlRaw(@"SELECT sp.Code, sp.[Name], CAST (0 AS int) AS TotalTrans, CAST (0 AS decimal) AS SubTotal,
                        CAST (0 AS decimal) AS Disc, CAST (0 AS decimal) AS Dpp, CAST (0 AS decimal) AS TaxAmount, CAST (0 AS decimal) AS Total
                        FROM General.Supplier sp
                        WHERE sp.IsActive = 1
                        GROUP BY sp.Code, sp.[Name]").ToList();

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
                        SubTotal = prData.Sum(x => x.SubTotal),
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

                    foreach (var itemSup in supData)
                    {
                        itemSup.TotalTrans = prDetailData.Count(x => x.SupCode == itemSup.Code);
                        itemSup.SubTotal = prDetailData.Where(x => x.SupCode == itemSup.Code).Sum(x => x.SubTotal);
                        itemSup.Dpp = prDetailData.Where(x => x.SupCode == itemSup.Code).Sum(x => x.Dpp);
                        itemSup.TaxAmount = prDetailData.Where(x => x.SupCode == itemSup.Code).Sum(x => x.TaxAmount);
                        itemSup.Total = prDetailData.Where(x => x.SupCode == itemSup.Code).Sum(x => x.Total);
                    }

                    supData = supData.Where(x => x.TotalTrans > 0).OrderBy(x => x.Code).ToList();

                    supData.Add(new Entity.Purchase.ReportBySupplierPO
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

                    foreach (var item in itemData)
                    {
                        item.TotalTrans = prDetailData.Count(x => x.ItemInitial == item.Initial);
                        item.Qty = prDetailData.Where(x => x.ItemInitial == item.Initial).Sum(x => x.Qty);
                        item.SubTotal = prDetailData.Where(x => x.ItemInitial == item.Initial).Sum(x => x.SubTotal);
                        item.Dpp = prDetailData.Where(x => x.ItemInitial == item.Initial).Sum(x => x.Dpp);
                        item.TaxAmount = prDetailData.Where(x => x.ItemInitial == item.Initial).Sum(x => x.TaxAmount);
                        item.Total = prDetailData.Where(x => x.ItemInitial == item.Initial).Sum(x => x.Total);
                    }

                    itemData = itemData.Where(x => x.TotalTrans > 0).OrderBy(x => x.Initial).ToList();

                    itemData.Add(new Entity.Purchase.ReportByItemPO
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
                        SubTotal = prDetailData.Sum(x => x.SubTotal),
                        Dpp = prDetailData.Sum(x => x.Dpp),
                        TaxAmount = prDetailData.Sum(x => x.TaxAmount),
                        Total = prDetailData.Sum(x => x.Total)
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

                    prDetailData = prDetailData.OrderBy(x => x.Date).ToList();

                    prDetailData.Add(new Entity.Purchase.ReportByDetailPR
                    {
                        Code = "Total",
                        Qty = prDetailData.Sum(x => x.Qty),
                        SubTotal = prDetailData.Sum(x => x.SubTotal),
                        Dpp = prDetailData.Sum(x => x.Dpp),
                        TaxAmount = prDetailData.Sum(x => x.TaxAmount),
                        Total = prDetailData.Sum(x => x.Total)
                    });

                    return prDetailData.AsQueryable().ToDataSourceResult(0, prDetailData.Count, null, null);
                }
            }
        }
    }
}
