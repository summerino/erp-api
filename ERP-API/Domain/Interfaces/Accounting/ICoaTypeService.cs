using ERP_API.Domain.Entities.Accounting;
using ERP_API.Domain.Models;
using System.Collections.Generic;

namespace ERP_API.Domain.Interfaces.Accounting
{
    public interface ICoaTypeService : IGeneralService<CoaType>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search);

        SaveResult Delete(int id, int userId);
    }
}
