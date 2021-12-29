using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.Sales;
using ERP.Web.API.Domain.Interfaces.Sales;
using Microsoft.EntityFrameworkCore;

namespace ERP.Web.API.Domain.Services.Sales
{
    public class ARAgingReportService : IARAgingReportService
    {
        private readonly TenantContext _db;
        public ARAgingReportService(TenantContext db)
        {
            _db = db;
        }
        public DataSourceResult GetData(int type, string date, string custCode, int? slsId, string duration)
        {
            var cusData = _db.ReportByCustomerAgings.FromSqlRaw(@"SELECT cs.Code, cs.[Name], COUNT(*) AS TotalTrans, CAST (0 AS decimal) AS RemainderAmount, 
	                        CAST (0 AS decimal) AS Past90, CAST (0 AS decimal) AS Past61To90, CAST (0 AS decimal) AS Past31To60, 
							CAST (0 AS decimal) AS Past15To30, CAST (0 AS decimal) AS Past8To14, CAST (0 AS decimal) AS Past1To7,
							CAST (0 AS decimal) AS DueToday, CAST (0 AS decimal) AS Due1To7, CAST (0 AS decimal) AS Due8To14,
							CAST (0 AS decimal) AS Due15To30, CAST (0 AS decimal) AS Due31To60, CAST (0 AS decimal) AS Due61To90,
							CAST (0 AS decimal) AS Due90
                            FROM General.Customer cs
                            LEFT JOIN Sales.SalesDeliveryHeader dlv ON dlv.CustCode = cs.Code
                            LEFT JOIN Sales.SalesInvoiceDetail invD ON invD.DOCode = dlv.Code
                            LEFT JOIN Sales.SalesInvoiceHeader inv ON inv.Code = invD.Code AND inv.Mark IN('A', 'PP', 'CMP')
                            WHERE dlv.Mark IN('A', 'INV') AND inv.Total IS NOT NULL GROUP BY cs.Code, cs.Initial, cs.[Name]").ToList();

            var dlvData = _db.ReportByDeliveryARAgings.FromSqlRaw(@"WITH cte_ara_report AS (SELECT dlv.[Date], inv.DueDate, dlv.Code, dlv.TransCode AS SrcCode, inv.Code AS InvCode, sls.FirstName AS SlsName, dlv.CustCode, sp.[Name] AS CustName, dlv.Total - dlv.PaidAmount AS RemainderAmount,
							CAST (CASE WHEN DATEDIFF(DAY, inv.DueDate, GETDATE()) > 90 THEN
							dlv.Total - dlv.PaidAmount ELSE 0 END AS decimal) AS Past90,
							CAST (CASE WHEN DATEDIFF(DAY, inv.DueDate, GETDATE()) > 60 AND DATEDIFF(DAY, inv.DueDate, GETDATE()) <= 90 THEN
							dlv.Total - dlv.PaidAmount ELSE 0 END AS decimal) AS Past61To90,
							CAST (CASE WHEN DATEDIFF(DAY, inv.DueDate, GETDATE()) > 30 AND DATEDIFF(DAY, inv.DueDate, GETDATE()) <= 60 THEN
							dlv.Total - dlv.PaidAmount ELSE 0 END AS decimal) AS Past31To60,
							CAST (CASE WHEN DATEDIFF(DAY, inv.DueDate, GETDATE()) >= 15 AND DATEDIFF(DAY, inv.DueDate, GETDATE()) <= 30 THEN
							dlv.Total - dlv.PaidAmount ELSE 0 END AS decimal) AS Past15To30,
							CAST (CASE WHEN DATEDIFF(DAY, inv.DueDate, GETDATE()) >= 8 AND DATEDIFF(DAY, inv.DueDate, GETDATE()) <= 14 THEN
							dlv.Total - dlv.PaidAmount ELSE 0 END AS decimal) AS Past8To14,
							CAST (CASE WHEN DATEDIFF(DAY, inv.DueDate, GETDATE()) >= 1 AND DATEDIFF(DAY, inv.DueDate, GETDATE()) <= 7 THEN
							dlv.Total - dlv.PaidAmount ELSE 0 END AS decimal) AS Past1To7,
							CAST (CASE WHEN DATEDIFF(DAY, inv.DueDate, GETDATE()) = 0 THEN
							dlv.Total - dlv.PaidAmount ELSE 0 END AS decimal) AS DueToday,
							CAST (CASE WHEN DATEDIFF(DAY, GETDATE(), inv.DueDate) >= 1 AND DATEDIFF(DAY, GETDATE(), inv.DueDate) <= 7 THEN
							dlv.Total - dlv.PaidAmount ELSE 0 END AS decimal) AS Due1To7,
							CAST (CASE WHEN DATEDIFF(DAY, GETDATE(), inv.DueDate) >= 8 AND DATEDIFF(DAY, GETDATE(), inv.DueDate) <= 14 THEN
							dlv.Total - dlv.PaidAmount ELSE 0 END AS decimal) AS Due8To14,
							CAST (CASE WHEN DATEDIFF(DAY, GETDATE(), inv.DueDate) >= 15 AND DATEDIFF(DAY, GETDATE(), inv.DueDate) <= 30 THEN
							dlv.Total - dlv.PaidAmount ELSE 0 END AS decimal) AS Due15To30,
							CAST (CASE WHEN DATEDIFF(DAY, GETDATE(), inv.DueDate) > 30 AND DATEDIFF(DAY, GETDATE(), inv.DueDate) <= 60 THEN
							dlv.Total - dlv.PaidAmount ELSE 0 END AS decimal) AS Due31To60,
							CAST (CASE WHEN DATEDIFF(DAY, GETDATE(), inv.DueDate) > 60 AND DATEDIFF(DAY, GETDATE(), inv.DueDate) <= 90 THEN
							dlv.Total - dlv.PaidAmount ELSE 0 END AS decimal) AS Due61To90,
							CAST (CASE WHEN DATEDIFF(DAY, GETDATE(), inv.DueDate) > 90 THEN
							dlv.Total - dlv.PaidAmount ELSE 0 END AS decimal) AS Due90
                            FROM Sales.SalesDeliveryHeader dlv
                            LEFT JOIN Sales.SalesOrderHeader so ON so.Code = dlv.TransCode
                            LEFT JOIN General.Employee sls ON sls.Id = so.SalesBy
                            LEFT JOIN General.Customer sp ON sp.Code = dlv.CustCode
                            LEFT JOIN Sales.SalesInvoiceDetail invD ON invD.DOCode = dlv.Code
                            LEFT JOIN Sales.SalesInvoiceHeader inv ON inv.Code = invD.Code AND inv.Mark IN('A','PP','CMP')
                            WHERE dlv.Mark IN('A','INV') AND dlv.SrcTrans = 1" + (slsId > 0 ? $" AND so.SalesBy = {slsId} " : " ") + @"
							UNION
							SELECT bb.[Date], bb.DueDate, bb.Code, '' AS OrderCode, '' AS SrcCode, '' AS SlsName, bb.CustCode, bb.CustName, bb.Amount - bb.PaidAmount AS RemainderAmount,
							CAST (CASE WHEN DATEDIFF(DAY, bb.DueDate, GETDATE()) > 90 THEN
							bb.Amount - bb.PaidAmount ELSE 0 END AS decimal) AS Past90,
							CAST (CASE WHEN DATEDIFF(DAY, bb.DueDate, GETDATE()) > 60 AND DATEDIFF(DAY, bb.DueDate, GETDATE()) <= 90 THEN
							bb.Amount - bb.PaidAmount ELSE 0 END AS decimal) AS Past61To90,
							CAST (CASE WHEN DATEDIFF(DAY, bb.DueDate, GETDATE()) > 30 AND DATEDIFF(DAY, bb.DueDate, GETDATE()) <= 60 THEN
							bb.Amount - bb.PaidAmount ELSE 0 END AS decimal) AS Past31To60,
							CAST (CASE WHEN DATEDIFF(DAY, bb.DueDate, GETDATE()) >= 15 AND DATEDIFF(DAY, bb.DueDate, GETDATE()) <= 30 THEN
							bb.Amount - bb.PaidAmount ELSE 0 END AS decimal) AS Past15To30,
							CAST (CASE WHEN DATEDIFF(DAY, bb.DueDate, GETDATE()) >= 8 AND DATEDIFF(DAY, bb.DueDate, GETDATE()) <= 14 THEN
							bb.Amount - bb.PaidAmount ELSE 0 END AS decimal) AS Past8To14,
							CAST (CASE WHEN DATEDIFF(DAY, bb.DueDate, GETDATE()) >= 1 AND DATEDIFF(DAY, bb.DueDate, GETDATE()) <= 7 THEN
							bb.Amount - bb.PaidAmount ELSE 0 END AS decimal) AS Past1To7,
							CAST (CASE WHEN DATEDIFF(DAY, bb.DueDate, GETDATE()) = 0 THEN
							bb.Amount - bb.PaidAmount ELSE 0 END AS decimal) AS DueToday,
							CAST (CASE WHEN DATEDIFF(DAY, GETDATE(), bb.DueDate) >= 1 AND DATEDIFF(DAY, GETDATE(), bb.DueDate) <= 7 THEN
							bb.Amount - bb.PaidAmount ELSE 0 END AS decimal) AS Due1To7,
							CAST (CASE WHEN DATEDIFF(DAY, GETDATE(), bb.DueDate) >= 8 AND DATEDIFF(DAY, GETDATE(), bb.DueDate) <= 14 THEN
							bb.Amount - bb.PaidAmount ELSE 0 END AS decimal) AS Due8To14,
							CAST (CASE WHEN DATEDIFF(DAY, GETDATE(), bb.DueDate) >= 15 AND DATEDIFF(DAY, GETDATE(), bb.DueDate) <= 30 THEN
							bb.Amount - bb.PaidAmount ELSE 0 END AS decimal) AS Due15To30,
							CAST (CASE WHEN DATEDIFF(DAY, GETDATE(), bb.DueDate) > 30 AND DATEDIFF(DAY, GETDATE(), bb.DueDate) <= 60 THEN
							bb.Amount - bb.PaidAmount ELSE 0 END AS decimal) AS Due31To60,
							CAST (CASE WHEN DATEDIFF(DAY, GETDATE(), bb.DueDate) > 60 AND DATEDIFF(DAY, GETDATE(), bb.DueDate) <= 90 THEN
							bb.Amount - bb.PaidAmount ELSE 0 END AS decimal) AS Due61To90,
							CAST (CASE WHEN DATEDIFF(DAY, GETDATE(), bb.DueDate) > 90 THEN
							bb.Amount - bb.PaidAmount ELSE 0 END AS decimal) AS Due90
							FROM Accounting.vwBeginningBalanceAR bb
							WHERE bb.IsActive = 1)" +
							@" SELECT * FROM cte_ara_report " + (string.IsNullOrEmpty(duration) ? "" : $"WHERE {duration.Replace("'", "''")} > 0")).ToList();

			dlvData = dlvData.Where(x => x.Date <= Convert.ToDateTime(date)).ToList();

			dlvData = dlvData.Where(x => x.RemainderAmount > 0).ToList();

			foreach (var itemCus in cusData)
			{
				itemCus.TotalTrans = dlvData.Count(x => x.CustCode == itemCus.Code);
				itemCus.RemainderAmount = dlvData.Where(x => x.CustCode == itemCus.Code).Sum(x => x.RemainderAmount);
				itemCus.Past90 = dlvData.Where(x => x.CustCode == itemCus.Code).Sum(x => x.Past90);
				itemCus.Past61To90 = dlvData.Where(x => x.CustCode == itemCus.Code).Sum(x => x.Past61To90);
				itemCus.Past31To60 = dlvData.Where(x => x.CustCode == itemCus.Code).Sum(x => x.Past31To60);
				itemCus.Past15To30 = dlvData.Where(x => x.CustCode == itemCus.Code).Sum(x => x.Past15To30);
				itemCus.Past8To14 = dlvData.Where(x => x.CustCode == itemCus.Code).Sum(x => x.Past8To14);
				itemCus.Past1To7 = dlvData.Where(x => x.CustCode == itemCus.Code).Sum(x => x.Past1To7);
				itemCus.DueToday = dlvData.Where(x => x.CustCode == itemCus.Code).Sum(x => x.DueToday);
				itemCus.Due1To7 = dlvData.Where(x => x.CustCode == itemCus.Code).Sum(x => x.Due1To7);
				itemCus.Due8To14 = dlvData.Where(x => x.CustCode == itemCus.Code).Sum(x => x.Due8To14);
				itemCus.Due15To30 = dlvData.Where(x => x.CustCode == itemCus.Code).Sum(x => x.Due15To30);
				itemCus.Due31To60 = dlvData.Where(x => x.CustCode == itemCus.Code).Sum(x => x.Due31To60);
				itemCus.Due61To90 = dlvData.Where(x => x.CustCode == itemCus.Code).Sum(x => x.Due61To90);
				itemCus.Due90 = dlvData.Where(x => x.CustCode == itemCus.Code).Sum(x => x.Due90);
			}

            if (type == 1)
            {
                if (!string.IsNullOrEmpty(custCode))
                {
                    dlvData = dlvData.Where(x => x.CustCode == custCode).ToList();
                }

                dlvData.Add(new ReportByDeliveryARAging
                {
					CustName = "Total",
					RemainderAmount = dlvData.Sum(x => x.RemainderAmount),
					Past90 = dlvData.Sum(x => x.Past90),
					Past61To90 = dlvData.Sum(x => x.Past61To90),
					Past31To60 = dlvData.Sum(x => x.Past31To60),
					Past15To30 = dlvData.Sum(x => x.Past15To30),
					Past8To14 = dlvData.Sum(x => x.Past8To14),
					Past1To7 = dlvData.Sum(x => x.Past1To7),
					DueToday = dlvData.Sum(x => x.DueToday),
					Due1To7 = dlvData.Sum(x => x.Due1To7),
					Due8To14 = dlvData.Sum(x => x.Due8To14),
					Due15To30 = dlvData.Sum(x => x.Due15To30),
					Due31To60 = dlvData.Sum(x => x.Due31To60),
					Due61To90 = dlvData.Sum(x => x.Due61To90),
					Due90 = dlvData.Sum(x => x.Due90)
				});

                return dlvData.AsQueryable().ToDataSourceResult(0, dlvData.Count, null, null);
            }
            else
            {
                if (!string.IsNullOrEmpty(custCode))
                {
                    cusData = cusData.Where(x => x.Code == custCode).ToList();
                }

                cusData.Add(new ReportByCustomerAging
                {
					Name = "Total",
					RemainderAmount = cusData.Sum(x => x.RemainderAmount),
					Past90 = cusData.Sum(x => x.Past90),
					Past61To90 = cusData.Sum(x => x.Past61To90),
					Past31To60 = cusData.Sum(x => x.Past31To60),
					Past15To30 = cusData.Sum(x => x.Past15To30),
					Past8To14 = cusData.Sum(x => x.Past8To14),
					Past1To7 = cusData.Sum(x => x.Past1To7),
					DueToday = cusData.Sum(x => x.DueToday),
					Due1To7 = cusData.Sum(x => x.Due1To7),
					Due8To14 = cusData.Sum(x => x.Due8To14),
					Due15To30 = cusData.Sum(x => x.Due15To30),
					Due31To60 = cusData.Sum(x => x.Due31To60),
					Due61To90 = cusData.Sum(x => x.Due61To90),
					Due90 = cusData.Sum(x => x.Due90)
				});

                return cusData.AsQueryable().ToDataSourceResult(0, cusData.Count, null, null);
            }
        }
    }
}
