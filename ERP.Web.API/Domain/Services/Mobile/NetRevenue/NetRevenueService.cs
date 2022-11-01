using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Mobile.NetRevenue;
using ERP.Web.API.Domain.Models.Mobile.General;
using ERP.Web.API.Domain.Models.Mobile.NetRevenue;
using ERP.Web.API.Domain.Models.Mobile.Operational;

namespace ERP.Web.API.Domain.Services.Mobile.NetRevenue;

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
        var salesmanId = Db.Users.Where(x => x.Id.Equals(userId)).Select(x => x.EmployeeId).FirstOrDefault();

        // Income
        var data = from payment in Db.MobilePaymentInvoices.Where(x => x.SalesmanId.Equals(salesmanId)
                                                                    && x.Date.Equals(DateTime.ParseExact(date, "yyyy-MM-dd", null)))
                   join coa in Db.Coas on payment.CoaCode equals coa.Code
                   group new { payment, coa } by new
                   {
                       Date = payment.Date,
                       CoaCode = payment.CoaCode,
                       CoaName = coa.Name,
                   } into gRevenue
                   select new NetRevenueDetailModel
                   {
                       Date = gRevenue.Key.Date,
                       CoaCode = gRevenue.Key.CoaCode,
                       CoaName = gRevenue.Key.CoaName,
                       Amount = gRevenue.Sum(x => x.payment.Amount)
                   };

        data = data.OrderBy(x => x.CoaName);

        return data.ToList();
    }

    public IEnumerable<CostDetailModel> GetCostDetail(string date, int userId)
    {
        var salesmanId = Db.Users.Where(x => x.Id.Equals(userId)).Select(x => x.EmployeeId).FirstOrDefault();

        // Cost in revenue (sub)
        var data = from cost in Db.MobileCostDetails
                   join coa in Db.Coas on cost.CoaCode equals coa.Code
                   join header in Db.MobileCostHeaders.Where(x => x.SalesmanId.Equals(salesmanId)
                                                               && x.Date.Equals(DateTime.ParseExact(date, "yyyy-MM-dd", null)))
                   on cost.Code equals header.Code
                   select new CostDetailModel
                   {
                       Code = cost.Code,
                       LineNo = cost.LineNo,
                       CoaCode = cost.CoaCode,
                       CoaName = coa.Name,
                       Amount = cost.Amount
                   };

        data = data.OrderBy(x => x.CoaName);

        return data.ToList();
    }

    public DataSourceResult GetRevenueDetailByCoa(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, string date, string coaCode, int userId)
    {
        var customer = getCustomers();
        var salesmanId = Db.Users.Where(x => x.Id.Equals(userId)).Select(x => x.EmployeeId).FirstOrDefault();

        var data = from payment in Db.MobilePaymentInvoices.Where(x => x.SalesmanId.Equals(salesmanId)
                                                                    && x.Date.Equals(DateTime.ParseExact(date, "yyyy-MM-dd", null))
                                                                    && x.CoaCode.Equals(coaCode))
                   join cust in customer on payment.CustCode equals cust.Code
                   select new NetRevenueDetailByCoaModel
                   {
                       Code = payment.Code,
                       Date = payment.Date,
                       CustCode = payment.CustCode,
                       CustName = cust.Name,
                       CoaCode = payment.CoaCode,
                       Amount = payment.Amount,
                       TransCode = payment.TransCode
                   };

        return data.ToDataSourceResult(skip, take, filter, sort);
    }

    public IQueryable<CustomerModel> getCustomers()
    {
        // customers and mobile customers
        var dataOriginal = (from cust in Db.VwCustomers
                            select new CustomerModel
                            {
                                Code = cust.Code,
                                Initial = cust.Initial,
                                Name = cust.Name,
                                TypeId = cust.TypeId,
                                TypeName = cust.TypeName,
                                CreditLimit = cust.CreditLimit,
                                Used = cust.CreditUsed,
                                Remaining = cust.CreditLimit - cust.CreditUsed,
                                AreaId1 = cust.AreaId1,
                                AreaId2 = cust.AreaId2,
                                AreaId3 = cust.AreaId3,
                                AreaId4 = cust.AreaId4,
                                AreaId5 = cust.AreaId5,
                                AreaName1 = cust.AreaName1,
                                AreaName2 = cust.AreaName2,
                                AreaName3 = cust.AreaName3,
                                AreaName4 = cust.AreaName4,
                                AreaName5 = cust.AreaName5,
                                Lat = cust.Lat,
                                Lng = cust.Lng,
                                InitialAddress = cust.InitialAddress,
                                Address1 = cust.Address1,
                                Address2 = cust.Address2,
                                Phone = cust.Phone,
                                Fax = cust.Fax,
                                ContactPerson = cust.ContactPerson,
                                IsActive = true,
                                UpdatedDate = cust.UpdatedDate
                            });

        var dataMobile = (from custMobile in Db.VwMobileCustomers
                          where custMobile.CustCode == null && custMobile.Mark == "A"
                          select new CustomerModel
                          {
                              Code = custMobile.Code,
                              Initial = custMobile.Initial,
                              Name = custMobile.Name,
                              TypeId = custMobile.TypeId,
                              TypeName = custMobile.TypeName,
                              CreditLimit = (decimal)0.00,
                              Used = (decimal)0.00,
                              Remaining = (decimal)0.00,
                              AreaId1 = custMobile.AreaId1,
                              AreaId2 = custMobile.AreaId2,
                              AreaId3 = custMobile.AreaId3,
                              AreaId4 = custMobile.AreaId4,
                              AreaId5 = custMobile.AreaId5,
                              AreaName1 = custMobile.AreaName1,
                              AreaName2 = custMobile.AreaName2,
                              AreaName3 = custMobile.AreaName3,
                              AreaName4 = custMobile.AreaName4,
                              AreaName5 = custMobile.AreaName5,
                              Lat = custMobile.Lat,
                              Lng = custMobile.Lng,
                              InitialAddress = custMobile.InitialAddress,
                              Address1 = custMobile.Address1,
                              Address2 = custMobile.Address2,
                              Phone = custMobile.Phone,
                              Fax = custMobile.Fax,
                              ContactPerson = custMobile.ContactPerson,
                              IsActive = true,
                              UpdatedDate = custMobile.UpdatedDate
                          });

        var dataCustomer = dataOriginal.Union(dataMobile).OrderBy(x => x.UpdatedDate).AsQueryable();

        return dataCustomer;
    }
}