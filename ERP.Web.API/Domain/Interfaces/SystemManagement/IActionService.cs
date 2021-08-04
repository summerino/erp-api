using System.Collections.Generic;
using ERP.Entity.SystemManagement;

namespace ERP.Web.API.Domain.Interfaces.SystemManagement
{
    public interface IActionService
    {
        IEnumerable<Action> GetData();
    }
}
