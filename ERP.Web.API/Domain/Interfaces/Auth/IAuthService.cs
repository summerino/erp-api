using ERP.Web.API.Model;
using ERP.Web.API.Model.Auth;
using UserCatalog = ERP.Entity.Catalog.User;

namespace ERP.Web.API.Domain.Interfaces.Auth
{
    public interface IAuthService
    {
        AuthResult Login(UserCatalog data);

        AuthResult Logout();

        IEnumerable<int> GetActions(int menuId, int roleId, Actions[] actions);

        IEnumerable<int> GetActions(int menuId, int roleId, List<int> actions);
    }
}