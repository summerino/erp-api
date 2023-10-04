using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.Finance;
using ERP.Entity.Inventory;
using ERP.Web.API.Domain.Interfaces.Finance;
using Microsoft.EntityFrameworkCore;

namespace ERP.Web.API.Domain.Services.Finance;

public class CBReportService : ICBReportService
{
    private readonly TenantContext _db;

    public CBReportService(TenantContext db)
    {
        _db = db;
    }

    public DataSourceResult GetData(int? type, string startDate, string endDate, string coaCode, IEnumerable<Sort> sorts)
    {
        List<ReportByAllAccount> reportAC = new();
        List<ReportByAccount> reportA = new();
        List<ReportByAccountDetail> reportAD = new();

        var baseQuery = $@"WITH cte_cb AS (SELECT ISNULL(cb_header.ChequeDate, cb_header.[Date]) AS [Date], cb_header.Code,
	        cb_header.CoaCode AS HeaderCoaCode, cb_detail.CoaCode AS DetailCoaCode, sys_par.[Description], coa.[Name],
	        cb_header.Notes AS HeaderNotes, cb_detail.Notes AS DetailNotes, cb_detail.TransCode,
	        CASE WHEN cb_detail.TypeAmount = 'C' THEN TransAmount ELSE 0 END AS IncomingBalance,
	        CASE WHEN cb_detail.TypeAmount = 'D' THEN TransAmount ELSE 0 END AS OutgoingBalance
	        FROM  Finance.GeneralCashBankDetail cb_detail
	        LEFT JOIN Finance.GeneralCashBankHeader cb_header ON cb_header.Code = cb_detail.Code
	        LEFT JOIN Accounting.COA coa ON coa.Code = cb_detail.CoaCode
	        LEFT JOIN SystemManagement.SystemParameter sys_par ON sys_par.[Value] = cb_detail.CoaCode
	        WHERE cb_header.Mark = 'A'" +
            (!string.IsNullOrEmpty(coaCode) ? $" AND cb_header.CoaCode = {coaCode}" : "") +
            (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate) ? $" AND ISNULL(cb_header.ChequeDate, cb_header.Date) BETWEEN '{startDate}' AND '{endDate}'"
            : !string.IsNullOrEmpty(endDate) ? $" AND ISNULL(cb_header.ChequeDate, cb_header.Date) <= '{endDate}'" : "") + ")";

