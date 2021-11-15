using ERP.Common;
using ERP.Common.Models;
using ERP.Entity.Accounting;

namespace ERP.Web.API.Domain.Interfaces.Accounting
{
    public interface ICoaService : IGeneralService<Coa>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search);

        DataSourceResult GetLists(IEnumerable<Filter> filters, IEnumerable<Sort> sorts, string mobileLastSync = null);

        DataSourceResult GetListsNonSysPar(IEnumerable<Filter> filters, IEnumerable<Sort> sorts);

        DataSourceResult GetListParents(IEnumerable<Filter> filters, IEnumerable<Sort> sorts);

        SaveResult Delete(int id, int userId);
    }
}
