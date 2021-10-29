using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERP.Web.API.Domain.Interfaces.Auth;
using ERP.Web.API.Model;
using ERP.Web.API.Model.Auth;

namespace ERP.Web.API.Controllers.Auth
{
    [Authorize(AppConstant.ValidateMobileCustomerTokenPolicy)]
    [Route("mobile-customer-auth")]
    [ApiController]
    public class MobileCustomerAuthController : ControllerBase
    {
        private readonly IMobileAuthService _auth;

        public MobileCustomerAuthController(IMobileAuthService auth)
        {
            _auth = auth;
        }

        [HttpPost("[action]")]
        [AllowAnonymous]
        public IActionResult Login(MobileLoginCustomerRequest data)
        {
            if (ModelState.IsValid)
            {
                var result = _auth.LoginCustomer(data);
                return Ok(result) ;
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
                var result = _auth.ChangePasswordCustomer(data);
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
                var result = _auth.LogoutCustomer();
                return Ok(result);
            }

            return BadRequest(new MobileAuthResult
            {
                Message = "Invalid request.",
                Success = false
            });
        }
    }
}
