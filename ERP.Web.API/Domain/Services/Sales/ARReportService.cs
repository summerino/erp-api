using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Sales;

namespace ERP.Web.API.Domain.Services.Sales
{
    public class ARReportService : IARReportService
    {
        private readonly TenantContext _db;
        public ARReportService(TenantContext db)
        {
            _db = db;
        }
        public DataSourceResult GetData(int type, string date, string custCode, int slsId, IEnumerable<Sort> sorts)
        {
            var cusData = _db.ReportByCustomers.FromSqlRaw(@"select cs.Code, cs.Initial, cs.Name, count(*) as TotalTrans, sum(dlv.Total) as TotalAmount, CAST (0 as decimal) as PaidAmount, CAST (0 as decimal) as RemainderAmount
                            from General.Customer cs
                            left join Sales.SalesDeliveryHeader dlv on dlv.CustCode = cs.Code
                            left join Sales.SalesInvoiceDetail invD on invD.DOCode = dlv.Code
                            left join Sales.SalesInvoiceHeader inv on inv.Code = invD.Code and inv.Mark IN('A', 'PP', 'CMP')
                            Where dlv.Mark IN('A', 'INV') Group by cs.Code, cs.Initial, cs.Name").ToList();

            var dlvData = _db.ReportByDeliveries.FromSqlRaw(@"select dlv.Date, inv.DueDate, dlv.Code, dlv.TransCode as SrcCode, inv.Code as InvCode, sls.Initial as SlsInitial, sls.FirstName as SlsName, dlv.CustCode, sp.[Name] as CustName, dlv.Total as TotalAmount, cast(0 as decimal) as PaidAmount, cast(0 as decimal) as RemainderAmount
                            from Sales.SalesDeliveryHeader dlv
                            left join Sales.SalesOrderHeader so on so.Code = dlv.TransCode
                            left join General.Employee sls on sls.Id = so.SalesBy
                            left join General.Customer sp on sp.Code = dlv.CustCode
                            left join Sales.SalesInvoiceDetail invD on invD.DOCode = dlv.Code
                            left join Sales.SalesInvoiceHeader inv on inv.Code = invD.Code and inv.Mark IN('A','PP','CMP')
                            Where dlv.Mark IN('A','INV')" + (slsId > 0 ? $" and so.SalesBy = {slsId} " : " ") + "").ToList();

            var cbData = _db.GeneralCashBankHeaders.Where(x => x.Mark != "V" && (x.ChequeDate ?? x.Date) <= Convert.ToDateTime(date)).ToList();

            var cbDetail = _db.GeneralCashBankDetails.Where(x => cbData.Select(c => c.Code).Contains(x.Code)).ToList();

            dlvData = dlvData.Where(x => x.Date <= Convert.ToDateTime(date)).ToList();

            foreach (var itemDlv in dlvData)
            {
                var totDlv = dlvData.Where(x => x.InvCode == itemDlv.InvCode).Sum(x => x.TotalAmount);
                var totCb = cbDetail.Where(x => x.TransCode == itemDlv.InvCode).Sum(x => x.TransAmount);
                itemDlv.PaidAmount = totCb * itemDlv.TotalAmount / totDlv;
                itemDlv.RemainderAmount = itemDlv.TotalAmount - itemDlv.PaidAmount;
            }

            dlvData = dlvData.Where(x => x.RemainderAmount > 0).ToList();

            foreach (var itemCus in cusData)
            {
                itemCus.TotalTrans = dlvData.Count(x => x.CustCode == itemCus.Code);
                itemCus.TotalAmount = dlvData.Where(x => x.CustCode == itemCus.Code).Sum(x => x.TotalAmount);
                itemCus.PaidAmount = dlvData.Where(x => x.CustCode == itemCus.Code).Sum(x => x.PaidAmount);
                itemCus.RemainderAmount = dlvData.Where(x => x.CustCode == itemCus.Code).Sum(x => x.RemainderAmount);
            }

            cusData = cusData.Where(x => x.TotalTrans > 0).ToList();

            if (type == 1)
            {
                if (!string.IsNullOrEmpty(custCode))
                {
                    dlvData = dlvData.Where(x => x.CustCode == custCode).ToList();
                }
                return dlvData.AsQueryable().ToDataSourceResult(0, dlvData.Count, null, sorts);
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
