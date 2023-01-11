using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.Sales;
using ERP.Web.API.Domain.Interfaces.Sales;
using Microsoft.EntityFrameworkCore;

namespace ERP.Web.API.Domain.Services.Sales;

public class ARAgingReportService : IARAgingReportService
{
    private readonly TenantContext _db;
    public ARAgingReportService(TenantContext db)
    {
        _db = db;
    }
    public DataSourceResult GetData(int type, string date, string custCode, int? slsId, string duration)
    {
        var cusData = _db.ReportByCustomerAgings.FromSqlRaw(@"SELECT a.Code, a.[Name], SUM(a.TotalTrans) AS TotalTrans, CAST (0 AS decimal(19, 6)) AS RemainderAmount, 
	                        CAST (0 AS decimal(19, 6)) AS Past90, CAST (0 AS decimal(19, 6)) AS Past61To90, CAST (0 AS decimal(19, 6)) AS Past31To60, 
							CAST (0 AS decimal(19, 6)) AS Past15To30, CAST (0 AS decimal(19, 6)) AS Past8To14, CAST (0 AS decimal(19, 6)) AS Past1To7,
							CAST (0 AS decimal(19, 6)) AS DueToday, CAST (0 AS decimal(19, 6)) AS Due1To7, CAST (0 AS decimal(19, 6)) AS Due8To14,
							CAST (0 AS decimal(19, 6)) AS Due15To30, CAST (0 AS decimal(19, 6)) AS Due31To60, CAST (0 AS decimal(19, 6)) AS Due61To90,
							CAST (0 AS decimal(19, 6)) AS Due90 FROM (
                    SELECT cs.Code, cs.[Name], COUNT(*) AS TotalTrans
                                                FROM General.Customer cs
                                                LEFT JOIN Sales.SalesDeliveryHeader dlv ON dlv.CustCode = cs.Code
                                                LEFT JOIN Sales.SalesInvoiceDetail invD ON invD.DOCode = dlv.Code
                                                LEFT JOIN Sales.SalesInvoiceHeader inv ON inv.Code = invD.Code AND inv.Mark IN('A', 'PP', 'CMP')
                                                WHERE dlv.Mark IN('A', 'INV') AND inv.Total IS NOT NULL GROUP BY cs.Code, cs.Initial, cs.[Name]
							                    UNION
                    SELECT cs.Code, cs.[Name], COUNT(*) AS TotalTrans
                                                FROM General.Customer cs
                                                LEFT JOIN Accounting.BeginningBalanceAR ar on ar.CustCode = cs.Code
                                                WHERE ar.IsActive = 1 GROUP BY cs.Code, cs.Initial, cs.[Name]) a
                    GROUP BY a.Code, a.[Name]").ToList();

        var query = "";
        var sysData = _db.SystemParameters.FirstOrDefault(x => x.Code == "AR_RECOG_TIME");
        if (sysData.Value == "SI")
        {
            query = $@"SELECT inv.[Date], inv.DueDate, inv.Code, inv.SOCode AS SrcCode, inv.Code AS InvCode, sls.FirstName AS SlsName,
                    inv.CustCode, sp.[Name] AS CustName, inv.Total - (ISNULL(si_cm.Amount, 0) + ISNULL(cb.Amount, 0)) AS RemainderAmount,
                    CAST (CASE WHEN DATEDIFF(DAY, inv.DueDate, dbo.udf_current_local_time()) > 90 THEN
                    inv.Total - (ISNULL(si_cm.Amount, 0) + ISNULL(cb.Amount, 0)) ELSE 0 END AS decimal(19, 6)) AS Past90,
                    CAST (CASE WHEN DATEDIFF(DAY, inv.DueDate, dbo.udf_current_local_time()) > 60 AND DATEDIFF(DAY, inv.DueDate, dbo.udf_current_local_time()) <= 90 THEN
                    inv.Total - (ISNULL(si_cm.Amount, 0) + ISNULL(cb.Amount, 0)) ELSE 0 END AS decimal(19, 6)) AS Past61To90,
                    CAST (CASE WHEN DATEDIFF(DAY, inv.DueDate, dbo.udf_current_local_time()) > 30 AND DATEDIFF(DAY, inv.DueDate, dbo.udf_current_local_time()) <= 60 THEN
                    inv.Total - (ISNULL(si_cm.Amount, 0) + ISNULL(cb.Amount, 0)) ELSE 0 END AS decimal(19, 6)) AS Past31To60,
                    CAST (CASE WHEN DATEDIFF(DAY, inv.DueDate, dbo.udf_current_local_time()) >= 15 AND DATEDIFF(DAY, inv.DueDate, dbo.udf_current_local_time()) <= 30 THEN
                    inv.Total - (ISNULL(si_cm.Amount, 0) + ISNULL(cb.Amount, 0)) ELSE 0 END AS decimal(19, 6)) AS Past15To30,
                    CAST (CASE WHEN DATEDIFF(DAY, inv.DueDate, dbo.udf_current_local_time()) >= 8 AND DATEDIFF(DAY, inv.DueDate, dbo.udf_current_local_time()) <= 14 THEN
                    inv.Total - (ISNULL(si_cm.Amount, 0) + ISNULL(cb.Amount, 0)) ELSE 0 END AS decimal(19, 6)) AS Past8To14,
                    CAST (CASE WHEN DATEDIFF(DAY, inv.DueDate, dbo.udf_current_local_time()) >= 1 AND DATEDIFF(DAY, inv.DueDate, dbo.udf_current_local_time()) <= 7 THEN
                    inv.Total - (ISNULL(si_cm.Amount, 0) + ISNULL(cb.Amount, 0)) ELSE 0 END AS decimal(19, 6)) AS Past1To7,
                    CAST (CASE WHEN DATEDIFF(DAY, inv.DueDate, dbo.udf_current_local_time()) = 0 THEN
                    inv.Total - (ISNULL(si_cm.Amount, 0) + ISNULL(cb.Amount, 0)) WHEN inv.DueDate IS NULL THEN inv.Total - (ISNULL(si_cm.Amount, 0) + ISNULL(cb.Amount, 0)) ELSE 0 END AS decimal(19, 6)) AS DueToday,
                    CAST (CASE WHEN DATEDIFF(DAY, dbo.udf_current_local_time(), inv.DueDate) >= 1 AND DATEDIFF(DAY, dbo.udf_current_local_time(), inv.DueDate) <= 7 THEN
                    inv.Total - (ISNULL(si_cm.Amount, 0) + ISNULL(cb.Amount, 0)) ELSE 0 END AS decimal(19, 6)) AS Due1To7,
                    CAST (CASE WHEN DATEDIFF(DAY, dbo.udf_current_local_time(), inv.DueDate) >= 8 AND DATEDIFF(DAY, dbo.udf_current_local_time(), inv.DueDate) <= 14 THEN
                    inv.Total - (ISNULL(si_cm.Amount, 0) + ISNULL(cb.Amount, 0)) ELSE 0 END AS decimal(19, 6)) AS Due8To14,
                    CAST (CASE WHEN DATEDIFF(DAY, dbo.udf_current_local_time(), inv.DueDate) >= 15 AND DATEDIFF(DAY, dbo.udf_current_local_time(), inv.DueDate) <= 30 THEN
                    inv.Total - (ISNULL(si_cm.Amount, 0) + ISNULL(cb.Amount, 0)) ELSE 0 END AS decimal(19, 6)) AS Due15To30,
                    CAST (CASE WHEN DATEDIFF(DAY, dbo.udf_current_local_time(), inv.DueDate) > 30 AND DATEDIFF(DAY, dbo.udf_current_local_time(), inv.DueDate) <= 60 THEN
                    inv.Total - (ISNULL(si_cm.Amount, 0) + ISNULL(cb.Amount, 0)) ELSE 0 END AS decimal(19, 6)) AS Due31To60,
                    CAST (CASE WHEN DATEDIFF(DAY, dbo.udf_current_local_time(), inv.DueDate) > 60 AND DATEDIFF(DAY, dbo.udf_current_local_time(), inv.DueDate) <= 90 THEN
                    inv.Total - (ISNULL(si_cm.Amount, 0) + ISNULL(cb.Amount, 0)) ELSE 0 END AS decimal(19, 6)) AS Due61To90,
                    CAST (CASE WHEN DATEDIFF(DAY, dbo.udf_current_local_time(), inv.DueDate) > 90 THEN
                    inv.Total - (ISNULL(si_cm.Amount, 0) + ISNULL(cb.Amount, 0)) ELSE 0 END AS decimal(19, 6)) AS Due90
                    FROM Sales.SalesInvoiceHeader inv
                    LEFT JOIN Sales.SalesOrderHeader so ON so.Code = inv.SOCode
                    LEFT JOIN General.Employee sls ON sls.Id = so.SalesBy
                    LEFT JOIN General.Customer sp ON sp.Code = inv.CustCode
                    LEFT JOIN ( 
			                    SELECT cb_d.TransCode AS Code, SUM(cb_d.Amount) AS Amount FROM Finance.GeneralCashBankDetail cb_d
			                    LEFT JOIN Finance.GeneralCashBankHeader cb_h ON cb_h.Code = cb_d.Code
			                    WHERE cb_h.Mark NOT IN ('V', 'REJ') AND cb_d.Src = 'SI' AND cb_d.[Type] = 'AR' AND cb_h.Date <= '{date}'
			                    GROUP BY cb_d.TransCode
			                    ) cb ON cb.Code = inv.Code
                    LEFT JOIN (
			                    SELECT InvCode AS Code, SUM(CreditMemoAmount) AS Amount FROM Sales.SalesInvoiceCreditMemo si_cm
			                    LEFT JOIN Sales.CreditMemo cm ON cm.Code = si_cm.CreditMemoCode
			                    WHERE cm.Mark <> 'V' AND cm.Date <= '{date}'
			                    GROUP BY InvCode
			                    ) si_cm ON  si_cm.Code = inv.Code
                    WHERE inv.Mark IN('A','PP','CMP')
                    ";
        }
        else
        {
            query = $@"SELECT dlv.[Date], inv.DueDate, dlv.Code, dlv.TransCode AS SrcCode, inv.Code AS InvCode, sls.FirstName AS SlsName,
                    dlv.CustCode, sp.[Name] AS CustName, dlv.Total - (ISNULL(si_cm.Amount, 0) + ISNULL(cb.Amount, 0)) AS RemainderAmount,
                    CAST (CASE WHEN DATEDIFF(DAY, inv.DueDate, dbo.udf_current_local_time()) > 90 THEN
                    dlv.Total - (ISNULL(si_cm.Amount, 0) + ISNULL(cb.Amount, 0)) ELSE 0 END AS decimal(19, 6)) AS Past90,
                    CAST (CASE WHEN DATEDIFF(DAY, inv.DueDate, dbo.udf_current_local_time()) > 60 AND DATEDIFF(DAY, inv.DueDate, dbo.udf_current_local_time()) <= 90 THEN
                    dlv.Total - (ISNULL(si_cm.Amount, 0) + ISNULL(cb.Amount, 0)) ELSE 0 END AS decimal(19, 6)) AS Past61To90,
                    CAST (CASE WHEN DATEDIFF(DAY, inv.DueDate, dbo.udf_current_local_time()) > 30 AND DATEDIFF(DAY, inv.DueDate, dbo.udf_current_local_time()) <= 60 THEN
                    dlv.Total - (ISNULL(si_cm.Amount, 0) + ISNULL(cb.Amount, 0)) ELSE 0 END AS decimal(19, 6)) AS Past31To60,
                    CAST (CASE WHEN DATEDIFF(DAY, inv.DueDate, dbo.udf_current_local_time()) >= 15 AND DATEDIFF(DAY, inv.DueDate, dbo.udf_current_local_time()) <= 30 THEN
                    dlv.Total - (ISNULL(si_cm.Amount, 0) + ISNULL(cb.Amount, 0)) ELSE 0 END AS decimal(19, 6)) AS Past15To30,
                    CAST (CASE WHEN DATEDIFF(DAY, inv.DueDate, dbo.udf_current_local_time()) >= 8 AND DATEDIFF(DAY, inv.DueDate, dbo.udf_current_local_time()) <= 14 THEN
                    dlv.Total - (ISNULL(si_cm.Amount, 0) + ISNULL(cb.Amount, 0)) ELSE 0 END AS decimal(19, 6)) AS Past8To14,
                    CAST (CASE WHEN DATEDIFF(DAY, inv.DueDate, dbo.udf_current_local_time()) >= 1 AND DATEDIFF(DAY, inv.DueDate, dbo.udf_current_local_time()) <= 7 THEN
                    dlv.Total - (ISNULL(si_cm.Amount, 0) + ISNULL(cb.Amount, 0)) ELSE 0 END AS decimal(19, 6)) AS Past1To7,
                    CAST (CASE WHEN DATEDIFF(DAY, inv.DueDate, dbo.udf_current_local_time()) = 0 THEN
                    dlv.Total - (ISNULL(si_cm.Amount, 0) + ISNULL(cb.Amount, 0)) WHEN inv.DueDate IS NULL THEN dlv.Total - (ISNULL(si_cm.Amount, 0) + ISNULL(cb.Amount, 0)) ELSE 0 END AS decimal(19, 6)) AS DueToday,
                    CAST (CASE WHEN DATEDIFF(DAY, dbo.udf_current_local_time(), inv.DueDate) >= 1 AND DATEDIFF(DAY, dbo.udf_current_local_time(), inv.DueDate) <= 7 THEN
                    dlv.Total - (ISNULL(si_cm.Amount, 0) + ISNULL(cb.Amount, 0)) ELSE 0 END AS decimal(19, 6)) AS Due1To7,
                    CAST (CASE WHEN DATEDIFF(DAY, dbo.udf_current_local_time(), inv.DueDate) >= 8 AND DATEDIFF(DAY, dbo.udf_current_local_time(), inv.DueDate) <= 14 THEN
                    dlv.Total - (ISNULL(si_cm.Amount, 0) + ISNULL(cb.Amount, 0)) ELSE 0 END AS decimal(19, 6)) AS Due8To14,
                    CAST (CASE WHEN DATEDIFF(DAY, dbo.udf_current_local_time(), inv.DueDate) >= 15 AND DATEDIFF(DAY, dbo.udf_current_local_time(), inv.DueDate) <= 30 THEN
                    dlv.Total - (ISNULL(si_cm.Amount, 0) + ISNULL(cb.Amount, 0)) ELSE 0 END AS decimal(19, 6)) AS Due15To30,
                    CAST (CASE WHEN DATEDIFF(DAY, dbo.udf_current_local_time(), inv.DueDate) > 30 AND DATEDIFF(DAY, dbo.udf_current_local_time(), inv.DueDate) <= 60 THEN
                    dlv.Total - (ISNULL(si_cm.Amount, 0) + ISNULL(cb.Amount, 0)) ELSE 0 END AS decimal(19, 6)) AS Due31To60,
                    CAST (CASE WHEN DATEDIFF(DAY, dbo.udf_current_local_time(), inv.DueDate) > 60 AND DATEDIFF(DAY, dbo.udf_current_local_time(), inv.DueDate) <= 90 THEN
                    dlv.Total - (ISNULL(si_cm.Amount, 0) + ISNULL(cb.Amount, 0)) ELSE 0 END AS decimal(19, 6)) AS Due61To90,
                    CAST (CASE WHEN DATEDIFF(DAY, dbo.udf_current_local_time(), inv.DueDate) > 90 THEN
                    dlv.Total - (ISNULL(si_cm.Amount, 0) + ISNULL(cb.Amount, 0)) ELSE 0 END AS decimal(19, 6)) AS Due90
                    FROM Sales.SalesDeliveryHeader dlv
                    LEFT JOIN Sales.SalesOrderHeader so ON so.Code = dlv.TransCode
                    LEFT JOIN General.Employee sls ON sls.Id = so.SalesBy
                    LEFT JOIN General.Customer sp ON sp.Code = dlv.CustCode
                    LEFT JOIN Sales.SalesInvoiceDetail invD ON invD.DOCode = dlv.Code
                    LEFT JOIN Sales.SalesInvoiceHeader inv ON inv.Code = invD.Code AND inv.Mark IN('A','PP','CMP')
                    LEFT JOIN ( 
			                    SELECT cb_d.TransCode AS Code, SUM(cb_d.Amount) AS Amount FROM Finance.GeneralCashBankDetail cb_d
			                    LEFT JOIN Finance.GeneralCashBankHeader cb_h ON cb_h.Code = cb_d.Code
			                    WHERE cb_h.Mark NOT IN ('V', 'REJ') AND cb_d.Src = 'SI' AND cb_d.[Type] = 'AR' AND cb_h.Date <= '{date}'
			                    GROUP BY cb_d.TransCode
			                    ) cb ON cb.Code = inv.Code
                    LEFT JOIN (
			                    SELECT InvCode AS Code, SUM(CreditMemoAmount) AS Amount FROM Sales.SalesInvoiceCreditMemo si_cm
			                    LEFT JOIN Sales.CreditMemo cm ON cm.Code = si_cm.CreditMemoCode
			                    WHERE cm.Mark <> 'V' AND cm.Date <= '{date}'
			                    GROUP BY InvCode
			                    ) si_cm ON  si_cm.Code = inv.Code
                    WHERE dlv.Mark IN('A','INV') AND dlv.SrcTrans = 1";
        }

        var dlvData = _db.ReportByDeliveryARAgings.FromSqlRaw(@"WITH cte_ara_report AS (" + query + (slsId > 0 ? $" AND so.SalesBy = {slsId} " : " ") +
                  (slsId > 0 ? "" : $@" UNION
					SELECT bb.[Date], bb.DueDate, bb.Code, '' AS OrderCode, '' AS SrcCode, '' AS SlsName, bb.CustCode, bb.CustName, bb.Amount - ISNULL(cb.Amount, 0) AS RemainderAmount,
                    CAST (CASE WHEN DATEDIFF(DAY, bb.DueDate, dbo.udf_current_local_time()) > 90 THEN
                    bb.Amount - ISNULL(cb.Amount, 0) ELSE 0 END AS decimal(19, 6)) AS Past90,
                    CAST (CASE WHEN DATEDIFF(DAY, bb.DueDate, dbo.udf_current_local_time()) > 60 AND DATEDIFF(DAY, bb.DueDate, dbo.udf_current_local_time()) <= 90 THEN
                    bb.Amount - ISNULL(cb.Amount, 0) ELSE 0 END AS decimal(19, 6)) AS Past61To90,
                    CAST (CASE WHEN DATEDIFF(DAY, bb.DueDate, dbo.udf_current_local_time()) > 30 AND DATEDIFF(DAY, bb.DueDate, dbo.udf_current_local_time()) <= 60 THEN
                    bb.Amount - ISNULL(cb.Amount, 0) ELSE 0 END AS decimal(19, 6)) AS Past31To60,
                    CAST (CASE WHEN DATEDIFF(DAY, bb.DueDate, dbo.udf_current_local_time()) >= 15 AND DATEDIFF(DAY, bb.DueDate, dbo.udf_current_local_time()) <= 30 THEN
                    bb.Amount - ISNULL(cb.Amount, 0) ELSE 0 END AS decimal(19, 6)) AS Past15To30,
                    CAST (CASE WHEN DATEDIFF(DAY, bb.DueDate, dbo.udf_current_local_time()) >= 8 AND DATEDIFF(DAY, bb.DueDate, dbo.udf_current_local_time()) <= 14 THEN
                    bb.Amount - ISNULL(cb.Amount, 0) ELSE 0 END AS decimal(19, 6)) AS Past8To14,
                    CAST (CASE WHEN DATEDIFF(DAY, bb.DueDate, dbo.udf_current_local_time()) >= 1 AND DATEDIFF(DAY, bb.DueDate, dbo.udf_current_local_time()) <= 7 THEN
                    bb.Amount - ISNULL(cb.Amount, 0) ELSE 0 END AS decimal(19, 6)) AS Past1To7,
                    CAST (CASE WHEN DATEDIFF(DAY, bb.DueDate, dbo.udf_current_local_time()) = 0 THEN
                    bb.Amount - ISNULL(cb.Amount, 0) WHEN bb.DueDate IS NULL THEN bb.Amount - ISNULL(cb.Amount, 0) ELSE 0 END AS decimal(19, 6)) AS DueToday,
                    CAST (CASE WHEN DATEDIFF(DAY, dbo.udf_current_local_time(), bb.DueDate) >= 1 AND DATEDIFF(DAY, dbo.udf_current_local_time(), bb.DueDate) <= 7 THEN
                    bb.Amount - ISNULL(cb.Amount, 0) ELSE 0 END AS decimal(19, 6)) AS Due1To7,
                    CAST (CASE WHEN DATEDIFF(DAY, dbo.udf_current_local_time(), bb.DueDate) >= 8 AND DATEDIFF(DAY, dbo.udf_current_local_time(), bb.DueDate) <= 14 THEN
                    bb.Amount - ISNULL(cb.Amount, 0) ELSE 0 END AS decimal(19, 6)) AS Due8To14,
                    CAST (CASE WHEN DATEDIFF(DAY, dbo.udf_current_local_time(), bb.DueDate) >= 15 AND DATEDIFF(DAY, dbo.udf_current_local_time(), bb.DueDate) <= 30 THEN
                    bb.Amount - ISNULL(cb.Amount, 0) ELSE 0 END AS decimal(19, 6)) AS Due15To30,
                    CAST (CASE WHEN DATEDIFF(DAY, dbo.udf_current_local_time(), bb.DueDate) > 30 AND DATEDIFF(DAY, dbo.udf_current_local_time(), bb.DueDate) <= 60 THEN
                    bb.Amount - ISNULL(cb.Amount, 0) ELSE 0 END AS decimal(19, 6)) AS Due31To60,
                    CAST (CASE WHEN DATEDIFF(DAY, dbo.udf_current_local_time(), bb.DueDate) > 60 AND DATEDIFF(DAY, dbo.udf_current_local_time(), bb.DueDate) <= 90 THEN
                    bb.Amount - ISNULL(cb.Amount, 0) ELSE 0 END AS decimal(19, 6)) AS Due61To90,
                    CAST (CASE WHEN DATEDIFF(DAY, dbo.udf_current_local_time(), bb.DueDate) > 90 THEN
                    bb.Amount - ISNULL(cb.Amount, 0) ELSE 0 END AS decimal(19, 6)) AS Due90
                    FROM Accounting.vwBeginningBalanceAR bb
                    LEFT JOIN ( 
			                    SELECT cb_d.TransCode AS Code, SUM(cb_d.Amount) AS Amount FROM Finance.GeneralCashBankDetail cb_d
			                    LEFT JOIN Finance.GeneralCashBankHeader cb_h ON cb_h.Code = cb_d.Code
			                    WHERE cb_h.Mark NOT IN ('V', 'REJ') AND cb_d.[Type] = 'AR' AND cb_h.Date <= '{date}'
			                    GROUP BY cb_d.TransCode
			                    ) cb ON cb.Code = bb.Code
                    WHERE bb.IsActive = 1") +
                  @") SELECT * FROM cte_ara_report " + (string.IsNullOrEmpty(duration) ? "" : $"WHERE {duration.Replace("'", "''")} > 0")).ToList();

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