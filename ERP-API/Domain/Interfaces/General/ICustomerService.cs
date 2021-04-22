using System.Collections.Generic;
using ERP_API.Domain.Entities.General;
using ERP_API.Domain.Models;

namespace ERP_API.Domain.Interfaces.General
{
    public interface ICustomerService : IGeneralService<Customer>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search);

        DataSourceResult GetLists(IEnumerable<Filter> filters, IEnumerable<Sort> sorts);

        Customer FindByCode(string code);

        SaveResult Delete(string code, int userId);
    }
}
