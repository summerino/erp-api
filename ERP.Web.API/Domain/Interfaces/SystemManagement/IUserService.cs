using ERP.Common;
using ERP.Common.Models;
using ERP.Web.API.Model.SystemManagement;

namespace ERP.Web.API.Domain.Interfaces.SystemManagement;

public interface IUserService
{
    DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
        string search);

    SaveResult Insert(UserRequest data);

    SaveResult Update(UserRequest data);

    SaveResult Delete(int id);

    SaveResult ChangePassword(UserRequest data);
}