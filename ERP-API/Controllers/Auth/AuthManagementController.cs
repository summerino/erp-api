using ERP_API.Model.Auth;
using Microsoft.AspNetCore.Mvc;
using UserCatalog = ERP_API.Domain.Entities.Catalog.User;
using Microsoft.AspNetCore.Authorization;
using ERP_API.Domain.Interfaces;

namespace ERP_API.Controllers.Auth
{
    [Route("api/v1/auth")] // api/v1/auth
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

            return BadRequest(new AuthResult()
            {
                Message = "Invalid request.",
                Success = false
            });
        }

        [HttpPost]
        [Route("Logout")]
        public IActionResult Logout(UserCatalog data)
        {
            if (ModelState.IsValid)
            {
                var result = _authService.Logout(data);
                return Ok(result);
            }

            return BadRequest(new AuthResult()
            {
                Message = "Invalid request.",
                Success = false
            });
        }
    }
}
