using ERP.Common;
using ERP.Common.Models;
using ERP.Entity.General;
using ERP.Web.API.Model.General;

namespace ERP.Web.API.Domain.Interfaces.General;

public interface ICustomerService : IGeneralService<Customer>
{
    DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
        string search, string mobileLastSync = null);

    DataSourceResult GetLists(IEnumerable<Filter> filters, IEnumerable<Sort> sorts);

    IEnumerable<CustomerAddress> GetAddress(string code);

    VwCustomer FindByCode(string code);

    SaveResult Insert(CustomerRequest data);

    SaveResult Update(CustomerRequest data);

    SaveResult Delete(string code, int userId);
}