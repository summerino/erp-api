using ERP.Common;
using ERP.Common.Models;
using ERP.Entity.General;

namespace ERP.Web.API.Domain.Interfaces.General
{
    public interface ISupplierService : IGeneralService<Supplier>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search);

        DataSourceResult GetLists(IEnumerable<Filter> filters, IEnumerable<Sort> sorts);

        Supplier FindByCode(string code);

        SaveResult Delete(string code, int userId);
    }
}
