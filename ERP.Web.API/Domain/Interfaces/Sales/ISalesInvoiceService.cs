using ERP.Common;
using ERP.Common.Models;
using ERP.Entity.Sales;
using ERP.Web.API.Model.Sales;

namespace ERP.Web.API.Domain.Interfaces.Sales;

public interface ISalesInvoiceService : IGeneralService<SalesInvoiceHeader>
{
    DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
        string search);

    IEnumerable<SalesInvoiceDetail> GetDetailData(string code);

    List<dynamic> GetRelatedTransactions(string code);

    List<dynamic> GetDataMemo(string code);

    List<dynamic> GetDataSalesDownPayment(string code);

    List<dynamic> GetSalesInvoiceSDP(string soCode, string siCode);

    List<dynamic> GetDataDeliveryOrder(List<string> codes);

    SaveResult Insert(SalesInvoiceRequest data);

    SaveResult Update(SalesInvoiceRequest data);

    SaveResult Delete(string code, int userId);
}