using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Expedition;
using Microsoft.EntityFrameworkCore;

namespace ERP.Web.API.Domain.Services.Expedition
{
    public class EPAPReportService : IEPAPReportService
    {
        private readonly TenantContext _db;
        public EPAPReportService(TenantContext db)
        {
            _db = db;
        }
        public DataSourceResult GetData(int type, string date, string supCode, IEnumerable<Sort> sorts)
        {
            var supData = _db.ReportByExpeditionSuppliers.FromSqlRaw(@"select sp.Code, sp.Initial, sp.Name, count(*) as TotalTrans, sum(ex.Amount) as TotalAmount, CAST (0 as decimal) as PaidAmount, CAST (0 as decimal) as RemainderAmount
                        from General.Supplier sp
                        left join Expedition.ExpeditionInvoiceHeader ex on sp.Code = ex.SupCode
                        Where ex.Mark != 'V'
                        Group by sp.Code, sp.Initial, sp.Name
                        ").ToList();

            var invData = _db.ReportByExpeditionInvoices.FromSqlRaw(@"select ex.Date, ex.DueDate, ex.Code, ex.SupCode, sp.[Name] as SupName, ex.Amount as TotalAmount, CAST (0 as decimal) as PaidAmount, CAST (0 as decimal) as RemainderAmount
                        from Expedition.ExpeditionInvoiceHeader ex
                        left join General.Supplier sp on sp.Code = ex.SupCode
                        where ex.Mark != 'V'
                        group by ex.Date, ex.DueDate, ex.Code, ex.SupCode, sp.[Name], ex.Amount").ToList();

            var cbData = _db.GeneralCashBankHeaders.Where(x => x.Mark != "V" && x.Date <= Convert.ToDateTime(date)).ToList();

            var cbDetail = _db.GeneralCashBankDetails.Where(x => cbData.Select(c => c.Code).Contains(x.Code)).ToList();

            invData = invData.Where(x => x.Date <= Convert.ToDateTime(date)).ToList();

            foreach (var itemInv in invData)
            {
                itemInv.PaidAmount = cbDetail.Where(x => x.TransCode == itemInv.Code).Sum(x => x.TransAmount);
                itemInv.RemainderAmount = itemInv.TotalAmount - itemInv.PaidAmount;
            }

            invData = invData.Where(x => x.RemainderAmount > 0).ToList();

            foreach (var itemSup in supData)
            {
                itemSup.TotalTrans = invData.Where(x => x.SupCode == itemSup.Code).Count();
                itemSup.TotalAmount = invData.Where(x => x.SupCode == itemSup.Code).Sum(x => x.TotalAmount);
                itemSup.PaidAmount = invData.Where(x => x.SupCode == itemSup.Code).Sum(x => x.PaidAmount);
                itemSup.RemainderAmount = invData.Where(x => x.SupCode == itemSup.Code).Sum(x => x.RemainderAmount);
            }

            supData = supData.Where(x => x.TotalTrans > 0).ToList();

            if (type == 1)
            {
                if (!string.IsNullOrEmpty(supCode))
                {
                    invData = invData.Where(x => x.SupCode == supCode).ToList();
                }
                return invData.AsQueryable().ToDataSourceResult(0, invData.Count(), null, sorts);
            }
            else
            {
                if (!string.IsNullOrEmpty(supCode))
                {
                    supData = supData.Where(x => x.Code == supCode).ToList();
                }
                return supData.AsQueryable().ToDataSourceResult(0, supData.Count(), null, sorts);
            }
        }
    }
}
