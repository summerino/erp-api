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
            var cusData = _db.ReportByCustomers.FromSqlRaw("select cs.Code, cs.Initial, cs.Name, count(*) as TotalTrans, sum(dlv.Total) as TotalAmount, sum(cbd.TransAmount) as PaidAmount, (sum(dlv.Total) - sum(cbd.TransAmount)) as RemainderAmount" +
                        " from General.Customer cs" +
                        " left join Sales.SalesDeliveryHeader dlv on dlv.CustCode = cs.Code" +
                        " left join Sales.SalesOrderHeader so on so.Code = dlv.TransCode" +
                        " left join Sales.SalesInvoiceDetail invD on invD.DOCode = dlv.Code" +
                        " left join Sales.SalesInvoiceHeader inv on inv.Code = invD.Code" +
                        " left join Finance.GeneralCashBankDetail cbd on cbd.TransCode = inv.Code" +
                        " left join Finance.GeneralCashBankHeader cb on cb.Code = cbd.Code" +
                        $" Where dlv.Mark IN('A','INV') and inv.Mark IN('A','PP','CMP') and cb.Mark IN('A','CMP') and dlv.Date <= '{date}' and cb.Date <= '{date}'" +
                        (slsId > 0 ? $" and so.SalesBy = {slsId}" : "") +
                        " Group by cs.Code, cs.Initial, cs.Name").AsQueryable();

            

            var dlvData = _db.ReportByDeliveries.FromSqlRaw("select dlv.Date, inv.DueDate, dlv.Code, dlv.TransCode as SrcCode, inv.Code as InvCode, sls.Initial as SlsInitial, sls.FirstName as SlsName, dlv.CustCode, sp.[Name] as CustName, dlv.Total as TotalAmount, cbd.TransAmount as PaidAmount, (dlv.Total - cbd.TransAmount) as RemainderAmount" +
                " from Sales.SalesDeliveryHeader dlv" +
                " left join Sales.SalesOrderHeader so on so.Code = dlv.TransCode" +
                " left join General.Employee sls on sls.Id = so.SalesBy" +
                " left join General.Customer sp on sp.Code = dlv.CustCode" +
                " left join Sales.SalesInvoiceDetail invD on invD.DOCode = dlv.Code" +
                " left join Sales.SalesInvoiceHeader inv on inv.Code = invD.Code" +
                " left join Finance.GeneralCashBankDetail cbd on cbd.TransCode = inv.Code" +
                " left join Finance.GeneralCashBankHeader cb on cb.Code = cbd.Code" +
                $" Where dlv.Mark IN('A','INV') and inv.Mark IN('A','PP','CMP') and cb.Mark IN('A','CMP') and dlv.Date <= '{date}' and cb.Date <= '{date}'" + (slsId > 0 ? $" and so.SalesBy = {slsId}" : "")).AsQueryable();

            if (type == 1)
            {
                if (!string.IsNullOrEmpty(custCode))
                {
                    dlvData = dlvData.Where(x => x.CustCode == custCode);
                }
                return dlvData.ToDataSourceResult(0, dlvData.Count(), null, sorts);
            }
            else
            {
                if (!string.IsNullOrEmpty(custCode))
                {
                    cusData = cusData.Where(x => x.Code == custCode);
                }
                return cusData.ToDataSourceResult(0, cusData.Count(), null, sorts);
            }
        }
    }
}
