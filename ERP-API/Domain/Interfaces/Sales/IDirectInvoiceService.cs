using System.Collections;
using System.Collections.Generic;
using ERP_API.Domain.Entities.Sales;
using ERP_API.Domain.Models;
using ERP_API.Model.Sales;

namespace ERP_API.Domain.Interfaces.Sales
{
    public interface IDirectInvoiceService : IGeneralService<SalesInvoiceHeader>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            string search);

        IEnumerable GetDetailData(string code);

        SaveResult Insert(SalesInvoiceRequest data);

        SaveResult Update(SalesInvoiceRequest data);

        SaveResult Delete(string code, int userId);
    }
}
