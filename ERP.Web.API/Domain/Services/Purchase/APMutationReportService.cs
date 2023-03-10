using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.Purchase;
using ERP.Web.API.Domain.Interfaces.Purchase;
using Microsoft.EntityFrameworkCore;

namespace ERP.Web.API.Domain.Services.Purchase;

public class APMutationReportService : IAPMutationReportService
{
    private readonly TenantContext _db;
    public APMutationReportService(TenantContext db)
    {
        _db = db;
    }
    public DataSourceResult GetData(int type, string startDate, string endDate, string supCode, string status)
    {
        var sysData = _db.SystemParameters.FirstOrDefault(x => x.Code == "AP_RECOG_TIME")?.Value;
        
        if (string.IsNullOrEmpty(startDate))
        {
            startDate = "1900-01-01";
        }
        
        string whEndDate = "", whBbEndDate = "", whRcvEndDate = "", whCbEndDate = "";
        if (!string.IsNullOrEmpty(endDate))
        {
            whEndDate = $"AND inv.[Date] <= '{endDate.Replace("'", "''")}'";
            whBbEndDate = $"AND [Date] <= '{endDate.Replace("'", "''")}'";
            whRcvEndDate = $"AND rcv.[Date] <= '{endDate.Replace("'", "''")}'";
            whCbEndDate = $"AND ISNULL(cb_h.ChequeDate, cb_h.[Date]) <= '{endDate.Replace("'", "''")}'";
        }
        
        string whSup = "", whBbSup = "", whRcvSup = "";
        if (!string.IsNullOrWhiteSpace(supCode))
        {
            whSup = $"AND inv.SupCode = '{supCode.Replace("'", "''")}'";
            whBbSup = $"AND SupCode = '{supCode.Replace("'", "''")}'";
            whRcvSup = $"AND rcv.SupCode = '{supCode.Replace("'", "''")}'";
        }

        var whStatus = status switch
        {
            "NP" => "WHERE EndingBalance > 0",
            "P" => "WHERE EndingBalance <= 0",
            _ => ""
        };

        var srcQuery = sysData == "PI"
            ? @$"WITH cte_ap_src AS (
                SELECT inv.[Date], inv.DueDate, inv.Code, inv.POCode AS OrderCode,
		            inv.Code AS InvCode, inv.SupCode, inv.Total, 
		            'PI' AS src
	            FROM Purchasing.PurchaseInvoiceHeader inv
	            LEFT JOIN Purchasing.PurchaseOrderHeader po
		            ON po.Code = inv.POCode
                WHERE inv.Mark <> 'V' {whEndDate} {whSup}
				UNION ALL
				SELECT [Date], DueDate, Code, '' AS OrderCode,
					Code AS InvCode, SupCode, Amount,
					'BB' AS src
				FROM Accounting.BeginningBalanceAP
				WHERE IsActive = 1 {whBbEndDate} {whBbSup}
            )"
            : @$"WITH cte_ap_src AS (
                SELECT rcv.[Date], inv.DueDate, rcv.Code, rcv.TransCode AS OrderCode,
		            inv.Code AS InvCode, rcv.SupCode, rcv.Total,
		            'DO' AS src
                FROM Purchasing.PurchaseReceiveHeader rcv
                LEFT JOIN Purchasing.PurchaseOrderHeader po
	                ON po.Code = rcv.TransCode
                LEFT JOIN Purchasing.PurchaseInvoiceDetail invD
	                ON invD.RcvCode = rcv.Code
                LEFT JOIN Purchasing.PurchaseInvoiceHeader inv
	                ON inv.Code = invD.Code AND inv.Mark <> 'V'
                WHERE rcv.Mark <> 'V'
                AND rcv.SrcTrans = 1 {whRcvEndDate} {whRcvSup}
	            UNION ALL
	            SELECT [Date], DueDate, Code, '' AS SrcCode,
		            '' AS InvCode, SupCode, Amount,
		            'BB' AS src
	            FROM Accounting.BeginningBalanceAP
	            WHERE IsActive = 1 {whBbSup}
            )";
        
        var baseQuery = @$"{srcQuery}
            , cte_cb_src AS (
	            SELECT cb_h.Code, ISNULL(cb_h.ChequeDate, cb_h.[Date]) AS [Date], cb_d.TransCode, cb_d.TransAmount, 'CB' AS Src
	            FROM Finance.GeneralCashBankHeader cb_h
	            LEFT JOIN Finance.GeneralCashBankDetail cb_d
		            ON cb_h.Code = cb_d.Code
	            WHERE cb_h.Mark NOT IN ('V', 'REJ')
	            AND cb_d.[Type] = 'AP' {whCbEndDate}
	            UNION ALL
	            SELECT inv_dm.DebitMemoCode, cte.[Date], inv_dm.InvCode, inv_dm.DebitMemoAmount, 'DM' AS Src
	            FROM Purchasing.PurchaseInvoiceDebitMemo inv_dm
	            INNER JOIN cte_ap_src cte
		            ON cte.Code = inv_dm.InvCode
            ), cte_inv_dm_src AS (
	            SELECT *
	            FROM Purchasing.PurchaseInvoiceDebitMemo inv_dm
	            WHERE EXISTS (
		            SELECT 1
		            FROM cte_ap_src cte
		            WHERE cte.Code = inv_dm.InvCode
	            )
            ), cte_cb_begin_calc AS (
	            SELECT TransCode, SUM(TransAmount) AS sum_cb_amount
	            FROM cte_cb_src
	            WHERE [Date] < '{startDate.Replace("'", "''")}'
	            GROUP BY TransCode
            ), cte_cb_calc AS (
	            SELECT TransCode, SUM(TransAmount) AS sum_cb_amount
	            FROM cte_cb_src
	            WHERE [Date] >= '{startDate.Replace("'", "''")}'
	            GROUP BY TransCode
            ), cte_ap_all AS (
	            SELECT ap.*,
		            ISNULL(cb_b.sum_cb_amount, 0) AS cb_begin_amount,
		            CASE WHEN ap.[Date] < '{startDate.Replace("'", "''")}' THEN ap.Total - ISNULL(cb_b.sum_cb_amount, 0)
			            ELSE 0 END AS BeginningBalance,
		            CASE WHEN ap.[Date] >= '{startDate.Replace("'", "''")}' THEN ap.Total
			            ELSE 0 END AS TransAmount,
		            ISNULL(cb.sum_cb_amount, 0) AS PaidAmount
	            FROM cte_ap_src ap
	            LEFT JOIN cte_cb_begin_calc cb_b
		            ON cb_b.TransCode = ap.InvCode
	            LEFT JOIN cte_cb_calc cb
		            ON cb.TransCode = ap.InvCode
            ), cte_ap_all_calc AS (
                SELECT *,
	                BeginningBalance + TransAmount - PaidAmount AS EndingBalance
                FROM cte_ap_all
            )";
        
        if (type == 2)
        {
            var custData = _db.ReportBySupplierMutations.FromSqlRaw(@$"{baseQuery}
            SELECT ap.*,
	            s.[Name] AS Name
            FROM (
				SELECT SupCode AS Code,
					COUNT(Code) AS TotalTrans,
					SUM(BeginningBalance) AS BeginningBalance,
					SUM(TransAmount) AS TransAmount,
					SUM(PaidAmount) AS PaidAmount,
					SUM(EndingBalance) AS EndingBalance
				FROM cte_ap_all_calc
                {whStatus}
				GROUP BY SupCode
			) ap
            LEFT JOIN General.Supplier s
	            ON s.Code = ap.Code
            ORDER BY ap.Code").ToList();
            
            custData.Add(new ReportBySupplierMutation
            {
                Name = "Total",
                BeginningBalance = custData.Sum(x => x.BeginningBalance),
                TransAmount = custData.Sum(x => x.TransAmount),
                PaidAmount = custData.Sum(x => x.PaidAmount),
                EndingBalance = custData.Sum(x => x.EndingBalance),
            });
            
            return custData.AsQueryable().ToDataSourceResult(0, custData.Count, null, null);
        }
        
        var data = _db.ReportByInvoiceAPMutations.FromSqlRaw(@$"{baseQuery}
            SELECT ap.*,
	            s.[Name] AS SupName
            FROM cte_ap_all_calc ap
            LEFT JOIN General.Supplier s
	            ON s.Code = ap.SupCode {whStatus}
            ORDER BY ap.Code").ToList();
        
        data.Add(new ReportByInvoiceAPMutation
        {
            Code = "Total",
            BeginningBalance = data.Sum(x => x.BeginningBalance),
            TransAmount = data.Sum(x => x.TransAmount),
            PaidAmount = data.Sum(x => x.PaidAmount),
            EndingBalance = data.Sum(x => x.EndingBalance),
        });

        return data.AsQueryable().ToDataSourceResult(0, data.Count, null, null);
    }
}