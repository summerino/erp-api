using System.Collections.Generic;
using ERP_API.Domain.Entities;
using ERP_API.Domain.Entities.SystemManagement;
using ERP_API.Domain.Extensions;
using ERP_API.Domain.Interfaces.SystemManagement;
using ERP_API.Domain.Models;

namespace ERP_API.Domain.Services.SystemManagement
{
    public class SystemParameterService : GeneralService<SystemParameter>, ISystemParameterService
    {
        public SystemParameterService(TenantContext db)
            : base(db)
        {
        }

        public DataSourceResult GetLists(IEnumerable<Filter> filters, IEnumerable<Sort> sorts)
        {
            return Db.SystemParameters.ToDataSourceResult(0, -1, filters, sorts);
        }
    }
}
