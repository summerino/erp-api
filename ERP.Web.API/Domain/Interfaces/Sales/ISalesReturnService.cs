using System.Collections.Generic;
using ERP.Common;
using ERP.Common.Models;
using ERP.Entity.Sales;
using ERP.Web.API.Domain.Models;
using ERP.Web.API.Model.Sales;

namespace ERP.Web.API.Domain.Interfaces.Sales
{
    public interface ISalesReturnService : IGeneralService<SalesReturnHeader>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            string search);

        IEnumerable<VwSalesReturnDetail> GetDetailData(string code, bool? fullDelivered = null);

        IEnumerable<VwSalesReturnDetailExchDiffItem> GetDetailExchangeData(string code);

        List<dynamic> GetRelatedTransactions(string code);

        SaveResult Insert(SalesReturnRequest data);

        SaveResult Update(SalesReturnRequest data);

        SaveResult Delete(string code, int userId);
    }
}
