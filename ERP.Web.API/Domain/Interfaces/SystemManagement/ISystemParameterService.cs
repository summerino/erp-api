using System.Collections.Generic;
using ERP.Entity.SystemManagement;
using ERP.Web.API.Domain.Models;
using ERP.Web.API.Model.SystemManagement;

namespace ERP.Web.API.Domain.Interfaces.SystemManagement
{
    public interface ISystemParameterService : IGeneralService<SystemParameter>
    {
        DataSourceResult GetLists(IEnumerable<Filter> filters, IEnumerable<Sort> sorts, IEnumerable<string> codes);
        List<SystemParameterRequest> GetHierarchy();
        SaveResult Save(List<SystemParameterRequest> data);
    }
}
