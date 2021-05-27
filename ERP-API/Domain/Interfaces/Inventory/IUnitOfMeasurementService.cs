using System.Collections.Generic;
using ERP_API.Domain.Entities.Inventory;
using ERP_API.Domain.Models;
using ERP_API.Model.Inventory;

namespace ERP_API.Domain.Interfaces.Inventory
{
    public interface IUnitOfMeasurementService : IGeneralService<UoM>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,string search);

        DataSourceResult GetLists(IEnumerable<Filter> filters, IEnumerable<Sort> sorts);

        SaveResult Delete(int id, int userId);

        SaveResult Insert(UnitOfMeasurementRequest data, int userId);

        SaveResult Update(UnitOfMeasurementRequest data, int userId);

        IEnumerable<UoMConversion> GetDataConversion(int? uomId = null);
    }
}
