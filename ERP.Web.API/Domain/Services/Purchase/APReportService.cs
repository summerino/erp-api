using Microsoft.EntityFrameworkCore;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Purchase;

namespace ERP.Web.API.Domain.Services.Purchase
{
    public class APReportService : IAPReportService
    {
        private readonly TenantContext _db;
        public APReportService(TenantContext db)
        {
            _db = db;
        }
        public DataSourceResult GetData(int type, string date, string supCode, IEnumerable<Sort> sorts)
        {
            var supData = _db.ReportBySuppliers.FromSqlRaw(@"select sp.Code, sp.Initial, sp.Name, count(*) as TotalTrans, sum(rcv.Total) as TotalAmount, CAST (0 as decimal) as PaidAmount, CAST (0 as decimal) as RemainderAmount
                        from General.Supplier sp
                        left join Purchasing.PurchaseReceiveHeader rcv on rcv.SupCode = sp.Code
                        left join Purchasing.PurchaseInvoiceDetail invD on invD.RcvCode = rcv.Code
                        left join Purchasing.PurchaseInvoiceHeader inv on inv.Code = invD.Code and inv.Mark IN('A', 'PP', 'CMP')
                        Where rcv.Mark IN('A', 'INV') and rcv.SrcTrans = 1 Group by sp.Code, sp.Initial, sp.Name").ToList();

            var rcvData = _db.ReportByReceives.FromSqlRaw(@"select rcv.Date, inv.DueDate, rcv.Code, rcv.TransCode as SrcCode, inv.Code as InvCode, rcv.SupCode, sp.[Name] as SupName, rcv.Total as TotalAmount, CAST (0 as decimal) as PaidAmount, CAST (0 as decimal) as RemainderAmount
                        from Purchasing.PurchaseReceiveHeader rcv
                        left join General.Supplier sp on sp.Code = rcv.SupCode
                        left join Purchasing.PurchaseInvoiceDetail invD on invD.RcvCode = rcv.Code
                        left join Purchasing.PurchaseInvoiceHeader inv on inv.Code = invD.Code and inv.Mark IN('A', 'PP', 'CMP')
                        Where rcv.Mark IN('A', 'INV') and rcv.SrcTrans = 1").ToList();

            var cbData = _db.GeneralCashBankHeaders.Where(x => x.Mark != "V" && (x.ChequeDate ?? x.Date) <= Convert.ToDateTime(date)).ToList();

            var bbData = _db.VwBeginningBalanceAPs.Where(x => x.IsActive && x.Date <= Convert.ToDateTime(date)).ToList();

            var cbDetail = _db.GeneralCashBankDetails.Where(x => cbData.Select(c => c.Code).Contains(x.Code)).ToList();

            rcvData = rcvData.Where(x => x.Date <= Convert.ToDateTime(date)).ToList();

            foreach (var itemRcv in rcvData)
            {
                var totInv = rcvData.Where(x => x.InvCode == itemRcv.InvCode).Sum(x => x.TotalAmount);
                var totCb = cbDetail.Where(x => x.TransCode == itemRcv.InvCode).Sum(x => x.TransAmount);
                itemRcv.PaidAmount = totCb * itemRcv.TotalAmount / totInv;
                itemRcv.RemainderAmount = itemRcv.TotalAmount - itemRcv.PaidAmount;
            }

            foreach (var itemBB in bbData)
            {
                rcvData.Add(new Entity.Purchase.ReportByReceive
                {
                    Date = itemBB.Date,
                    DueDate = itemBB.DueDate,
                    Code = itemBB.Code,
                    SrcCode = "",
                    InvCode = "",
                    SupCode = itemBB.SupCode,
                    SupName = itemBB.SupName,
                    TotalAmount = itemBB.Amount,
                    PaidAmount = itemBB.PaidAmount,
                    RemainderAmount = itemBB.Amount - itemBB.PaidAmount
                });
            }

            rcvData = rcvData.Where(x => x.RemainderAmount > 0).ToList();

            foreach (var itemSup in supData)
            {
                itemSup.TotalTrans = rcvData.Count(x => x.SupCode == itemSup.Code);
                itemSup.TotalAmount = rcvData.Where(x => x.SupCode == itemSup.Code).Sum(x => x.TotalAmount);
                itemSup.PaidAmount = rcvData.Where(x => x.SupCode == itemSup.Code).Sum(x => x.PaidAmount);
                itemSup.RemainderAmount = rcvData.Where(x => x.SupCode == itemSup.Code).Sum(x => x.RemainderAmount);
            }

            supData = supData.Where(x => x.TotalTrans > 0).ToList();

            if (type == 1)
            {
                if (!string.IsNullOrEmpty(supCode))
                {
                    rcvData = rcvData.Where(x => x.SupCode == supCode).ToList();
                }
                return rcvData.AsQueryable().ToDataSourceResult(0, rcvData.Count, null, sorts);
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
