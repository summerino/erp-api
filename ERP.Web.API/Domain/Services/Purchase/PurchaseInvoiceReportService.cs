using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Purchase;
using Microsoft.EntityFrameworkCore;

namespace ERP.Web.API.Domain.Services.Purchase
{
    public class PurchaseInvoiceReportService : IPurchaseInvoiceReportService
    {
        private readonly TenantContext _db;
        public PurchaseInvoiceReportService(TenantContext db)
        {
            _db = db;
        }

        public DataSourceResult GetData(int type, string startDate, string endDate, string supCode, string status, int? itemId, string code, bool isDetail)
        {
            var invData = _db.ReportByPInvs.FromSqlRaw(@"SELECT inv.[Date], inv.DueDate, inv.Code,
                            inv.POCode AS OrderCode, inv.RefNo,  
                            inv.SupCode, inv.SupName, SUM(inv_d.SubTotal) AS SubTotal, SUM(inv_d.FinalDisc) AS Disc, 
                            SUM(inv_d.DPP) AS DPP, SUM(inv_d.TaxAmount) AS TaxAmount, SUM(inv_d.Total) AS Total,
                            CASE inv.Mark
	                            WHEN 'A' THEN 'Aktif'
	                            WHEN 'V' THEN 'Void'
	                            WHEN 'PP' THEN 'Aktif'
	                            WHEN 'CMP' THEN 'Aktif' END AS [Status]
                            FROM Purchasing.vwPurchaseInvoiceHeader inv
                            LEFT JOIN Purchasing.PurchaseInvoiceDetail inv_d on inv.Code = inv_d.Code" +
                            (string.IsNullOrEmpty(status) ? "" : status == "A" ? $" WHERE inv.Mark IN ('A','PP','CMP')" : $" WHERE inv.Mark = '{status.Replace("'", "''")}'") +
                            "GROUP BY inv.[Date], inv.DueDate, inv.Code, inv.POCode, inv.RefNo, inv.SupCode, inv.SupName, inv.Mark").ToList();

            var invDetailData = _db.ReportByDetailPInvs.FromSqlRaw(@"SELECT inv.[Date], inv.DueDate, inv.Code,
                            inv.POCode AS OrderCode, inv.RefNo,
                            inv.SupCode, inv.SupName, inv_d.RcvCode,
                            im.Initial AS ItemInitial, im.[Name] AS ItemName, rcv_d.Qty, 
                            rcv_d.UnitName AS Unit, rcv_d.Total AS SubTotal, rcv_d.Disc,
                            rcv_d.FinalDiscHeader AS DiscHeader, rcv_d.DPP, rcv_d.TaxAmount, rcv_d.Total,
                            CASE inv.Mark
	                            WHEN 'A' THEN 'Aktif'
	                            WHEN 'V' THEN 'Void'
	                            WHEN 'PP' THEN 'Aktif'
	                            WHEN 'CMP' THEN 'Aktif' END AS [Status]
                            FROM Purchasing.PurchaseInvoiceDetail inv_d 
                            LEFT JOIN Purchasing.vwPurchaseInvoiceHeader inv on inv_d.Code = inv.Code
                            LEFT JOIN Purchasing.vwPurchaseReceiveDetail rcv_d on inv_d.RcvCode = rcv_d.Code
                            LEFT JOIN Inventory.Item im on im.Id = rcv_d.ItemId" +
                            (!itemId.HasValue || itemId <= 0 ? "" : $" WHERE rcv_d.ItemId = {itemId}")).ToList();

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

                    foreach (var itemSup in supData)
                    {
                        itemSup.TotalTrans = invDetailData.Count(x => x.SupCode == itemSup.Code);
                        itemSup.SubTotal = invDetailData.Where(x => x.SupCode == itemSup.Code).Sum(x => x.SubTotal);
                        itemSup.Disc = invDetailData.Where(x => x.SupCode == itemSup.Code).Sum(x => x.Disc);
                        itemSup.Dpp = invDetailData.Where(x => x.SupCode == itemSup.Code).Sum(x => x.Dpp);
                        itemSup.TaxAmount = invDetailData.Where(x => x.SupCode == itemSup.Code).Sum(x => x.TaxAmount);
                        itemSup.Total = invDetailData.Where(x => x.SupCode == itemSup.Code).Sum(x => x.Total);
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
                        invDetailData = invDetailData.Where(x => invData.Select(y => y.Code).Contains(x.Code)).ToList();
                    }

                    if (!string.IsNullOrWhiteSpace(supCode))
                    {
                        invDetailData = invDetailData.Where(x => x.SupCode == supCode).ToList();
                    }

                    foreach (var item in itemData)
                    {
                        item.TotalTrans = invDetailData.Count(x => x.ItemInitial == item.Initial);
                        item.Qty = invDetailData.Where(x => x.ItemInitial == item.Initial).Sum(x => x.Qty);
                        item.SubTotal = invDetailData.Where(x => x.ItemInitial == item.Initial).Sum(x => x.SubTotal);
                        item.Disc = invDetailData.Where(x => x.ItemInitial == item.Initial).Sum(x => x.Disc);
                        item.DiscHeader = invDetailData.Where(x => x.ItemInitial == item.Initial).Sum(x => x.DiscHeader);
                        item.Dpp = invDetailData.Where(x => x.ItemInitial == item.Initial).Sum(x => x.Dpp);
                        item.TaxAmount = invDetailData.Where(x => x.ItemInitial == item.Initial).Sum(x => x.TaxAmount);
                        item.Total = invDetailData.Where(x => x.ItemInitial == item.Initial).Sum(x => x.Total);
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
                        invDetailData = invDetailData.Where(x => x.Code == code).ToList();
                    }

                    invDetailData = invDetailData.OrderBy(x => x.Date).ToList();

                    invDetailData.Add(new Entity.Purchase.ReportByDetailPInv
                    {
                        Code = "Total",
                        Qty = invDetailData.Sum(x => x.Qty),
                        SubTotal = invDetailData.Sum(x => x.SubTotal),
                        Disc = invDetailData.Sum(x => x.Disc),
                        DiscHeader = invDetailData.Sum(x => x.DiscHeader),
                        Dpp = invDetailData.Sum(x => x.Dpp),
                        TaxAmount = invDetailData.Sum(x => x.TaxAmount),
                        Total = invDetailData.Sum(x => x.Total)
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

                    invDetailData = invDetailData.OrderBy(x => x.Date).ToList();

                    invDetailData.Add(new Entity.Purchase.ReportByDetailPInv
                    {
                        Code = "Total",
                        Qty = invDetailData.Sum(x => x.Qty),
                        SubTotal = invDetailData.Sum(x => x.SubTotal),
                        Disc = invDetailData.Sum(x => x.Disc),
                        DiscHeader = invDetailData.Sum(x => x.DiscHeader),
                        Dpp = invDetailData.Sum(x => x.Dpp),
                        TaxAmount = invDetailData.Sum(x => x.TaxAmount),
                        Total = invDetailData.Sum(x => x.Total)
                    });

                    return invDetailData.AsQueryable().ToDataSourceResult(0, invDetailData.Count, null, null);
                }
            }
        }
    }
}
