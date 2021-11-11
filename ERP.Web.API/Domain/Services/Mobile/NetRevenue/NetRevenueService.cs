using System;
using System.Collections.Generic;
using System.Linq;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Mobile.NetRevenue;
using ERP.Web.API.Domain.Models.Mobile.NetRevenue;
using ERP.Web.API.Domain.Models.Mobile.Operational;

namespace ERP.Web.API.Domain.Services.Mobile.NetRevenue
{
    public class NetRevenueService : INetRevenueService
    {
        protected TenantContext Db;
        public NetRevenueService(TenantContext db)
        {
            Db = db;
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, int userId, string date)
        {
            var salesmanId = Db.Users.Where(x => x.Id.Equals(userId)).Select(x => x.EmployeeId).SingleOrDefault();
            var payments = Db.MobilePaymentInvoices.Where(x => x.SalesmanId.Equals(salesmanId)).GroupBy(c => c.Date).Select(z => new NetRevenueHeaderModel
            {
                Date = z.Key,
                Total = z.Sum(s => s.Amount)
            });

            var cost = Db.MobileCostHeaders.Where(x => x.SalesmanId.Equals(salesmanId)).GroupBy(c => c.Date).Select(z => new NetRevenueHeaderModel
            {
                Date = z.Key,
                Total = z.Sum(s => s.Total)
            });

            var leftoutrtjoin = (from p in payments
                                 join c in cost on p.Date equals c.Date into pc
                                 from c in pc.DefaultIfEmpty()
                                 select new NetRevenueHeaderModel
                                 {
                                     Date = p.Date,
                                     Total = (p.Total ?? 0) - (c.Total ?? 0)
                                 });

            var rightouterjoin = (from c in cost
                                  join p in payments on c.Date equals p.Date into ot
                                  from p in ot.DefaultIfEmpty()
                                  select new NetRevenueHeaderModel
                                  {
                                      Date = c.Date,
                                      Total = (p.Total ?? 0) - (c.Total ?? 0)
                                  });

            var data = leftoutrtjoin.Union(rightouterjoin);
            
            if (date != null && date != "")
            {
                var date1 = DateTime.ParseExact(date, "yyyy-MM-dd", null);
                data = data.Where(x => x.Date.Equals(date1));
            }

            return data.ToDataSourceResult(skip, take, filter, sort);
        }

        public IEnumerable<NetRevenueDetailModel> GetRevenueDetail(string date, int userId)
        {
            var data = from payment in Db.MobilePaymentInvoices
                       join cu in Db.Customers on payment.CustCode equals cu.Code
                       join coa in Db.Coas on payment.CoaCode equals coa.Code
                       join user in Db.Users on userId equals user.Id
                       where payment.SalesmanId.Equals(user.EmployeeId) && payment.Date.Equals(DateTime.ParseExact(date, "yyyy-MM-dd", null))
                       select new NetRevenueDetailModel
                       {
                           Code = payment.Code,
                           Date = payment.Date,
                           CoaCode = payment.CoaCode,
                           CoaName = coa.Name,
                           Amount = payment.Amount,
                       };
            return data.ToList();
        }

        public IEnumerable<CostDetailModel> GetCostDetail(string date, int userId)
        {
            var data = from cost in Db.MobileCostDetails
                       join coa in Db.Coas on cost.CoaCode equals coa.Code
                       join header in Db.MobileCostHeaders on DateTime.ParseExact(date, "yyyy-MM-dd", null) equals header.Date
                       join user in Db.Users on userId equals user.Id
                       where header.SalesmanId.Equals(user.EmployeeId) && cost.Code.Equals(header.Code)
                       select new CostDetailModel
                       {
                           Code = cost.Code,
                           LineNo = cost.LineNo,
                           CoaCode = cost.CoaCode,
                           CoaName = coa.Name,
                           Amount = cost.Amount
                       };
            return data.ToList();
        }

        public DataSourceResult GetRevenueDetailByCoa(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, string date, string coaCode, int userId)
        {
            var data = from payment in Db.MobilePaymentInvoices
                       join customer in Db.Customers on payment.CustCode equals customer.Code
                       join user in Db.Users on userId equals user.Id
                       where payment.Date.Equals(DateTime.ParseExact(date, "yyyy-MM-dd", null)) && payment.CoaCode.Equals(coaCode)
                       select new NetRevenueDetailByCoaModel
                       {
                           Code = payment.Code,
                           Date = payment.Date,
                           CustCode = payment.CustCode,
                           CustName = customer.Name,
                           CoaCode = payment.CoaCode,
                           Amount = payment.Amount,
                           TransCode = payment.TransCode,
                       };
            return data.ToDataSourceResult(skip, take, filter, sort);
        }
    }
}
