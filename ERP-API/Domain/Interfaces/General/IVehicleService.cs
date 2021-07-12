using ERP_API.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERP.Entity.General;

namespace ERP_API.Domain.Interfaces.General
{
    public interface IVehicleService : IGeneralService<Vehicle>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
           string search);

        SaveResult Delete(int id, int userId);
    }
}
