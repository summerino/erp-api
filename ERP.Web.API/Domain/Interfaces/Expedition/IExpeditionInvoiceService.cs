using ERP.Common;
using ERP.Common.Models;
using ERP.Entity.Expedition;
using ERP.Web.API.Model.Expedition;

namespace ERP.Web.API.Domain.Interfaces.Expedition;

public interface IExpeditionInvoiceService : IGeneralService<ExpeditionInvoiceHeader>
{
    DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
        string search);

    IEnumerable<dynamic> GetDetailData(string code);
        
    List<dynamic> GetRelatedTransactions(string code);

    DataSourceResult GetReceivesData(IEnumerable<Filter> filter);

    DataSourceResult GetDeliveriesData(IEnumerable<Filter> filter);

    SaveResult Insert(ExpeditionInvoiceRequest data);

    SaveResult Update(ExpeditionInvoiceRequest data);

    SaveResult Delete(string code, int userId);
}