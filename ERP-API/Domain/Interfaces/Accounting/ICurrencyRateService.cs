using System.Collections.Generic;
using ERP_API.Domain.Entities.Accounting;
using ERP_API.Domain.Models;
using ERP_API.Model.Accounting;

namespace ERP_API.Domain.Interfaces.Accounting
{
    public interface ICurrencyRateService : IGeneralService<CurrencyRate>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search);

        DataSourceResult GetLists(IEnumerable<Filter> filters, IEnumerable<Sort> sorts);
        
        SaveResult Insert(CurrencyRateRequest data);

    }
}
