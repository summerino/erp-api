using System.Collections.Generic;
using ERP_API.Domain.Entities.SystemManagement;
using ERP_API.Domain.Models;

namespace ERP_API.Domain.Interfaces.SystemManagement
{
    public interface ISystemParameterService : IGeneralService<SystemParameter>
    {
        DataSourceResult GetLists(IEnumerable<Filter> filters, IEnumerable<Sort> sorts);
    }
}
