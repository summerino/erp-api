using ERP_API.Domain.Entities.SystemManagement;
using ERP_API.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERP_API.Model.SystemManagement;

namespace ERP_API.Domain.Interfaces.SystemManagement
{
    public interface IUserService
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
           string search);

        SaveResult Insert(UserRequest data);

        SaveResult Update(UserRequest data);

        SaveResult Delete(int id);


    }
}
