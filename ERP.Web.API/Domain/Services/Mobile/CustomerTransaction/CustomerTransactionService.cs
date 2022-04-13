using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Mobile.CustomerTransaction;
using ERP.Web.API.Domain.Models.Mobile.General;
using ERP.Web.API.Domain.Models.Mobile.TransactionHistory;

namespace ERP.Web.API.Domain.Services.Mobile.CustomerTransaction;

public class CustomerTransactionService : ICustomerTransactionService
{
    protected TenantContext Db;

    public CustomerTransactionService(TenantContext db)
    {
        Db = db;
    }

    public CreditLimitModel GetCreditLimit(string custCode)
    {
        var data = (from c in Db.Customers.Where(x => x.Code.Equals(custCode))
            select new CreditLimitModel
            {
                CreditLimit = c.CreditLimit,
                CreditUsed = c.CreditUsed
            }).Single();

        return data;
    }

    public CustomerProfileModel GetCustomerProfile(string custCode)
    {
        var data = (from cust in Db.VwCustomers
            where cust.Code.Equals(custCode)
            select new CustomerProfileModel
            {
                Code = custCode,
                Initial = cust.Initial,
                Name = cust.Name,
                Address1 = cust.Address1,
                Address2 = cust.Address2
            }).FirstOrDefault();

        return data;
    }

    public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, DateTime? date, string custCode)
    {
        var data = (from si in Db.VwSalesInvoiceHeaders.Where(x => x.CustCode.Equals(custCode))
            join so in Db.SalesOrderHeaders on si.SoCode equals so.Code
            select new
            {
                si.Code,
                si.Date,
                so.FinalDiscPercent,
                so.FinalDisc,
                so.SubTotal,
                so.TaxAmount,
                si.PaidAmount,
                si.Total,
                si.Remaining
            }).AsQueryable();

        if (date.HasValue)
        {
            data = data.Where(x => x.Date.Date.Equals(date));
        }
        return data.ToDataSourceResult(skip, take, filter, sort);
    }

    public IEnumerable<TransactionItemDetail> GetTransactionItem(string transNo)
    {
        var data = (from si in Db.SalesInvoiceHeaders.Where(x => x.Code.Equals(transNo))
            join sd in Db.SalesDeliveryHeaders on si.Code equals sd.TransCode
            join sdd in Db.VwSalesDeliveryDetails on sd.Code equals sdd.Code
            join c in Db.Customers on sd.CustCode equals c.Code
            group new { si, sd, sdd } by new { si.Date, c.Code, sdd.ItemId, sdd.ItemName, sdd.UnitPrice, sdd.UomId, sdd.UnitName } into g
            select new TransactionItemDetail
            {
                Date = g.Key.Date,
                CustomerId = g.Key.Code,
                ItemId = g.Key.ItemId,
                ItemName = g.Key.ItemName,
                Quantity = g.Sum(qt => qt.sdd.Qty),
                Unit = g.Key.UnitName,
                Price = g.Key.UnitPrice,
                Discount = g.Sum(dc => dc.sdd.Disc),
                Total = g.Sum(tl => tl.sdd.Total)
            }).AsQueryable();
        data.OrderBy(x => x.Date);

        return data;
    }
}