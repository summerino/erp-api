using ERP_API.Domain.Entities;
using ERP_API.Domain.Entities.Catalog;
using ERP_API.Model.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using UserTenant = ERP_API.Domain.Entities.SystemManagement.User;
using UserCatalog = ERP_API.Domain.Entities.Catalog.User;
using ERP_API.Domain.Services;
using Microsoft.AspNetCore.Authorization;

namespace ERP_API.Controllers.Auth
{
    [Route("api/v1/auth")] // api/v1/auth
    [Authorize(Policy = "Session")]
    [ApiController]
    public class AuthManagementController : ControllerBase
    {
        private readonly CatalogContext _catalogcontext;
        private readonly TenantContext _tenantcontext;
        private readonly JwtConfig _jwtConfig;
        private readonly IClaimService _claimService;

        public AuthManagementController(
            CatalogContext catalogcontext,
            TenantContext tenantcontext,
            IClaimService claimService,
            IOptionsMonitor<JwtConfig> optionsMonitor)
        {
            _catalogcontext = catalogcontext;
            _jwtConfig = optionsMonitor.CurrentValue;
            _tenantcontext = tenantcontext;
            _claimService = claimService;
        }

        [HttpPost]
        [Route("Register")]
        [AllowAnonymous]
        public IActionResult Register(UserCatalog data)
        {
            if (ModelState.IsValid)
            {
                var existingUser = _catalogcontext.Users.Where(x => x.Username == data.Username).FirstOrDefault();
                if (existingUser != null)
                {
                    return BadRequest(new AuthResult()
                    {
                        Message = "Username already registered.",
                        Success = false
                    });
                }

                var pwh = new PasswordHasher<UserCatalog>();
                var hashPwd = pwh.HashPassword(data, data.Password);

                var newCatalogUser = new UserCatalog()
                {
                    Id = new Guid(),
                    TenantId = 1,
                    Username = data.Username,
                    Password = hashPwd
                };

                _catalogcontext.Add(newCatalogUser);
                _catalogcontext.SaveChanges();

                var newTenantUser = new UserTenant()
                {
                    CatalogUserId = newCatalogUser.Id,
                    Username = newCatalogUser.Username,
                    Initial = newCatalogUser.Username.Substring(0, 3).ToUpper(),
                    Name = newCatalogUser.Username,
                    IsActive = true,
                    CreatedBy = 1,
                    CreatedDate = DateTime.Now,
                    UpdatedBy = 1,
                    UpdatedDate = DateTime.Now

                };

                _tenantcontext.Add(newTenantUser);
                _tenantcontext.SaveChanges();

                return Ok(new AuthResult()
                {
                    Success = true,
                    Message = "User registration succeeded."
                });
            }


            return BadRequest(new AuthResult()
            {
                Message = "Invalid request.",
                Success = false
            });
        }

