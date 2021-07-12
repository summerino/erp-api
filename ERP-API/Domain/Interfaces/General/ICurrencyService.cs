using System.Collections.Generic;
using ERP.Entity.General;
using ERP.Web.API.Domain.Models;
using ERP.Web.API.Model.General;

namespace ERP.Web.API.Domain.Interfaces.General
{
    public interface ICurrencyService : IGeneralService<Currency>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search);

        DataSourceResult GetLists(IEnumerable<Filter> filters, IEnumerable<Sort> sorts);

        Currency FindByCode(string code);

        SaveResult Insert(CurrencyRequest data);

        SaveResult Update(CurrencyRequest data);

        SaveResult Delete(string code, int userId);
    }
}
