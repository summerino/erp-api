using System.Collections.Generic;
using ERP_API.Domain.Entities.Expedition;
using ERP_API.Domain.Models;
using ERP_API.Model.Expedition;

namespace ERP_API.Domain.Interfaces.Expedition
{
    public interface IExpeditionInvoiceService : IGeneralService<ExpeditionInvoiceHeader>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            string search);

        IEnumerable<ExpeditionInvoiceDetail> GetDetailData(string code);

        SaveResult Insert(ExpeditionInvoiceRequest data);

        SaveResult Update(ExpeditionInvoiceRequest data);

        SaveResult Delete(string code, int userId);
    }
}
