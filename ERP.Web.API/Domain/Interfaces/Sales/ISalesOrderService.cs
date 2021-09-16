using System.Collections.Generic;
using ERP.Common;
using ERP.Common.Models;
using ERP.Entity.Sales;
using ERP.Web.API.Model.Sales;

namespace ERP.Web.API.Domain.Interfaces.Sales
{
    public interface ISalesOrderService : IGeneralService<SalesOrderHeader>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search);

        IEnumerable<VwSalesOrderDetail> GetDetailData(string code, bool? fullReceived = null);

        List<dynamic> GetRelatedTransactions(string code);

        IEnumerable<DetailFreeGoodData> GetFreeDetailData(string code, bool? fullDlv);

        IEnumerable<SalesOrderDetailDiscount> GetDiscDetailData(string code);

        SaveResult Insert(SalesOrderRequest data);

        SaveResult Update(SalesOrderRequest data);

        SaveResult Delete(string code, int userId);

        SaveResult Close(string code, int userId);
    }
}
