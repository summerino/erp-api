using ERP.Common;
using ERP.Common.Models;
using ERP.Entity.General;

namespace ERP.Web.API.Domain.Interfaces.General
{
    public interface IVehicleService : IGeneralService<Vehicle>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
           string search);

        SaveResult Delete(int id, int userId);
    }
}
