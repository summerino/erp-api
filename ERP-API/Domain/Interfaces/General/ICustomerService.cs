using System.Collections.Generic;
using ERP_API.Domain.Entities.General;
using ERP_API.Domain.Models;
using ERP_API.Model.General;

namespace ERP_API.Domain.Interfaces.General
{
    public interface ICustomerService : IGeneralService<Customer>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search);

        DataSourceResult GetLists(IEnumerable<Filter> filters, IEnumerable<Sort> sorts);

        IEnumerable<CustomerAddress> GetAddress(string code);

        VwCustomer FindByCode(string code);

        SaveResult Insert(CustomerRequest data);

        SaveResult Update(CustomerRequest data);

        SaveResult Delete(string code, int userId);
    }
}
