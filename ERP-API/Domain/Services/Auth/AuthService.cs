using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ERP_API.Domain.Entities;
using ERP_API.Domain.Interfaces;
using ERP_API.Model.Auth;
using UserTenant = ERP_API.Domain.Entities.SystemManagement.User;
using UserCatalog = ERP_API.Domain.Entities.Catalog.User;

namespace ERP_API.Domain.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly CatalogContext _catalogCtx;
        private readonly TenantContext _tenantCtx;
        private readonly IClaimService _claim;
        private readonly JwtConfig _jwtConfig;

        public AuthService(CatalogContext catalogCtx, 
            TenantContext tenantCtx, 
            IClaimService claim,
            IOptionsMonitor<JwtConfig> optionsMonitor)
        {
            _catalogCtx = catalogCtx;
            _tenantCtx = tenantCtx;
            _claim = claim;
            _jwtConfig = optionsMonitor.CurrentValue;
        }

        public AuthResult Login(UserCatalog data)
        {
            var catalogUser = _catalogCtx.Users.FirstOrDefault(x => x.Username == data.Username);
            if (catalogUser == null)
            {
                return new AuthResult
                {
                    Message = "Username & Password incorrect.",
                    Success = false
                };
            }

            var pwh = new PasswordHasher<UserCatalog>();
            var isCorrect = pwh.VerifyHashedPassword(catalogUser, catalogUser.Password, data.Password);

            if (isCorrect != PasswordVerificationResult.Success)
            {
                return new AuthResult
                {
                    Message = "Username & Password incorrect.",
                    Success = false
                };
            }

            var tenantUser = _tenantCtx.Users.FirstOrDefault(x => x.CatalogUserId == catalogUser.Id);
            if (tenantUser.IsLoggedIn)
            {
                return new AuthResult
                {
                    Message = "User already logged in.",
                    Success = false
                };
            }

            var accessIpAdd = _claim.KeyToken;
            var jwtToken = GenerateJwtToken(tenantUser);

            tenantUser.IsLoggedIn = true;
            tenantUser.LastLogin = DateTime.Now;
            tenantUser.IpAddress = _claim.IpAddress;
            tenantUser.TokenId = jwtToken;
            tenantUser.SessionId = _claim.UserId.ToString();

            _tenantCtx.Users.Update(tenantUser);
            _tenantCtx.Entry(tenantUser).Property(e => e.CatalogUserId).IsModified = false;
            _tenantCtx.Entry(tenantUser).Property(e => e.Username).IsModified = false;
            _tenantCtx.Entry(tenantUser).Property(e => e.Initial).IsModified = false;
            _tenantCtx.Entry(tenantUser).Property(e => e.Name).IsModified = false;
            _tenantCtx.Entry(tenantUser).Property(e => e.RoleId).IsModified = false;
            _tenantCtx.Entry(tenantUser).Property(e => e.EmployeeId).IsModified = false;
            _tenantCtx.Entry(tenantUser).Property(e => e.IsActive).IsModified = false;
            _tenantCtx.Entry(tenantUser).Property(e => e.CreatedBy).IsModified = false;
            _tenantCtx.Entry(tenantUser).Property(e => e.CreatedDate).IsModified = false;
            _tenantCtx.Entry(tenantUser).Property(e => e.UpdatedBy).IsModified = false;
            _tenantCtx.Entry(tenantUser).Property(e => e.UpdatedDate).IsModified = false;
            _tenantCtx.SaveChanges();

            return new AuthResult
            {
                accessToken = jwtToken,
                Success = true,
                Message = "Login succeeded.",
                userData = catalogUser.Id.ToString()

            };
        }

        public AuthResult Logout()
        {
            var catalogUser = _catalogCtx.Users.FirstOrDefault(x => x.Id.ToString() == _claim.CatalogUserId);
            if (catalogUser == null)
            {
                return new AuthResult
                {
                    Message = "User not registered.",
                    Success = false
                };
            }

            var tenantUser = _tenantCtx.Users.FirstOrDefault(x => x.CatalogUserId == catalogUser.Id);
            if (tenantUser == null)
            {
                return new AuthResult
                {
                    Message = "User not registered.",
                    Success = false
                };
            }

            tenantUser.IsLoggedIn = false;
            tenantUser.IpAddress = null;
            tenantUser.TokenId = null;
            tenantUser.SessionId = null;

            _tenantCtx.Users.Update(tenantUser);
            _tenantCtx.Entry(tenantUser).Property(e => e.CatalogUserId).IsModified = false;
            _tenantCtx.Entry(tenantUser).Property(e => e.Username).IsModified = false;
            _tenantCtx.Entry(tenantUser).Property(e => e.Initial).IsModified = false;
            _tenantCtx.Entry(tenantUser).Property(e => e.Name).IsModified = false;
            _tenantCtx.Entry(tenantUser).Property(e => e.RoleId).IsModified = false;
            _tenantCtx.Entry(tenantUser).Property(e => e.EmployeeId).IsModified = false;
            _tenantCtx.Entry(tenantUser).Property(e => e.IsActive).IsModified = false;
            _tenantCtx.Entry(tenantUser).Property(e => e.CreatedBy).IsModified = false;
            _tenantCtx.Entry(tenantUser).Property(e => e.CreatedDate).IsModified = false;
            _tenantCtx.Entry(tenantUser).Property(e => e.UpdatedBy).IsModified = false;
            _tenantCtx.Entry(tenantUser).Property(e => e.UpdatedDate).IsModified = false;
            _tenantCtx.SaveChanges();

            return new AuthResult
            {
                Success = true,
                Message = "Logout succeeded"

            };
        }

        private string GenerateJwtToken(UserTenant data)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.ASCII.GetBytes(_jwtConfig.Secret);
            var issuer = _jwtConfig.Issuer;
            var time = _jwtConfig.TimeInMinute;

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("UserId", data.Id.ToString()),
                    new Claim("RoleId", data.RoleId.ToString()),
                    new Claim("CatalogUserId", data.CatalogUserId.ToString()),
                    new Claim(JwtRegisteredClaimNames.Sub, data.Username),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.GivenName, data.Initial)
                }),
                Expires = DateTime.Now.AddMinutes(time),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = issuer,
                NotBefore = DateTime.Now
            };

            var token = jwtTokenHandler.CreateJwtSecurityToken(tokenDescriptor);
            var jwtToken = jwtTokenHandler.WriteToken(token);

            return jwtToken;
        }
    }
}
