using ERP.Entity.Sales;
using ERP_API.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_API.Domain.Interfaces.Sales
{
    public interface ISalesmanGroupService : IGeneralService<SalesmanGroup>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            List<int> category, string search);

        SaveResult Delete(int id, int userId);
    }
}
