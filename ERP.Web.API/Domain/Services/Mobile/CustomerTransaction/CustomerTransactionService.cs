using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Mobile.CustomerTransaction;
using ERP.Web.API.Domain.Models.Mobile.General;
using ERP.Web.API.Domain.Models.Mobile.TransactionHistory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP.Web.API.Domain.Services.Mobile.CustomerTransaction
{
    public class CustomerTransactionService : ICustomerTransactionService
    {
        protected TenantContext Db;

        public CustomerTransactionService(TenantContext db)
        {
            Db = db;
        }

        public CreditLimitModel GetCreditLimit(string custCode)
        {
            var data = (from c in Db.Customers.Where(x => x.Code.Equals(custCode)) select new CreditLimitModel
            {
                CreditLimit=c.CreditLimit,
                CreditUsed=c.CreditUsed
            }).Single();

            return data;
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,DateTime? date,string custCode)
        {
            var data = Db.VwSalesInvoiceHeaders.Where(x => x.CustCode.Equals(custCode)).AsQueryable();

            if (date.HasValue)
            {
                data = data.Where(x => x.Date.Date.Equals(date));
            }
            return data.ToDataSourceResult(skip, take, filter, sort);
        }

        public IEnumerable<TransactionItemDetail> GetTransactionItem(string transNo)
        {
            var data = (from si in Db.SalesInvoiceHeaders.Where(x=>x.Code.Equals(transNo)) 
                        join so in Db.SalesOrderHeaders on si.SoCode equals so.Code
                             join sod in Db.VwSalesOrderDetails on so.Code equals sod.Code
                             join c in Db.Customers on so.CustCode equals c.Code
                             group new { so, sod } by new { so.Date, so.SalesBy, c.Code, sod.ItemId, sod.ItemName, sod.UnitPrice, sod.UomId, sod.UnitName } into g
                             select new TransactionItemDetail
                             {
                                 SalesId = g.Key.SalesBy,
                                 Date = g.Key.Date,
                                 CustomerId = g.Key.Code,
                                 ItemId = g.Key.ItemId,
                                 ItemName = g.Key.ItemName,
                                 Quantity = g.Sum(qt => qt.sod.Qty),
                                 Unit = g.Key.UnitName,
                                 Price = g.Key.UnitPrice,
                                 Discount = g.Sum(dc => dc.sod.Disc),
                                 Total = g.Sum(tl => tl.sod.Total)
                             }).AsQueryable();

            return data;
        }
    }
}
