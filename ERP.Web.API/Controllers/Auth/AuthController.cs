using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERP.Web.API.Domain.Interfaces.Auth;
using ERP.Web.API.Model.Auth;
using UserCatalog = ERP.Entity.Catalog.User;

namespace ERP.Web.API.Controllers.Auth
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;

        public AuthController(IAuthService auth)
        {
            _auth = auth;
        }

        [HttpPost("[action]")]
        [AllowAnonymous]
        public IActionResult Login(UserCatalog data)
        {
            if (ModelState.IsValid)
            {
                var result = _auth.Login(data);
                return Ok(result) ;
            }

            return BadRequest(new AuthResult
            {
                Message = "Invalid request.",
                Success = false
            });
        }

        [HttpPost("[action]")]
        public IActionResult Logout()
        {
            if (ModelState.IsValid)
            {
                var result = _auth.Logout();
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