        var initQuery = $@"
            WITH cte_init_cb AS ( SELECT cb_header.CoaCode, 
            CASE WHEN cb_detail.TypeAmount = 'C' THEN TransAmount ELSE 0 END AS IncomingBalance,
	        CASE WHEN cb_detail.TypeAmount = 'D' THEN TransAmount ELSE 0 END AS OutgoingBalance
	        FROM  Finance.GeneralCashBankDetail cb_detail
	        LEFT JOIN Finance.GeneralCashBankHeader cb_header ON cb_header.Code = cb_detail.Code
	        WHERE cb_header.Mark = 'A' AND ISNULL(cb_header.ChequeDate, cb_header.Date) < '{startDate}'" +
            (!string.IsNullOrEmpty(coaCode) ? $" AND cb_header.CoaCode = {coaCode}" : "") + ")";


        if (type == 1)
        {
            var endBalance = 0m;
            if (!string.IsNullOrEmpty(startDate))
            {
                reportA.AddRange(_db.ReportByAccounts.FromSqlRaw(initQuery + " SELECT NULL AS [Date], 'Saldo Awal' AS Code, NULL AS Notes, CAST(0 AS decimal) AS IncomingBalance," +
                    "CAST(0 AS decimal) AS OutgoingBalance, ISNULL(SUM(IncomingBalance - OutgoingBalance),CAST(0 AS decimal)) AS EndingBalance, CAST(1 AS bit) AS IsBold FROM cte_init_cb").ToList());
                endBalance = reportA.FirstOrDefault().EndingBalance;
            }
            else
            {
                reportA.Add(new ReportByAccount
                {
                    Code = "Saldo Awal",
                    EndingBalance = endBalance,
                    IsBold = true
                });
            }

            var fetchedData = _db.ReportByAccounts.FromSqlRaw(baseQuery +
                @",cte_cb_sum AS ( SELECT [Date], Code, HeaderNotes As Notes, SUM(IncomingBalance) AS IncomingBalance,
                SUM(OutgoingBalance) AS OutgoingBalance, CAST(0 AS decimal) AS EndingBalance, CAST(0 as bit) AS IsBold
                FROM cte_cb GROUP BY [Date],Code,HeaderNotes)
                SELECT *FROM cte_cb_sum ORDER BY [Date], Code").ToList();


            foreach (var item in fetchedData)
            {
                item.EndingBalance = endBalance + item.IncomingBalance.Value - item.OutgoingBalance.Value;
                endBalance = item.EndingBalance;

                reportA.Add(item);
            }

            reportA.Add(new ReportByAccount
            {
                Code = "Saldo Akhir",
                IncomingBalance = reportA.Where(x => !x.IsBold).Sum(x => x.IncomingBalance),
                OutgoingBalance = reportA.Where(x => !x.IsBold).Sum(x => x.OutgoingBalance),
                EndingBalance = endBalance,
                IsBold = true
            });

            return reportA.AsQueryable().ToDataSourceResult(0, reportA.Count, null, null);
        }
        else if (type == 2)
        {
            var endBalance = 0m;
            if (!string.IsNullOrEmpty(startDate))
            {
                reportAD.AddRange(_db.ReportByAccountDetails.FromSqlRaw(initQuery + " SELECT NULL AS [Date], 'Saldo Awal' AS Code, NULL AS Notes," +
                    "NULL AS TransCode, NULL AS CoaCode, NULL AS CoaName, CAST(0 AS decimal) AS IncomingBalance," +
                    "CAST(0 AS decimal) AS OutgoingBalance, ISNULL(SUM(IncomingBalance - OutgoingBalance), CAST(0 AS decimal)) AS EndingBalance, CAST(1 as bit) AS IsBold FROM cte_init_cb").ToList());
                endBalance = reportAD.FirstOrDefault().EndingBalance;
            }
            else
            {
                reportAD.Add(new ReportByAccountDetail
                {
                    Code = "Saldo Awal",
                    EndingBalance = endBalance,
                    IsBold = true
                });
            }

            var fetchedData = _db.ReportByAccountDetails.FromSqlRaw(baseQuery +
                @"SELECT [Date], Code, DetailNotes As Notes, TransCode, DetailCoaCode AS CoaCode, ISNULL([Description],[Name]) AS CoaName,
	            IncomingBalance, OutgoingBalance, CAST(0 AS decimal) AS EndingBalance, CAST(0 as bit) AS IsBold
	            FROM cte_cb ORDER BY [Date], Code").ToList();

            foreach (var item in fetchedData)
            {
                item.EndingBalance = endBalance + item.IncomingBalance.Value - item.OutgoingBalance.Value;
                endBalance = item.EndingBalance;

                reportAD.Add(item);
            }

            reportAD.Add(new ReportByAccountDetail
            {
                Code = "Saldo Akhir",
                IncomingBalance = reportAD.Where(x => !x.IsBold).Sum(x => x.IncomingBalance),
                OutgoingBalance = reportAD.Where(x => !x.IsBold).Sum(x => x.OutgoingBalance),
                EndingBalance = endBalance,
                IsBold = true
            });

            return reportAD.AsQueryable().ToDataSourceResult(0, reportAD.Count, null, null);
        }
        else
        {
            var initData = new List<ReportByAllAccount>();
            if(!string.IsNullOrEmpty(startDate))
            {
                initData = _db.ReportByAllAccounts.FromSqlRaw(initQuery + @"SELECT CoaCode AS Code, NULL AS Name, CAST(0 AS decimal) AS BeginningBalance,
                CAST(0 AS decimal) AS IncomingBalance, CAST(0 AS decimal) AS OutgoingBalance, ISNULL(SUM(IncomingBalance - OutgoingBalance),CAST(0 AS decimal)) AS EndingBalance
	            FROM cte_init_cb GROUP BY CoaCode").ToList();
            }

            var fetchedData = _db.ReportByAllAccounts.FromSqlRaw(baseQuery +
                @"SELECT cb.HeaderCoaCode AS Code, coa.[Name] As [Name], CAST(0 AS decimal) AS BeginningBalance,
	            SUM(cb.IncomingBalance) AS IncomingBalance, SUM(cb.OutgoingBalance) AS OutgoingBalance, CAST(0 AS decimal) AS EndingBalance
	            FROM cte_cb cb
	            LEFT JOIN Accounting.COA coa ON coa.Code = cb.HeaderCoaCode
	            GROUP BY cb.HeaderCoaCode, coa.[Name]").ToList();


            foreach (var item in fetchedData)
            {
                item.BeginningBalance = initData.FirstOrDefault(x => x.Code == item.Code).EndingBalance;
                item.EndingBalance = (item.BeginningBalance + item.IncomingBalance) - item.OutgoingBalance;
                reportAC.Add(item);
            }
            return reportAC.AsQueryable().ToDataSourceResult(0, reportAC.Count, null, sorts);
        }
    }
}