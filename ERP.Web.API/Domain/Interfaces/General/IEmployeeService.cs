using ERP.Common;
using ERP.Common.Models;
using ERP.Entity.General;
using ERP.Web.API.Model.General;

namespace ERP.Web.API.Domain.Interfaces.General
{
    public interface IEmployeeService : IGeneralService<Employee>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search);

        DataSourceResult GetLists(IEnumerable<Filter> filters, IEnumerable<Sort> sorts);

        IEnumerable<Employee> GetUnUsedList(long? empId);

        SaveResult Insert(EmployeeRequest data);

        SaveResult Update(EmployeeRequest data);

        SaveResult Delete(long id, int userId);
    }
}
