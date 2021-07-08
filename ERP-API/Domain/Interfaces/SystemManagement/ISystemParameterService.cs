using System.Collections.Generic;
using ERP_API.Domain.Entities.SystemManagement;
using ERP_API.Domain.Models;
using ERP_API.Model.SystemManagement;

namespace ERP_API.Domain.Interfaces.SystemManagement
{
    public interface ISystemParameterService : IGeneralService<SystemParameter>
    {
        DataSourceResult GetLists(IEnumerable<Filter> filters, IEnumerable<Sort> sorts, IEnumerable<string> codes);
        List<SystemParameterRequest> GetHierarchy();
        SaveResult Save(List<SystemParameterRequest> data);
    }
}
