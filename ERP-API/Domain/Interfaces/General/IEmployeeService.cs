using System.Collections.Generic;
using ERP_API.Domain.Entities.General;
using ERP_API.Domain.Models;
using ERP_API.Model.General;

namespace ERP_API.Domain.Interfaces.General
{
    public interface IEmployeeService : IGeneralService<Employee>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search);

        DataSourceResult GetLists(IEnumerable<Filter> filters, IEnumerable<Sort> sorts);

        IEnumerable<Employee> GetUnUsedList();

        SaveResult Insert(EmployeeRequest data);

        SaveResult Update(EmployeeRequest data);

        SaveResult Delete(long id, int userId);
    }
}
