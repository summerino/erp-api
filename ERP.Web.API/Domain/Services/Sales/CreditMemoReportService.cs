using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.Sales;
using ERP.Web.API.Domain.Interfaces.Sales;
using Microsoft.EntityFrameworkCore;

namespace ERP.Web.API.Domain.Services.Sales
{
    public class CreditMemoReportService : ICreditMemoReportService
    {
        private readonly TenantContext _db;
        public CreditMemoReportService(TenantContext db)
        {
            _db = db;
        }
        public DataSourceResult GetData(int type, string date, string custCode, string status, IEnumerable<Sort> sorts)
        {
            var cusData = _db.ReportByCustomers.FromSqlRaw(@"SELECT cs.Code, cs.Initial, cs.Name, CAST (0 AS int) AS TotalTrans, CAST (0 AS decimal) AS TotalAmount, CAST (0 AS decimal) AS PaidAmount, CAST (0 AS decimal) AS RemainderAmount
                            FROM General.Customer cs
                            GROUP BY cs.Code, cs.Initial, cs.Name").ToList();

            var cmData = _db.ReportByCreditMemos.FromSqlRaw(@"SELECT cm.Date, cm.Code, cm.TransCode as SrcCode, cm.CustCode, cm.CustName, cm.Amount, cm.Used AS UsedAmount, CAST (0 AS decimal) AS RemainderAmount
                        FROM Sales.vwCreditMemo cm
                        WHERE cm.Mark != 'V'").ToList();

            var cbData = _db.GeneralCashBankHeaders.Where(x => x.Mark != "V" && (x.ChequeDate ?? x.Date) <= Convert.ToDateTime(date)).ToList();

            var bbData = _db.VwBeginningBalanceCreditMemos.Where(x => x.IsActive && x.Date <= Convert.ToDateTime(date)).ToList();

            var cbDetail = _db.GeneralCashBankDetails.Where(x => cbData.Select(c => c.Code).Contains(x.Code)).ToList();

            cmData = cmData.Where(x => x.Date <= Convert.ToDateTime(date)).ToList();

            foreach (var itemCm in cmData)
            {
                var totCb = cbDetail.Where(x => x.TransCode == itemCm.Code).Sum(x => x.TransAmount);
                itemCm.UsedAmount = totCb;
                itemCm.RemainderAmount = itemCm.Amount - itemCm.UsedAmount;
            }

            foreach (var itemBB in bbData)
            {
                var totCb = cbDetail.Where(x => x.TransCode == itemBB.Code).Sum(x => x.TransAmount);
                cmData.Add(new ReportByCreditMemo
                {
                    Date = itemBB.Date,
                    Code = itemBB.Code,
                    SrcCode = "",
                    CustCode = itemBB.CustCode,
                    CustName = itemBB.CustName,
                    Amount = itemBB.Amount,
                    UsedAmount = totCb,
                    RemainderAmount = itemBB.Amount - totCb
                });
            }

            cmData = cmData.Where(x => x.Amount > 0).ToList();

            if (!string.IsNullOrEmpty(status))
            {
                if (status == "A")
                {
                    cmData = cmData.Where(x => x.UsedAmount == 0).ToList();
                }
                else if (status == "PU")
                {
                    cmData = cmData.Where(x => x.RemainderAmount > 0 && x.UsedAmount > 0).ToList();
                }
                else if (status == "FU")
                {
                    cmData = cmData.Where(x => x.RemainderAmount == 0).ToList();
                }
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
}
