using System.Collections.Generic;
using ERP.Entity.Sales;
using ERP_API.Domain.Models;
using ERP_API.Model.Sales;

namespace ERP_API.Domain.Interfaces.Sales
{
    public interface ISalesDeliveryService : IGeneralService<SalesDeliveryHeader>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            string search);

        IEnumerable<VwSalesDeliveryDetail> GetDetailData(string code);

        List<dynamic> GetRelatedTransactions(string code);

        IEnumerable<SalesDeliveryHeader> GetUnInvoiceData(string soCode, string invCode);

        SaveResult Insert(SalesDeliveryRequest data);

        SaveResult Update(SalesDeliveryRequest data);

        SaveResult Delete(string code, int userId);
    }
}
