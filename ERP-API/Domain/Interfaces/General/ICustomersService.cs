using System.Collections.Generic;
using ERP_API.Domain.Entities.General;
using ERP_API.Domain.Models;
using Swift.Framework.Model;

namespace ERP_API.Domain.Interfaces.General
{
    public interface ICustomersService : IGeneralService<Customer>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search);

        DataSourceResult GetLists(IEnumerable<Filter> filters, IEnumerable<Sort> sorts);

        SaveResult Delete(string code, int userId);
    }
}
