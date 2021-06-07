using ERP_API.Model.Auth;
using UserCatalog = ERP_API.Domain.Entities.Catalog.User;

namespace ERP_API.Domain.Interfaces.Auth
{
    public interface IAuthService
    {
        AuthResult Login(UserCatalog data);
        AuthResult Logout();
    }
}
