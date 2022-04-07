using ERP.Common;
using ERP.Common.Models;
using ERP.Entity.Accounting;
using ERP.Web.API.Model.Accounting;

namespace ERP.Web.API.Domain.Interfaces.Accounting;

public interface ICurrencyRateService : IGeneralService<CurrencyRate>
{
    DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
        string search);

    DataSourceResult GetLists(IEnumerable<Filter> filters, IEnumerable<Sort> sorts);
        
    SaveResult Insert(CurrencyRateRequest data);

}