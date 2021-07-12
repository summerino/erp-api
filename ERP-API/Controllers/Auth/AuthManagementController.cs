using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERP_API.Domain.Interfaces.Auth;
using ERP_API.Model.Auth;
using UserCatalog = ERP.Entity.Catalog.User;

namespace ERP_API.Controllers.Auth
{
    [Route("auth")]
    [ApiController]
    public class AuthManagementController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthManagementController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost]
        [Route("Login")]
        [AllowAnonymous]
        public IActionResult Login(UserCatalog data)
        {
            if (ModelState.IsValid)
            {
                var result = _authService.Login(data);
                return Ok(result) ;
            }

            return BadRequest(new AuthResult
            {
                Message = "Invalid request.",
                Success = false
            });
        }

        [HttpPost]
        [Route("Logout")]
        public IActionResult Logout()
        {
            if (ModelState.IsValid)
            {
                var result = _authService.Logout();
                return Ok(result);
            }

            return BadRequest(new AuthResult
            {
                Message = "Invalid request.",
                Success = false
            });
        }
    }
}
