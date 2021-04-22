using System.Collections.Generic;
using ERP_API.Domain.Entities.Accounting;
using ERP_API.Domain.Models;

namespace ERP_API.Domain.Interfaces.Accounting
{
    public interface ICoaService : IGeneralService<Coa>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search);

        DataSourceResult GetLists(IEnumerable<Filter> filters, IEnumerable<Sort> sorts);
        
        SaveResult Delete(int id, int userId);
    }
}
