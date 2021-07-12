using System.Collections.Generic;
using ERP.Entity.General;
using ERP.Web.API.Domain.Models;

namespace ERP.Web.API.Domain.Interfaces.General
{
    public interface IVehicleTypeService : IGeneralService<VehicleType>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
           string search);

        DataSourceResult GetLists(IEnumerable<Filter> filters, IEnumerable<Sort> sorts);

        SaveResult Delete(int id, int userId);
    }
}
