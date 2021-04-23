using ERP_API.Model.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserCatalog = ERP_API.Domain.Entities.Catalog.User;

namespace ERP_API.Domain.Interfaces
{
    public interface IAuthService
    {
        AuthResult Login(UserCatalog data);
        AuthResult Logout();
    }
}
