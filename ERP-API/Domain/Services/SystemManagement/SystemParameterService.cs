using System.Collections.Generic;
using System.Linq;
using ERP_API.Domain.Entities;
using ERP_API.Domain.Entities.SystemManagement;
using ERP_API.Domain.Interfaces.SystemManagement;

namespace ERP_API.Domain.Services.SystemManagement
{
    public class SystemParameterService : GeneralService<SystemParameter>, ISystemParameterService
    {
        public SystemParameterService(TenantContext db)
            : base(db)
        {
        }

        public IEnumerable<SystemParameter> GetList(string[] codes)
        {
            return Db.SystemParameters.Where(x => codes.Contains(x.Code));
        }
    }
}
