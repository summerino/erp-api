using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Purchase;
using Microsoft.EntityFrameworkCore;

namespace ERP.Web.API.Domain.Services.Purchase
{
    public class APAgingReportService : IAPAgingReportService
    {
        private readonly TenantContext _db;
        public APAgingReportService(TenantContext db)
        {
            _db = db;
        }

        public DataSourceResult GetData(int type, string date, string supCode, string duration)
        {
            var supData = _db.ReportBySupplierAgings.FromSqlRaw(@"SELECT sp.Code, sp.[Name], COUNT(*) AS TotalTrans, CAST (0 AS decimal) AS RemainderAmount, 
	                        CAST (0 AS decimal) AS Past90, CAST (0 AS decimal) AS Past61To90, CAST (0 AS decimal) AS Past31To60, 
							CAST (0 AS decimal) AS Past15To30, CAST (0 AS decimal) AS Past8To14, CAST (0 AS decimal) AS Past1To7,
							CAST (0 AS decimal) AS DueToday, CAST (0 AS decimal) AS Due1To7, CAST (0 AS decimal) AS Due8To14,
							CAST (0 AS decimal) AS Due15To30, CAST (0 AS decimal) AS Due31To60, CAST (0 AS decimal) AS Due61To90,
							CAST (0 AS decimal) AS Due90
                            FROM General.Supplier sp
                            LEFT JOIN Purchasing.PurchaseInvoiceHeader inv on inv.SupCode = sp.Code
                            WHERE inv.Mark IN('A', 'PP', 'CMP') GROUP BY sp.Code, sp.Initial, sp.Name").ToList();

            var invData = _db.ReportByInvoiceAPAgings.FromSqlRaw(@"WITH cte_apa_report AS (SELECT inv.[Date], inv.DueDate, inv.Code, inv.POCode AS OrderCode, inv.SupCode, sp.[Name] AS SupName, inv.Total - inv.PaidAmount AS RemainderAmount,
							CAST (CASE WHEN DATEDIFF(DAY, inv.DueDate, GETDATE()) > 90 THEN
							inv.Total ELSE 0 END AS decimal) AS Past90,
							CAST (CASE WHEN DATEDIFF(DAY, inv.DueDate, GETDATE()) > 60 AND DATEDIFF(DAY, inv.DueDate, GETDATE()) <= 90 THEN
							inv.Total ELSE 0 END AS decimal) AS Past61To90,
							CAST (CASE WHEN DATEDIFF(DAY, inv.DueDate, GETDATE()) > 30 AND DATEDIFF(DAY, inv.DueDate, GETDATE()) <= 60 THEN
							inv.Total ELSE 0 END AS decimal) AS Past31To60,
							CAST (CASE WHEN DATEDIFF(DAY, inv.DueDate, GETDATE()) >= 15 AND DATEDIFF(DAY, inv.DueDate, GETDATE()) <= 30 THEN
							inv.Total ELSE 0 END AS decimal) AS Past15To30,
							CAST (CASE WHEN DATEDIFF(DAY, inv.DueDate, GETDATE()) >= 8 AND DATEDIFF(DAY, inv.DueDate, GETDATE()) <= 14 THEN
							inv.Total ELSE 0 END AS decimal) AS Past8To14,
							CAST (CASE WHEN DATEDIFF(DAY, inv.DueDate, GETDATE()) >= 1 AND DATEDIFF(DAY, inv.DueDate, GETDATE()) <= 7 THEN
							inv.Total ELSE 0 END AS decimal) AS Past1To7,
							CAST (CASE WHEN DATEDIFF(DAY, inv.DueDate, GETDATE()) = 0 THEN
							inv.Total ELSE 0 END AS decimal) AS DueToday,
							CAST (CASE WHEN DATEDIFF(DAY, GETDATE(), inv.DueDate) >= 1 AND DATEDIFF(DAY, GETDATE(), inv.DueDate) <= 7 THEN
							inv.Total ELSE 0 END AS decimal) AS Due1To7,
							CAST (CASE WHEN DATEDIFF(DAY, GETDATE(), inv.DueDate) >= 8 AND DATEDIFF(DAY, GETDATE(), inv.DueDate) <= 14 THEN
							inv.Total ELSE 0 END AS decimal) AS Due8To14,
							CAST (CASE WHEN DATEDIFF(DAY, GETDATE(), inv.DueDate) >= 15 AND DATEDIFF(DAY, GETDATE(), inv.DueDate) <= 30 THEN
							inv.Total ELSE 0 END AS decimal) AS Due15To30,
							CAST (CASE WHEN DATEDIFF(DAY, GETDATE(), inv.DueDate) > 30 AND DATEDIFF(DAY, GETDATE(), inv.DueDate) <= 60 THEN
							inv.Total ELSE 0 END AS decimal) AS Due31To60,
							CAST (CASE WHEN DATEDIFF(DAY, GETDATE(), inv.DueDate) > 60 AND DATEDIFF(DAY, GETDATE(), inv.DueDate) <= 90 THEN
							inv.Total ELSE 0 END AS decimal) AS Due61To90,
							CAST (CASE WHEN DATEDIFF(DAY, GETDATE(), inv.DueDate) > 90 THEN
							inv.Total ELSE 0 END AS decimal) AS Due90
							FROM Purchasing.PurchaseInvoiceHeader inv
							LEFT JOIN General.Supplier sp on sp.Code = inv.SupCode
							WHERE inv.Mark IN('A', 'PP', 'CMP')
							UNION
							SELECT bb.[Date], bb.DueDate, bb.Code, '' AS OrderCode, bb.SupCode, bb.SupName, bb.Amount - bb.PaidAmount AS RemainderAmount,
							CAST (CASE WHEN DATEDIFF(DAY, bb.DueDate, GETDATE()) > 90 THEN
							bb.Amount ELSE 0 END AS decimal) AS Past90,
							CAST (CASE WHEN DATEDIFF(DAY, bb.DueDate, GETDATE()) > 60 AND DATEDIFF(DAY, bb.DueDate, GETDATE()) <= 90 THEN
							bb.Amount ELSE 0 END AS decimal) AS Past61To90,
							CAST (CASE WHEN DATEDIFF(DAY, bb.DueDate, GETDATE()) > 30 AND DATEDIFF(DAY, bb.DueDate, GETDATE()) <= 60 THEN
							bb.Amount ELSE 0 END AS decimal) AS Past31To60,
							CAST (CASE WHEN DATEDIFF(DAY, bb.DueDate, GETDATE()) >= 15 AND DATEDIFF(DAY, bb.DueDate, GETDATE()) <= 30 THEN
							bb.Amount ELSE 0 END AS decimal) AS Past15To30,
							CAST (CASE WHEN DATEDIFF(DAY, bb.DueDate, GETDATE()) >= 8 AND DATEDIFF(DAY, bb.DueDate, GETDATE()) <= 14 THEN
							bb.Amount ELSE 0 END AS decimal) AS Past8To14,
							CAST (CASE WHEN DATEDIFF(DAY, bb.DueDate, GETDATE()) >= 1 AND DATEDIFF(DAY, bb.DueDate, GETDATE()) <= 7 THEN
							bb.Amount ELSE 0 END AS decimal) AS Past1To7,
							CAST (CASE WHEN DATEDIFF(DAY, bb.DueDate, GETDATE()) = 0 THEN
							bb.Amount ELSE 0 END AS decimal) AS DueToday,
							CAST (CASE WHEN DATEDIFF(DAY, GETDATE(), bb.DueDate) >= 1 AND DATEDIFF(DAY, GETDATE(), bb.DueDate) <= 7 THEN
							bb.Amount ELSE 0 END AS decimal) AS Due1To7,
							CAST (CASE WHEN DATEDIFF(DAY, GETDATE(), bb.DueDate) >= 8 AND DATEDIFF(DAY, GETDATE(), bb.DueDate) <= 14 THEN
							bb.Amount ELSE 0 END AS decimal) AS Due8To14,
							CAST (CASE WHEN DATEDIFF(DAY, GETDATE(), bb.DueDate) >= 15 AND DATEDIFF(DAY, GETDATE(), bb.DueDate) <= 30 THEN
							bb.Amount ELSE 0 END AS decimal) AS Due15To30,
							CAST (CASE WHEN DATEDIFF(DAY, GETDATE(), bb.DueDate) > 30 AND DATEDIFF(DAY, GETDATE(), bb.DueDate) <= 60 THEN
							bb.Amount ELSE 0 END AS decimal) AS Due31To60,
							CAST (CASE WHEN DATEDIFF(DAY, GETDATE(), bb.DueDate) > 60 AND DATEDIFF(DAY, GETDATE(), bb.DueDate) <= 90 THEN
							bb.Amount ELSE 0 END AS decimal) AS Due61To90,
							CAST (CASE WHEN DATEDIFF(DAY, GETDATE(), bb.DueDate) > 90 THEN
							bb.Amount ELSE 0 END AS decimal) AS Due90
							FROM Accounting.vwBeginningBalanceAP bb
							WHERE bb.IsActive = 1)
							SELECT *FROM cte_apa_report " + (string.IsNullOrEmpty(duration) ? "" : $"WHERE {duration.Replace("'", "''")} > 0")).ToList();

			invData = invData.Where(x => x.Date <= Convert.ToDateTime(date)).ToList();

			foreach (var itemSup in supData)
			{
				itemSup.TotalTrans = invData.Count(x => x.SupCode == itemSup.Code);
				itemSup.RemainderAmount = invData.Where(x => x.SupCode == itemSup.Code).Sum(x => x.RemainderAmount);
				itemSup.Past90 = invData.Where(x => x.SupCode == itemSup.Code).Sum(x => x.Past90);
				itemSup.Past61To90 = invData.Where(x => x.SupCode == itemSup.Code).Sum(x => x.Past61To90);
				itemSup.Past31To60 = invData.Where(x => x.SupCode == itemSup.Code).Sum(x => x.Past31To60);
				itemSup.Past15To30 = invData.Where(x => x.SupCode == itemSup.Code).Sum(x => x.Past15To30);
				itemSup.Past8To14 = invData.Where(x => x.SupCode == itemSup.Code).Sum(x => x.Past8To14);
				itemSup.Past1To7 = invData.Where(x => x.SupCode == itemSup.Code).Sum(x => x.Past1To7);
				itemSup.DueToday = invData.Where(x => x.SupCode == itemSup.Code).Sum(x => x.DueToday);
				itemSup.Due1To7 = invData.Where(x => x.SupCode == itemSup.Code).Sum(x => x.Due1To7);
				itemSup.Due8To14 = invData.Where(x => x.SupCode == itemSup.Code).Sum(x => x.Due8To14);
				itemSup.Due15To30 = invData.Where(x => x.SupCode == itemSup.Code).Sum(x => x.Due15To30);
				itemSup.Due31To60 = invData.Where(x => x.SupCode == itemSup.Code).Sum(x => x.Due31To60);
				itemSup.Due61To90 = invData.Where(x => x.SupCode == itemSup.Code).Sum(x => x.Due61To90);
				itemSup.Due90 = invData.Where(x => x.SupCode == itemSup.Code).Sum(x => x.Due90);
			}

			invData = invData.Where(x => x.RemainderAmount > 0).ToList();

			if (type == 1)
			{
				if (!string.IsNullOrEmpty(supCode))
				{
					invData = invData.Where(x => x.SupCode == supCode).ToList();
				}

				invData = invData.OrderBy(x => x.Date).ToList();

				invData.Add(new Entity.Purchase.ReportByInvoiceAPAging
				{
					SupName = "Total",
					RemainderAmount = invData.Sum(x => x.RemainderAmount),
					Past90 = invData.Sum(x => x.Past90),
					Past61To90 = invData.Sum(x => x.Past61To90),
					Past31To60 = invData.Sum(x => x.Past31To60),
					Past15To30 = invData.Sum(x => x.Past15To30),
					Past8To14 = invData.Sum(x => x.Past8To14),
					Past1To7 = invData.Sum(x => x.Past1To7),
					DueToday = invData.Sum(x => x.DueToday),
					Due1To7 = invData.Sum(x => x.Due1To7),
					Due8To14 = invData.Sum(x => x.Due8To14),
					Due15To30 = invData.Sum(x => x.Due15To30),
					Due31To60 = invData.Sum(x => x.Due31To60),
					Due61To90 = invData.Sum(x => x.Due61To90),
					Due90 = invData.Sum(x => x.Due90)
				});

				return invData.AsQueryable().ToDataSourceResult(0, invData.Count, null, null);
			}
			else
			{
				if (!string.IsNullOrEmpty(supCode))
				{
					supData = supData.Where(x => x.Code == supCode).ToList();
				}

				supData.Add(new Entity.Purchase.ReportBySupplierAging
				{
					Name = "Total",
					RemainderAmount = supData.Sum(x => x.RemainderAmount),
					Past90 = supData.Sum(x => x.Past90),
					Past61To90 = supData.Sum(x => x.Past61To90),
					Past31To60 = supData.Sum(x => x.Past31To60),
					Past15To30 = supData.Sum(x => x.Past15To30),
					Past8To14 = supData.Sum(x => x.Past8To14),
					Past1To7 = supData.Sum(x => x.Past1To7),
					DueToday = supData.Sum(x => x.DueToday),
					Due1To7 = supData.Sum(x => x.Due1To7),
					Due8To14 = supData.Sum(x => x.Due8To14),
					Due15To30 = supData.Sum(x => x.Due15To30),
					Due31To60 = supData.Sum(x => x.Due31To60),
					Due61To90 = supData.Sum(x => x.Due61To90),
					Due90 = supData.Sum(x => x.Due90)
				});

				return supData.AsQueryable().ToDataSourceResult(0, supData.Count, null, null);
			}
		}
	}
}
