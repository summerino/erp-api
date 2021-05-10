using ERP_API.Domain.Entities.Sales;
using ERP_API.Domain.Models;
using System.Collections.Generic;

namespace ERP_API.Domain.Interfaces.Sales
{
    public interface ISalesReturnService : IGeneralService<SalesReturnHeader>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            string search);
        IEnumerable<VwSalesReturnDetail> GetDetailData(string code, bool? fullDelivered = null);

    }
}
