using ERP.Web.API.Model;
using ERP.Web.API.Model.Auth;

namespace ERP.Web.API.Domain.Interfaces.Auth
{
    public interface IMobileAuthService
    {
        MobileAuthResult Login(MobileLoginRequest data);

        MobileAuthResult LoginWarehouse(MobileLoginRequest data);

        MobileAuthResult LoginCustomer(MobileLoginCustomerRequest data);

        MobileSimpleResponse ChangePassword(MobileChangePasswordRequest data);

        MobileSimpleResponse ChangePasswordCustomer(MobileChangePasswordRequest data);

        MobileAuthResult Logout();

        MobileAuthResult LogoutCustomer();
    }
}