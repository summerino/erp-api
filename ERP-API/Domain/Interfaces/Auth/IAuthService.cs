using ERP_API.Model;
using ERP_API.Model.Auth;
using System.Collections.Generic;
using UserCatalog = ERP_API.Domain.Entities.Catalog.User;

namespace ERP_API.Domain.Interfaces.Auth
{
    public interface IAuthService
    {
        AuthResult Login(UserCatalog data);
        AuthResult Logout();
        IEnumerable<int> GetActions(int menuId, int roleId, Actions[] actions);
        IEnumerable<int> GetActions(int menuId, int roleId, List<int> actions);
    }
}