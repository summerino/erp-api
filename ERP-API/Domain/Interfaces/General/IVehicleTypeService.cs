using ERP_API.Domain.Entities.General;
using ERP_API.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_API.Domain.Interfaces.General
{
    public interface IVehicleTypeService : IGeneralService<VehicleType>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
           string search);

        DataSourceResult GetLists(IEnumerable<Filter> filters, IEnumerable<Sort> sorts);

        SaveResult Delete(int id, int userId);
    }
}
