using ERP_API.Domain.Entities;
using ERP_API.Domain.Entities.Sales;
using ERP_API.Domain.Extensions;
using ERP_API.Domain.Interfaces.Sales;
using ERP_API.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_API.Domain.Services.Sales
{
    public class SalesReturnService : GeneralService<SalesReturnHeader>, ISalesReturnService
    {
        public SalesReturnService(TenantContext db)
            : base(db)
        {
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            string search)
        {
            var data = Db.VwSalesReturnHeaders.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = DateTime.TryParse(search, out var searchDate)
                    ? data.Where(x => x.Date == searchDate)
                    : data.Where(x =>
                        x.Code.Contains(search) || x.CustCode.Contains(search) ||
                        x.SalesInitial.Contains(search));
            }

            return data.ToDataSourceResult(skip, take, filter, sort);
        }

        public IEnumerable<VwSalesReturnDetail> GetDetailData(string code, bool? fullDelivered = null)
        {
            var data = Db.VwSalesReturnDetails.Where(x => x.Code == code);

            if (fullDelivered.HasValue)
            {
                data = (bool)fullDelivered
                    ? data.Where(x => x.Qty <= x.QtyDlv)
                    : data.Where(x => x.Qty > x.QtyDlv);
            }
            return data.OrderBy(x => x.LineNo);
        }
       
    }
}
