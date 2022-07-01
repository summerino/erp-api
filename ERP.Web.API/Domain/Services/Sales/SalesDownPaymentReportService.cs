using Microsoft.EntityFrameworkCore;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.Sales;
using ERP.Web.API.Domain.Interfaces.Sales;

namespace ERP.Web.API.Domain.Services.Sales;

public class SalesDownPaymentReportService : ISalesDownPaymentReportService
{
    private readonly TenantContext _db;

    public SalesDownPaymentReportService(TenantContext db)
    {
        _db = db;
    }

    public DataSourceResult GetData(int type, int? srcTrans, string date, string custCode, string status, IEnumerable<Sort> sorts)
    {
        var cusData = _db.ReportByCustomers.FromSqlRaw(@"SELECT cs.Code, cs.Initial, cs.Name, CAST (0 AS int) AS TotalTrans, CAST (0 AS decimal) AS TotalAmount, CAST (0 AS decimal) AS PaidAmount, CAST (0 AS decimal) AS RemainderAmount
                            FROM General.Customer cs
                            GROUP BY cs.Code, cs.Initial, cs.Name").ToList();

        var cmData = _db.ReportByCreditMemos.FromSqlRaw(@"SELECT cm.Date, cm.SrcTrans, cm.Code, cm.TransCode as SrcCode, cm.CustCode, cm.CustName, cm.Amount, cm.Used AS UsedAmount, CAST (0 AS decimal) AS RemainderAmount, cm.Mark
                        FROM Sales.vwCreditMemo cm
                        WHERE cm.Mark != 'V' " +
                        (srcTrans.HasValue ? $"AND cm.SrcTrans = {srcTrans}" : "AND cm.SrcTrans IN (3, 4)")).ToList();

        var cbData = _db.GeneralCashBankHeaders.Where(x => x.Mark != "V" && (x.ChequeDate ?? x.Date) <= Convert.ToDateTime(date)).ToList();

        var cbDetail = _db.GeneralCashBankDetails.Where(x => cbData.Select(c => c.Code).Contains(x.Code)).ToList();

        cmData = cmData.Where(x => x.Date <= Convert.ToDateTime(date)).ToList();

        foreach (var itemCm in cmData)
        {
            var totCb = cbDetail.Where(x => x.TransCode == itemCm.Code).Sum(x => x.TransAmount);
            itemCm.UsedAmount = totCb;
            itemCm.RemainderAmount = itemCm.Amount - itemCm.UsedAmount;
        }

        cmData = cmData.Where(x => x.Amount > 0).ToList();

        if (!string.IsNullOrEmpty(status))
        {
            cmData = status switch
            {
                "PP" or "A" or "PU" or "FU" => cmData.Where(x => x.Mark == status).ToList(),
                "OS" => cmData.Where(x => new[] {"A", "PU"}.Contains(x.Mark)).ToList(),
                _ => cmData
            };
        }

        foreach (var itemCus in cusData)
        {
            itemCus.TotalTrans = cmData.Count(x => x.CustCode == itemCus.Code);
            itemCus.TotalAmount = cmData.Where(x => x.CustCode == itemCus.Code).Sum(x => x.Amount);
            itemCus.PaidAmount = cmData.Where(x => x.CustCode == itemCus.Code).Sum(x => x.UsedAmount);
            itemCus.RemainderAmount = cmData.Where(x => x.CustCode == itemCus.Code).Sum(x => x.RemainderAmount);
        }

        cusData = cusData.Where(x => x.TotalTrans > 0).ToList();

        if (type == 1)
        {
            if (!string.IsNullOrEmpty(custCode))
            {
                cmData = cmData.Where(x => x.CustCode == custCode).ToList();
            }
            return cmData.AsQueryable().ToDataSourceResult(0, cmData.Count, null, sorts);
        }
        else
        {
            if (!string.IsNullOrEmpty(custCode))
            {
                cusData = cusData.Where(x => x.Code == custCode).ToList();
            }
            return cusData.AsQueryable().ToDataSourceResult(0, cusData.Count, null, sorts);
        }
    }
}