using ERP_API.Domain.Entities.SystemManagement;
using System.Collections.Generic;

namespace ERP_API.Domain.Interfaces.SystemManagement
{
    public interface ISystemParameterService : IGeneralService<SystemParameter>
    {
        IEnumerable<SystemParameter> GetList(string[] codes);
    }
}
