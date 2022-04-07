using System.ComponentModel.DataAnnotations;

namespace ERP.Web.API.Model.Auth;

public class MobileLoginRequest
{
    [Required]
    public string Username { get; set; }
    [Required]
    public string Password { get; set; }
    [Required]
    public int Type { get; set; }
}

public class MobileLoginCustomerRequest
{
    [Required]
    public string Username { get; set; }
    [Required]
    public string Password { get; set; }
}

public class MobileChangePasswordRequest
{
    [Required]
    public string OldPassword { get; set; }
    [Required]
    public string NewPassword { get; set; }
    [Required]
    public string ConfirmPassword { get; set; }
    [Required]
    public int Type { get; set; }
}