using System.Collections.Generic;
using ERP_API.Domain.Entities.Sales;
using ERP_API.Domain.Models;
using ERP_API.Model.Sales;
using Swift.Framework.Model;

namespace ERP_API.Domain.Interfaces.Sales
{
    public interface ISalesOrderService : IGeneralService<SalesOrderHeader>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search);

        IEnumerable<VwSalesOrderDetail> GetDetailData(string code, bool? fullReceived = null);

        List<dynamic> GetRelatedTransactions(string code);

        SaveResult Insert(SalesOrderRequest data);

        SaveResult Update(SalesOrderRequest data);

        SaveResult Delete(string code, int userId);
    }
}
