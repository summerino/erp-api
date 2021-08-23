using System.Collections.Generic;
using ERP.Common;
using ERP.Common.Models;
using ERP.Entity.Accounting;
using ERP.Web.API.Domain.Models;

namespace ERP.Web.API.Domain.Interfaces.Accounting
{
    public interface ICoaService : IGeneralService<Coa>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search);

        DataSourceResult GetLists(IEnumerable<Filter> filters, IEnumerable<Sort> sorts);

        DataSourceResult GetListsNonSysPar(IEnumerable<Filter> filters, IEnumerable<Sort> sorts);

        SaveResult Delete(int id, int userId);
    }
}
