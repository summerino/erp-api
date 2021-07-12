using System.Collections.Generic;
using ERP.Entity.Sales;
using ERP.Web.API.Domain.Models;

namespace ERP.Web.API.Domain.Interfaces.Sales
{
    public interface IAreaService : IGeneralService<Area>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search);

        IEnumerable<Area> GetLists();

        object GetHierarchy();

        SaveResult Delete(int id, int userId);
    }
}
