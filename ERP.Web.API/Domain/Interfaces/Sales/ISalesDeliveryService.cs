using System.Collections.Generic;
using ERP.Common;
using ERP.Common.Models;
using ERP.Entity.Sales;
using ERP.Web.API.Model.Sales;

namespace ERP.Web.API.Domain.Interfaces.Sales
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
