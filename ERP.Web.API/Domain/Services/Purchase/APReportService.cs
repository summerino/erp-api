using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Purchase;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ERP.Web.API.Domain.Services.Purchase
{
    public class APReportService : IAPReportService
    {
        private readonly TenantContext _db;
        public APReportService(TenantContext db)
        {
            _db = db;
        }
        public DataSourceResult GetData(int type, string date, string supCode, IEnumerable<Sort> sorts, string search)
        {
            var supData = _db.ReportBySuppliers.FromSqlRaw("select sp.Code, sp.Initial, sp.Name, count(*) as TotalTrans, sum(rcv.Total) as TotalAmount, sum(cbd.TransAmount) as PaidAmount, (sum(rcv.Total) - sum(cbd.TransAmount)) as RemainderAmount" +
                " from General.Supplier sp" +
                " left join Purchasing.PurchaseReceiveHeader rcv on rcv.SupCode = sp.Code" +
                " left join Purchasing.PurchaseInvoiceDetail invD on invD.RcvCode = rcv.Code" +
                " left join Purchasing.PurchaseInvoiceHeader inv on inv.Code = invD.Code" +
                " left join Finance.GeneralCashBankDetail cbd on cbd.TransCode = inv.Code" +
                " left join Finance.GeneralCashBankHeader cb on cb.Code = cbd.Code" +
                $" Where rcv.Mark IN('A', 'INV') and inv.Mark IN('A', 'PP', 'CMP') and cb.Mark IN('A', 'CMP') and rcv.Date <= '{date}' and cb.Date <= '{date}'" +
                " Group by sp.Code, sp.Initial, sp.Name").AsQueryable();

            var rcvData = _db.ReportByReceives.FromSqlRaw("select rcv.Date, inv.DueDate, rcv.Code, rcv.TransCode as SrcCode, inv.Code as InvCode, rcv.SupCode, sp.[Name] as SupName, rcv.Total as TotalAmount, cbd.TransAmount as PaidAmount, (rcv.Total - cbd.TransAmount) as RemainderAmount" +
                " from Purchasing.PurchaseReceiveHeader rcv" +
                " left join General.Supplier sp on sp.Code = rcv.SupCode" +
                " left join Purchasing.PurchaseInvoiceDetail invD on invD.RcvCode = rcv.Code" +
                " left join Purchasing.PurchaseInvoiceHeader inv on inv.Code = invD.Code" +
                " left join Finance.GeneralCashBankDetail cbd on cbd.TransCode = inv.Code" +
                " left join Finance.GeneralCashBankHeader cb on cb.Code = cbd.Code" +
                $" Where rcv.Mark IN('A', 'INV') and inv.Mark IN('A', 'PP', 'CMP') and cb.Mark IN('A', 'CMP') and rcv.Date <= '{date}' and cb.Date <= '{date}'").AsQueryable();

            if (type == 1)
            {
                if (!string.IsNullOrEmpty(search) || !string.IsNullOrEmpty(supCode))
                {
                    rcvData = DateTime.TryParse(search, out var searchDate)
                    ? rcvData.Where(x => x.Date == searchDate || x.DueDate == searchDate)
                    : rcvData.Where(x =>
                        x.Code.Contains(search) || x.SupName.Contains(search) || x.SrcCode.Contains(search) ||
                        x.InvCode.Contains(search) || x.SupCode.Contains(search) || x.SupCode == supCode);
                }
                return rcvData.ToDataSourceResult(0, rcvData.Count(), null, sorts);
            }
            else
            {
                if (!string.IsNullOrEmpty(search) || !string.IsNullOrEmpty(supCode))
                {
                    supData = supData.Where(x => x.Code.Contains(search) || x.Name.Contains(search) || x.Code == supCode);
                }
                return supData.ToDataSourceResult(0, supData.Count(), null, sorts);
            }
        }
    }
}
