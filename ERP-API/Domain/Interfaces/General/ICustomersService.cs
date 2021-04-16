using System.Collections.Generic;
using ERP_API.Domain.Entities.General;
using ERP_API.Domain.Models;
using ERP_API.Model;
using Swift.Framework.Model;

namespace ERP_API.Domain.Interfaces.General
{
    public interface ICustomersService : IGeneralService<Customer>
    {
        DataSourceResult GetViewData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search);

        SaveResult Delete(string code, int userId);
    }
}
