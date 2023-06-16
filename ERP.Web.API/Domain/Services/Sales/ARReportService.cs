using Microsoft.EntityFrameworkCore;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Sales;
using Syncfusion.XlsIO.Implementation.XmlSerialization;
using Humanizer;

namespace ERP.Web.API.Domain.Services.Sales;

public class ARReportService : IARReportService
{
    private readonly TenantContext _db;
    public ARReportService(TenantContext db)
    {
        _db = db;
    }
    public DataSourceResult GetData(int type, string date, string custCode, int slsId, IEnumerable<Sort> sorts)
    {
        var sysData = _db.SystemParameters.FirstOrDefault(x => x.Code == "AR_RECOG_TIME");
        if (sysData != null)
        {
            var baseQuery = @$"DECLARE @date VARCHAR(50), @salesId int;
                            SET @date = '{date}';
                            SET @salesId = {slsId};
                            WITH cte_cb_detail_sum AS (
                                SELECT cb_d.TransCode, SUM(cb_d.TransAmount) AS TransAmount
                                FROM Finance.GeneralCashBankDetail cb_d
                                WHERE cb_d.Code IN ( 
                                    SELECT cb_h.Code
                                    FROM Finance.GeneralCashBankHeader cb_h
                                    WHERE cb_h.Mark NOT IN ('V', 'REJ') AND 
                                    ((cb_h.ChequeDate IS NULL AND cb_h.[Date] <= @date) OR (cb_h.ChequeDate IS NOT NULL AND cb_h.ChequeDate <= @date))
                                )
                                GROUP BY cb_d.TransCode
                            ),
                            cte_inv_cm_sum AS (
                                SELECT inv_cm.InvCode AS TransCode, SUM(inv_cm.CreditMemoAmount) AS TransAmount
                                FROM Sales.SalesInvoiceCreditMemo inv_cm
                                WHERE inv_cm.CreditMemoCode IN (
                                    SELECT cm.Code
                                    FROM Sales.CreditMemo cm
                                    WHERE cm.Mark != 'V'
                                    UNION
		                            SELECT bb_cm.Code
		                            FROM Accounting.BeginningBalanceCreditMemo bb_cm
		                            Where bb_cm.IsActive = 1
                                )
                                GROUP BY inv_cm.InvCode
                            ),
                            cte_inv_sum AS (
                                SELECT inv.Code, ISNULL(SUM(cb_d.TransAmount), CAST(0 as decimal(19,8))) + ISNULL(SUM(inv_cm.TransAmount), CAST(0 as decimal(19,8))) AS PaidAmount
                                FROM Sales.SalesInvoiceHeader inv
                                LEFT JOIN cte_cb_detail_sum cb_d ON cb_d.TransCode = inv.Code
                                LEFT JOIN cte_inv_cm_sum inv_cm ON inv_cm.TransCode = inv.Code
                                WHERE inv.[Date] <= @date        
                                GROUP BY inv.Code
                                UNION 
                                SELECT bb_ar.Code, ISNULL(SUM(cb_d.TransAmount), CAST(0 as decimal(19,8))) AS PaidAmount
                                FROM Accounting.vwBeginningBalanceAR bb_ar
                                LEFT JOIN cte_cb_detail_sum cb_d ON cb_d.TransCode = bb_ar.Code
                                WHERE @salesId <= 0 AND bb_ar.[Date] <= @date
                                GROUP BY bb_ar.Code
                            )";
            if (sysData.Value == "SI")
            {
                var invQuery = baseQuery + @", cte_inv AS (
                            SELECT inv.[Date], inv.DueDate, inv.Code, inv.SOCode AS OrderCode, 
                            e.Id AS SalesId, e.FirstName AS SalesName,  
                            inv.CustCode, cust.[Name] AS CustName, 
                            inv.Total AS TotalAmount,
                            inv_sum.PaidAmount AS PaidAmount,
                            inv.Total - inv_sum.PaidAmount AS RemainderAmount
                            FROM Sales.SalesInvoiceHeader inv
                            LEFT JOIN General.Customer cust on cust.Code = inv.CustCode
                            LEFT JOIN Sales.SalesOrderHeader so ON so.Code = inv.SOCode
                            LEFT JOIN General.Employee e ON e.Id = so.SalesBy  
                            LEFT JOIN cte_inv_sum inv_sum ON inv_sum.Code = inv.Code
                            WHERE inv.Mark IN('A', 'PP', 'CMP') AND inv.[Date] <= @date 
                            AND ((inv.[Date] = @date AND inv.Total - inv_sum.PaidAmount >= 0) OR (inv.[Date] < @date AND inv.Total - inv_sum.PaidAmount > 0))" + 
                            (slsId > 0 ? $" AND so.SalesBy = {slsId} " : "") +
                            (!string.IsNullOrEmpty(custCode) ? $" AND inv.CustCode = '{custCode}'" : "") + 
                            @" UNION
                            SELECT bb_ar.[Date], bb_ar.DueDate, bb_ar.Code,
                            '' AS SalesId, '' AS SalesName,
                            '' AS OrderCode, bb_ar.CustCode, bb_ar.CustName,
                            bb_ar.Amount AS TotalAmount, inv_sum.PaidAmount AS PaidAmount,
                            bb_ar.Amount - inv_sum.PaidAmount AS RemainderAmount
                            FROM Accounting.vwBeginningBalanceAR bb_ar
                            LEFT JOIN cte_inv_sum inv_sum ON inv_sum.Code = bb_ar.Code
                            WHERE @salesId <= 0 AND bb_ar.[Date] <= @date AND (bb_ar.Amount - inv_sum.PaidAmount) > 0" +
                            (!string.IsNullOrEmpty(custCode) ? $" AND bb_ar.CustCode = '{custCode}'" : "") + ")";

                var custQuery = invQuery + @"SELECT cs.Code, cs.Initial, cs.[Name],
                            COUNT(inv.CustCode) AS TotalTrans, ISNULL(SUM(inv.TotalAmount), CAST(0 as decimal(19,8))) AS TotalAmount,
                            ISNULL(SUM(inv.PaidAmount), CAST(0 as decimal(19,8))) AS PaidAmount, ISNULL(SUM(inv.RemainderAmount),
                            CAST(0 as decimal(19,8))) AS RemainderAmount
                            FROM General.Customer cs
                            LEFT JOIN cte_inv inv ON inv.CustCode = cs.Code" +
                            (!string.IsNullOrEmpty(custCode) ? $" WHERE cs.Code = '{custCode}'" : "") +
                            @" GROUP BY cs.Code, cs.Initial, cs.[Name]
                            HAVING COUNT(inv.CustCode) > 0";

                if (type == 1)
                {
                    var invData = _db.ReportByInvoiceARs.FromSqlRaw(invQuery + " SELECT *FROM cte_inv").ToList();
                    return invData.AsQueryable().ToDataSourceResult(0, invData.Count, null, sorts);
                }
                else
                {
                    var custData = _db.ReportByCustomers.FromSqlRaw(custQuery).ToList();
                    return custData.AsQueryable().ToDataSourceResult(0, custData.Count, null, sorts);
                }
            }
            else
            {
                var dlvQuery = baseQuery + @", cte_inv AS (
                                SELECT dlv.Date, inv.DueDate, dlv.Code, dlv.TransCode AS SrcCode, inv.Code AS InvCode,
                                sls.Initial AS SlsInitial, sls.FirstName AS SlsName, dlv.CustCode, sp.[Name] AS CustName, dlv.Total AS TotalAmount,
                                ISNULL((inv_sum.PaidAmount * dlv.Total / inv.Total), CAST(0 as decimal(19,8))) AS PaidAmount,
                                (dlv.Total - ISNULL((inv_sum.PaidAmount * dlv.Total / inv.Total), CAST(0 as decimal(19,8)))) AS RemainderAmount
                                FROM Sales.SalesDeliveryHeader dlv
                                LEFT JOIN Sales.SalesOrderHeader so ON so.Code = dlv.TransCode
                                LEFT JOIN General.Employee sls ON sls.Id = so.SalesBy
                                LEFT JOIN General.Customer sp ON sp.Code = dlv.CustCode
                                LEFT JOIN Sales.SalesInvoiceDetail invD ON invD.DOCode = dlv.Code
                                LEFT JOIN Sales.SalesInvoiceHeader inv ON inv.Code = invD.Code and inv.Mark IN('A','PP','CMP')
                                LEFT JOIN cte_inv_sum inv_sum ON inv_sum.Code = inv.Code
                                WHERE dlv.Mark IN ('A','INV') 
                                AND ((dlv.[Date] = @date AND (dlv.Total - ISNULL((inv_sum.PaidAmount * dlv.Total / inv.Total), CAST(0 as decimal(19,8)))) >= 0)
                                OR (dlv.[Date] < @date AND (dlv.Total - ISNULL((inv_sum.PaidAmount * dlv.Total / inv.Total), CAST(0 as decimal(19, 8)))) > 0))" + 
                                (slsId > 0 ? $" AND so.SalesBy = {slsId} " : "") +
                                (!string.IsNullOrEmpty(custCode) ? $" AND inv.CustCode = '{custCode}'" : "") + 
                                @" UNION
                                SELECT bb_ar.[Date], bb_ar.DueDate, bb_ar.Code,
                                '' AS SrcCode, '' AS InvCode,
                                '' AS SlsInitial, '' AS SlsName,
                                bb_ar.CustCode, bb_ar.CustName,
                                bb_ar.Amount AS TotalAmount, inv_sum.PaidAmount AS PaidAmount,
                                bb_ar.Amount - inv_sum.PaidAmount AS RemainderAmount
                                FROM Accounting.vwBeginningBalanceAR bb_ar
                                LEFT JOIN cte_inv_sum inv_sum ON inv_sum.Code = bb_ar.Code
                                WHERE @salesId <= 0 AND bb_ar.[Date] <= @date AND (bb_ar.Amount - inv_sum.PaidAmount) > 0" +
                                (!string.IsNullOrEmpty(custCode) ? $" AND bb_ar.CustCode = '{custCode}'" : "") + ")";

                var custQuery = dlvQuery + @"SELECT cs.Code, cs.Initial, cs.[Name],
                            COUNT(inv.CustCode) AS TotalTrans, ISNULL(SUM(inv.TotalAmount), CAST(0 as decimal(19,8))) AS TotalAmount,
                            ISNULL(SUM(inv.PaidAmount), CAST(0 as decimal(19,8))) AS PaidAmount, ISNULL(SUM(inv.RemainderAmount),
                            CAST(0 as decimal(19,8))) AS RemainderAmount
                            FROM General.Customer cs
                            LEFT JOIN cte_inv inv ON inv.CustCode = cs.Code" +
                            (!string.IsNullOrEmpty(custCode) ? $" WHERE cs.Code = '{custCode}'" : "") +
                            @" GROUP BY cs.Code, cs.Initial, cs.[Name]
                            HAVING COUNT(inv.CustCode) > 0";

                if (type == 1)
                {
                    var dlvData = _db.ReportByDeliveries.FromSqlRaw(dlvQuery + " SELECT *FROM cte_inv").ToList();
                    return dlvData.AsQueryable().ToDataSourceResult(0, dlvData.Count, null, sorts);
                }
                else
                {
                    var custData = _db.ReportByCustomers.FromSqlRaw(custQuery).ToList();
                    return custData.AsQueryable().ToDataSourceResult(0, custData.Count, null, sorts);
                }
            }
        }
        else
        {
            return new DataSourceResult();
        }
    }
}