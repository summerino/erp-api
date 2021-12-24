using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.Purchase;
using ERP.Web.API.Domain.Interfaces.Purchase;
using Microsoft.EntityFrameworkCore;

namespace ERP.Web.API.Domain.Services.Purchase
{
    public class DebitMemoReportService : IDebitMemoReportService
    {
        private readonly TenantContext _db;
        public DebitMemoReportService(TenantContext db)
        {
            _db = db;
        }

        public DataSourceResult GetData(int type, string date, string supCode, string status, IEnumerable<Sort> sorts)
        {
            var supData = _db.ReportBySuppliers.FromSqlRaw(@"SELECT sp.Code, sp.Initial, sp.Name, CAST (0 AS int) AS TotalTrans, CAST (0 AS decimal) AS TotalAmount, CAST (0 AS decimal) AS PaidAmount, CAST (0 AS decimal) AS RemainderAmount
                        FROM General.Supplier sp
                        GROUP BY sp.Code, sp.Initial, sp.Name").ToList();

            var dmData = _db.ReportByDebitMemos.FromSqlRaw(@"SELECT dm.Date, dm.Code, dm.TransCode as SrcCode, dm.SupCode, dm.SupName, dm.Amount, dm.Used AS UsedAmount, CAST (0 AS decimal) AS RemainderAmount
                        FROM Purchasing.vwDebitMemo dm
                        WHERE dm.Mark != 'V'").ToList();

            var cbData = _db.GeneralCashBankHeaders.Where(x => x.Mark != "V" && (x.ChequeDate ?? x.Date) <= Convert.ToDateTime(date)).ToList();

            var bbData = _db.VwBeginningBalanceDebitMemos.Where(x => x.IsActive && x.Date <= Convert.ToDateTime(date)).ToList();

            var cbDetail = _db.GeneralCashBankDetails.Where(x => cbData.Select(c => c.Code).Contains(x.Code)).ToList();

            dmData = dmData.Where(x => x.Date <= Convert.ToDateTime(date)).ToList();

            foreach (var itemDm in dmData)
            {
                var totCb = cbDetail.Where(x => x.TransCode == itemDm.Code).Sum(x => x.TransAmount);
                itemDm.UsedAmount = totCb;
                itemDm.RemainderAmount = itemDm.Amount - itemDm.UsedAmount;
            }

            foreach (var itemBB in bbData)
            {
                var totCb = cbDetail.Where(x => x.TransCode == itemBB.Code).Sum(x => x.TransAmount);
                dmData.Add(new ReportByDebitMemo
                {
                    Date = itemBB.Date,
                    Code = itemBB.Code,
                    SrcCode = "",
                    SupCode = itemBB.SupCode,
                    SupName = itemBB.SupName,
                    Amount = itemBB.Amount,
                    UsedAmount = totCb,
                    RemainderAmount = itemBB.Amount - totCb
                });
            }

            dmData = dmData.Where(x => x.Amount > 0).ToList();

            if (!string.IsNullOrEmpty(status))
            {
                if (status == "A")
                {
                    dmData = dmData.Where(x => x.UsedAmount == 0).ToList();
                }
                else if (status == "PU")
                {
                    dmData = dmData.Where(x => x.RemainderAmount > 0 && x.UsedAmount > 0).ToList();
                }
                else if (status == "FU")
                {
                    dmData = dmData.Where(x => x.RemainderAmount == 0).ToList();
                }
            }

            foreach (var itemSup in supData)
            {
                itemSup.TotalTrans = dmData.Count(x => x.SupCode == itemSup.Code);
                itemSup.TotalAmount = dmData.Where(x => x.SupCode == itemSup.Code).Sum(x => x.Amount);
                itemSup.PaidAmount = dmData.Where(x => x.SupCode == itemSup.Code).Sum(x => x.UsedAmount);
                itemSup.RemainderAmount = dmData.Where(x => x.SupCode == itemSup.Code).Sum(x => x.RemainderAmount);
            }

            supData = supData.Where(x => x.TotalTrans > 0).ToList();

            if (type == 1)
            {
                if (!string.IsNullOrEmpty(supCode))
                {
                    dmData = dmData.Where(x => x.SupCode == supCode).ToList();
                }
                return dmData.AsQueryable().ToDataSourceResult(0, dmData.Count, null, sorts);
            }
            else
            {
                if (!string.IsNullOrEmpty(supCode))
                {
                    supData = supData.Where(x => x.Code == supCode).ToList();
                }
                return supData.AsQueryable().ToDataSourceResult(0, supData.Count, null, sorts);
            }
        }
    }
}
