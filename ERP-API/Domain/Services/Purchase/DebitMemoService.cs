using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using ERP_API.Domain.Entities;
using ERP_API.Domain.Entities.Purchase;
using ERP_API.Domain.Extensions;
using ERP_API.Domain.Interfaces.Purchase;
using ERP_API.Domain.Models;

namespace ERP_API.Domain.Services.Purchase
{
    public class DebitMemoService : GeneralService<DebitMemo>, IDebitMemoService
    {
        public DebitMemoService(TenantContext db)
            : base(db)
        {
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            string search)
        {
            var data = Db.VwDebitMemos.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = DateTime.TryParse(search, out var searchDate)
                    ? data.Where(x => x.Date == searchDate)
                    : data.Where(x =>
                        x.Code.Contains(search) || x.SupName.Contains(search) || x.SrcTransName == search ||
                        x.TransCode.Contains(search));
            }

            return data.ToDataSourceResult(skip, take, filter, sort);
        }

        public List<dynamic> GetRelatedTransactions(string code)
        {
            var piD = from dt in Db.PurchaseInvoiceDetails
                      where dt.RcvCode == code
                      select dt.Code;

            var data = from piH in Db.PurchaseInvoiceHeaders
                       where piD.Contains(piH.Code) && piH.Mark == "A"
                       select new { piH.Code, piH.Date, piH.Total };

            return data.ToDynamicList();
        }
    }
}