        [HttpPost]
        [Route("Login")]
        [AllowAnonymous]
        public IActionResult Login(UserCatalog data)
        {
            if (ModelState.IsValid)
            {
                var catalogUser = _catalogcontext.Users.Where(x => x.Username == data.Username).FirstOrDefault();
                if (catalogUser == null)
                {
                    return BadRequest(new AuthResult()
                    {
                        Message = "User not registered.",
                        Success = false
                    });
                }

                var pwh = new PasswordHasher<UserCatalog>();
                var isCorrect = pwh.VerifyHashedPassword(catalogUser, catalogUser.Password, data.Password);

                if (isCorrect != PasswordVerificationResult.Success)
                {
                    return BadRequest(new AuthResult()
                    {
                        Message = "Password incorrect.",
                        Success = false
                    });
                }
                else
                {
                    var tenantUser = _tenantcontext.Users.Where(x => x.CatalogUserId == catalogUser.Id).FirstOrDefault();
                    var accessIpAdd = _claimService.KeyToken;
                    var jwtToken = GenerateJwtToken(catalogUser);

                    tenantUser.IsLoggedIn = true;
                    tenantUser.LastLogin = DateTime.Now;
                    tenantUser.IpAddress = _claimService.IpAddress;
                    tenantUser.TokenId = jwtToken;

                    _tenantcontext.Users.Update(tenantUser);
                    _tenantcontext.Entry(tenantUser).Property(e => e.CatalogUserId).IsModified = false;
                    _tenantcontext.Entry(tenantUser).Property(e => e.Username).IsModified = false;
                    _tenantcontext.Entry(tenantUser).Property(e => e.Initial).IsModified = false;
                    _tenantcontext.Entry(tenantUser).Property(e => e.Name).IsModified = false;
                    _tenantcontext.Entry(tenantUser).Property(e => e.RoleId).IsModified = false;
                    _tenantcontext.Entry(tenantUser).Property(e => e.EmployeeId).IsModified = false;
                    _tenantcontext.Entry(tenantUser).Property(e => e.IsActive).IsModified = false;
                    _tenantcontext.Entry(tenantUser).Property(e => e.CreatedBy).IsModified = false;
                    _tenantcontext.Entry(tenantUser).Property(e => e.CreatedDate).IsModified = false;
                    _tenantcontext.Entry(tenantUser).Property(e => e.UpdatedBy).IsModified = false;
                    _tenantcontext.Entry(tenantUser).Property(e => e.UpdatedDate).IsModified = false;
                    _tenantcontext.SaveChanges();


                    return Ok(new AuthResult()
                    {
                        accessToken = jwtToken,
                        Success = true,
                        Message = "Login succeeded.",
                        userData = catalogUser.Id.ToString()

                    });
                }
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
                var catalogUser = _catalogcontext.Users.Where(x => x.Username == data.Username).FirstOrDefault();
                if (catalogUser == null)
                {
                    return BadRequest(new AuthResult()
                    {
                        Message = "User not registered.",
                        Success = false
                    });
                }

                var tenantUser = _tenantcontext.Users.Where(x => x.CatalogUserId == catalogUser.Id).FirstOrDefault();
                if (tenantUser == null)
                {
                    return BadRequest(new AuthResult()
                    {
                        Message = "User not registered.",
                        Success = false
                    });
                }
                else
                {
                    tenantUser.IsLoggedIn = false;
                    tenantUser.IpAddress = null;
                    tenantUser.TokenId = null;

                    _tenantcontext.Users.Update(tenantUser);
                    _tenantcontext.Entry(tenantUser).Property(e => e.CatalogUserId).IsModified = false;
                    _tenantcontext.Entry(tenantUser).Property(e => e.Username).IsModified = false;
                    _tenantcontext.Entry(tenantUser).Property(e => e.Initial).IsModified = false;
                    _tenantcontext.Entry(tenantUser).Property(e => e.Name).IsModified = false;
                    _tenantcontext.Entry(tenantUser).Property(e => e.RoleId).IsModified = false;
                    _tenantcontext.Entry(tenantUser).Property(e => e.EmployeeId).IsModified = false;
                    _tenantcontext.Entry(tenantUser).Property(e => e.IsActive).IsModified = false;
                    _tenantcontext.Entry(tenantUser).Property(e => e.CreatedBy).IsModified = false;
                    _tenantcontext.Entry(tenantUser).Property(e => e.CreatedDate).IsModified = false;
                    _tenantcontext.Entry(tenantUser).Property(e => e.UpdatedBy).IsModified = false;
                    _tenantcontext.Entry(tenantUser).Property(e => e.UpdatedDate).IsModified = false;
                    _tenantcontext.SaveChanges();

                    return Ok(new AuthResult()
                    {
                        Success = true,
                        Message = "Logout succeeded"

                    });

                }
            }

            return BadRequest(new AuthResult()
            {
                Message = "Invalid request.",
                Success = false
            });
        }

        private string GenerateJwtToken(UserCatalog data)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.ASCII.GetBytes(_jwtConfig.Secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("UserId", data.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, data.Username),
                    new Claim(JwtRegisteredClaimNames.Sub, data.Username),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                }),
                Expires = DateTime.Now.AddHours(4),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = jwtTokenHandler.WriteToken(token);

            return jwtToken;
        }
    }
}
