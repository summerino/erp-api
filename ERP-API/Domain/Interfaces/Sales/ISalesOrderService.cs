using System.Collections.Generic;
using ERP.Entity.Sales;
using ERP_API.Domain.Models;
using ERP_API.Model.Sales;

namespace ERP_API.Domain.Interfaces.Sales
{
    public interface ISalesOrderService : IGeneralService<SalesOrderHeader>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search);

        IEnumerable<VwSalesOrderDetail> GetDetailData(string code, bool? fullReceived = null);

        List<dynamic> GetRelatedTransactions(string code);

        IEnumerable<DetailFreeGoodData> GetFreeDetailData(string code);

        IEnumerable<SalesOrderDetailDiscount> GetDiscDetailData(string code);

        SaveResult Insert(SalesOrderRequest data);

        SaveResult Update(SalesOrderRequest data);

        SaveResult Delete(string code, int userId);

        SaveResult Close(string code, int userId);
    }
}
