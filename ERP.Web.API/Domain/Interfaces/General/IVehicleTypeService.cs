using ERP.Common;
using ERP.Common.Models;
using ERP.Entity.General;

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
