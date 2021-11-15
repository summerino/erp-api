using ERP.Entity.Sales;
using ERP.Common;
using ERP.Common.Models;

namespace ERP.Web.API.Domain.Interfaces.Sales
{
    public interface ISalesmanGroupService : IGeneralService<SalesmanGroup>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            List<int> category, string search);

        SaveResult Delete(int id, int userId);
    }
}
