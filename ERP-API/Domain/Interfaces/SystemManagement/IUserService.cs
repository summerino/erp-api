using ERP_API.Domain.Entities.SystemManagement;
using ERP_API.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserTenant = ERP_API.Domain.Entities.SystemManagement.VMUser;

namespace ERP_API.Domain.Interfaces.SystemManagement
{
    public interface IUserService
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
           string search);

        SaveResult Delete(int id);

        SaveResult Update(UserTenant dataTenant, string password);

        SaveResult Insert(UserTenant dataTenant, string password);
    }
}
