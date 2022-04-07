using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERP.Web.API.Domain.Interfaces.Auth;
using ERP.Web.API.Model;
using ERP.Web.API.Model.Auth;

namespace ERP.Web.API.Controllers.Auth;

[Authorize(AppConstant.ValidateMobileTokenPolicy)]
[Route("mobile-auth")]
[ApiController]
public class MobileAuthController : ControllerBase
{
    private readonly IMobileAuthService _auth;

    public MobileAuthController(IMobileAuthService auth)
    {
        _auth = auth;
    }

    [HttpPost("[action]")]
    [AllowAnonymous]
    public IActionResult Login(MobileLoginRequest data)
    {
        if (ModelState.IsValid)
        {
            var result = _auth.Login(data);
            return Ok(result) ;
        }

        return BadRequest(new MobileAuthResult
        {
            Message = "Invalid request.",
            Success = false
        });
    }

    [HttpPost("login-warehouse")]
    [AllowAnonymous]
    public IActionResult LoginWarehouse(MobileLoginRequest data)
    {
        if (ModelState.IsValid)
        {
            var result = _auth.LoginWarehouse(data);
            return Ok(result);
        }

        return BadRequest(new MobileAuthResult
        {
            Message = "Invalid request.",
            Success = false
        });
    }
        
    [HttpPost("change-password")]
    public IActionResult ChangePassword(MobileChangePasswordRequest data)
    {
        if (ModelState.IsValid)
        {
            var result = _auth.ChangePassword(data);
            return Ok(result);
        }

        return BadRequest(new MobileAuthResult
        {
            Message = "Invalid request.",
            Success = false
        });
    }

    [HttpPost("Logout")]
    public IActionResult Logout()
    {
        if (ModelState.IsValid)
        {
            var result = _auth.Logout();
            return Ok(result);
        }

        return BadRequest(new MobileAuthResult
        {
            Message = "Invalid request.",
            Success = false
        });
    }
}