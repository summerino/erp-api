using Microsoft.EntityFrameworkCore;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.Sales;
using ERP.Web.API.Domain.Interfaces.Sales;

namespace ERP.Web.API.Domain.Services.Sales;

public class ARMutationReportService : IARMutationReportService
{
    private readonly TenantContext _db;
    public ARMutationReportService(TenantContext db)
    {
        _db = db;
    }

    public DataSourceResult GetData(int type, string startDate, string endDate, string custCode, int slsId, string status)
    {
        var sysData = _db.SystemParameters.FirstOrDefault(x => x.Code == "AR_RECOG_TIME")?.Value;
        
        if (string.IsNullOrEmpty(startDate))
        {
            startDate = "1900-01-01";
        }
        
        string whEndDate = "", whBbEndDate = "",whDoEndDate = "", whCbEndDate = "";
        if (!string.IsNullOrEmpty(endDate))
        {
            whEndDate = $"AND inv.[Date] <= '{endDate.Replace("'", "''")}'";
            whBbEndDate = $"AND [Date] <= '{endDate.Replace("'", "''")}'";
            whDoEndDate = $"AND dlv.[Date] <= '{endDate.Replace("'", "''")}'";
            whCbEndDate = $"AND ISNULL(cb_h.ChequeDate, cb_h.[Date]) <= '{endDate.Replace("'", "''")}'";
        }
        
        string whCust = "", whBbCust = "", whDoCust = "";
        if (!string.IsNullOrWhiteSpace(custCode))
        {
            whCust = $"AND inv.CustCode = '{custCode.Replace("'", "''")}'";
            whBbCust = $"AND CustCode = '{custCode.Replace("'", "''")}'";
            whDoCust = $"AND dlv.CustCode = '{custCode.Replace("'", "''")}'";
        }

        string whSales = "", whBbSales = "";
        if (slsId > 0)
        {
            whSales = $"AND so.SalesBy = {slsId}";
            whBbSales = "AND 1=2";
        }
        
        var whStatus = status switch
        {
            "NP" => "WHERE EndingBalance > 0",
            "P" => "WHERE EndingBalance <= 0",
            _ => ""
        };

        var srcQuery = sysData == "SI"
            ? @$"WITH cte_ar_src AS (
                SELECT inv.[Date], inv.DueDate, inv.Code, inv.SOCode AS SrcCode,
		            inv.Code AS InvCode, inv.CustCode, inv.Total, 
		            so.SalesBy, 'SI' AS src
	            FROM Sales.SalesInvoiceHeader inv
	            LEFT JOIN Sales.SalesOrderHeader so
		            ON so.Code = inv.SOCode
                WHERE inv.Mark NOT IN ('V', 'OL') {whEndDate} {whCust} {whSales}
				UNION ALL
				SELECT [Date], DueDate, Code, '' AS SrcCode,
					Code AS InvCode, CustCode, Amount,
					0 AS SalesBy, 'BB' AS src
				FROM Accounting.BeginningBalanceAR
				WHERE IsActive = 1 {whBbEndDate} {whBbCust} {whBbSales}
            )"
            : @$"WITH cte_ar_src AS (
                SELECT dlv.[Date], inv.DueDate, dlv.Code, dlv.TransCode AS SrcCode,
		            inv.Code AS InvCode, dlv.CustCode, dlv.Total,
		            so.SalesBy, 'DO' AS src
                FROM Sales.SalesDeliveryHeader dlv
                LEFT JOIN Sales.SalesOrderHeader so
	                ON so.Code = dlv.TransCode
                LEFT JOIN Sales.SalesInvoiceDetail invD
	                ON invD.DOCode = dlv.Code
                LEFT JOIN Sales.SalesInvoiceHeader inv
	                ON inv.Code = invD.Code AND inv.Mark NOT IN ('V', 'OL')
                WHERE dlv.Mark NOT IN ('V', 'OL')
				AND dlv.SrcTrans = 1 {whDoEndDate} {whDoCust} {whSales}
	            UNION ALL
	            SELECT [Date], DueDate, Code, '' AS SrcCode,
		            '' AS InvCode, CustCode, Amount,
		            0 AS SalesBy, 'BB' AS src
	            FROM Accounting.BeginningBalanceAR
	            WHERE IsActive = 1 {whBbSales}
            )";

        var baseQuery = @$"{srcQuery}
            , cte_cb_src AS (
	            SELECT cb_h.Code, ISNULL(cb_h.ChequeDate, cb_h.[Date]) AS [Date], cb_d.TransCode, cb_d.TransAmount, 'CB' AS Src
	            FROM Finance.GeneralCashBankHeader cb_h
	            LEFT JOIN Finance.GeneralCashBankDetail cb_d
		            ON cb_h.Code = cb_d.Code
	            WHERE cb_h.Mark NOT IN ('V', 'REJ')
	            AND cb_d.[Type] = 'AR' {whCbEndDate}
	            UNION ALL
	            SELECT inv_cm.CreditMemoCode, cte.[Date], inv_cm.InvCode, inv_cm.CreditMemoAmount, inv_cm.Src
	            FROM Sales.SalesInvoiceCreditMemo inv_cm
	            INNER JOIN cte_ar_src cte
		            ON cte.Code = inv_cm.InvCode
            ), cte_inv_cm_src AS (
	            SELECT *
	            FROM Sales.SalesInvoiceCreditMemo inv_cm
	            WHERE EXISTS (
		            SELECT 1
		            FROM cte_ar_src cte
		            WHERE cte.Code = inv_cm.InvCode
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
            ), cte_ar_all AS (
	            SELECT ar.*,
		            ISNULL(cb_b.sum_cb_amount, 0) AS cb_begin_amount,
		            CASE WHEN ar.[Date] < '{startDate.Replace("'", "''")}' THEN ar.Total - ISNULL(cb_b.sum_cb_amount, 0)
			            ELSE 0 END AS BeginningBalance,
		            CASE WHEN ar.[Date] >= '{startDate.Replace("'", "''")}' THEN ar.Total
			            ELSE 0 END AS TransAmount,
		            ISNULL(cb.sum_cb_amount, 0) AS PaidAmount
	            FROM cte_ar_src ar
	            LEFT JOIN cte_cb_begin_calc cb_b
		            ON cb_b.TransCode = ar.InvCode
	            LEFT JOIN cte_cb_calc cb
		            ON cb.TransCode = ar.InvCode
            ), cte_ar_all_calc AS (
                SELECT *,
	                BeginningBalance + TransAmount - PaidAmount AS EndingBalance
                FROM cte_ar_all
            )";

        if (type == 2)
        {
            var custData = _db.ReportByCustomerMutations.FromSqlRaw(@$"{baseQuery}
            SELECT ar.*,
	            c.[Name] AS Name
            FROM (
				SELECT CustCode AS Code,
					COUNT(Code) AS TotalTrans,
					SUM(BeginningBalance) AS BeginningBalance,
					SUM(TransAmount) AS TransAmount,
					SUM(PaidAmount) AS PaidAmount,
					SUM(EndingBalance) AS EndingBalance
				FROM cte_ar_all_calc
                {whStatus}
				GROUP BY CustCode
			) ar
            LEFT JOIN General.Customer c
	            ON c.Code = ar.Code
			ORDER BY ar.Code").ToList();
            
            custData.Add(new ReportByCustomerMutation
            {
                Name = "Total",
                BeginningBalance = custData.Sum(x => x.BeginningBalance),
                TransAmount = custData.Sum(x => x.TransAmount),
                PaidAmount = custData.Sum(x => x.PaidAmount),
                EndingBalance = custData.Sum(x => x.EndingBalance),
            });
            
            return custData.AsQueryable().ToDataSourceResult(0, custData.Count, null, null);
        }
        
        var data = _db.ReportByDeliveryARMutations.FromSqlRaw(@$"{baseQuery}
            SELECT ar.*,
	            sls.FirstName AS SlsName,
	            c.[Name] AS CustName
            FROM cte_ar_all_calc ar
            LEFT JOIN General.Employee sls
	            ON sls.Id = ar.SalesBy
            LEFT JOIN General.Customer c
	            ON c.Code = ar.CustCode {whStatus}
			ORDER BY ar.Code").ToList();
        
        data.Add(new ReportByDeliveryARMutation
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