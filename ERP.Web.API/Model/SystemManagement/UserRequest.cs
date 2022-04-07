using ERP.Entity.SystemManagement;

namespace ERP.Web.API.Model.SystemManagement;

public class UserRequest : User
{
    public string Password { get; set; }

    public string NewPassword { get; set; }

    public string ConfirmPassword { get; set; }
}