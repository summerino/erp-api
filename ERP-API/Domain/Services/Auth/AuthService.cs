using ERP_API.Domain.Entities;
using ERP_API.Domain.Interfaces;
using ERP_API.Model.Auth;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using UserTenant = ERP_API.Domain.Entities.SystemManagement.User;
using UserCatalog = ERP_API.Domain.Entities.Catalog.User;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace ERP_API.Domain.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly CatalogContext _catalogcontext;
        private readonly TenantContext _tenantcontext;
        private readonly IClaimService _claimService;
        private readonly JwtConfig _jwtConfig;
        public AuthService(CatalogContext catalogContext, 
            TenantContext tenantContext, 
            IClaimService claimService,
            IOptionsMonitor<JwtConfig> optionsMonitor)
        {
            _catalogcontext = catalogContext;
            _tenantcontext = tenantContext;
            _claimService = claimService;
            _jwtConfig = optionsMonitor.CurrentValue;
        }
        public AuthResult Login(UserCatalog data)
        {
            var catalogUser = _catalogcontext.Users.Where(x => x.Username == data.Username).FirstOrDefault();
            if (catalogUser == null)
            {
                return new AuthResult()
                {
                    Message = "User not registered.",
                    Success = false
                };
            }

            var pwh = new PasswordHasher<UserCatalog>();
            var isCorrect = pwh.VerifyHashedPassword(catalogUser, catalogUser.Password, data.Password);

            if (isCorrect != PasswordVerificationResult.Success)
            {
                return new AuthResult()
                {
                    Message = "Password incorrect.",
                    Success = false
                };
            }
            else
            {
                var tenantUser = _tenantcontext.Users.Where(x => x.CatalogUserId == catalogUser.Id).FirstOrDefault();
                var accessIpAdd = _claimService.KeyToken;
                var jwtToken = GenerateJwtToken(tenantUser);

                tenantUser.IsLoggedIn = true;
                tenantUser.LastLogin = DateTime.Now;
                tenantUser.IpAddress = _claimService.IpAddress;
                tenantUser.TokenId = jwtToken;
                tenantUser.SessionId = _claimService.UserId.ToString();

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


                return new AuthResult()
                {
                    accessToken = jwtToken,
                    Success = true,
                    Message = "Login succeeded.",
                    userData = catalogUser.Id.ToString()

                };
            }
        }

        public AuthResult Logout(UserCatalog data)
        {
            var catalogUser = _catalogcontext.Users.Where(x => x.Username == data.Username).FirstOrDefault();
            if (catalogUser == null)
            {
                return new AuthResult()
                {
                    Message = "User not registered.",
                    Success = false
                };
            }

            var tenantUser = _tenantcontext.Users.Where(x => x.CatalogUserId == catalogUser.Id).FirstOrDefault();
            if (tenantUser == null)
            {
                return new AuthResult()
                {
                    Message = "User not registered.",
                    Success = false
                };
            }
            else
            {
                tenantUser.IsLoggedIn = false;
                tenantUser.IpAddress = null;
                tenantUser.TokenId = null;
                tenantUser.SessionId = null;

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

                return new AuthResult()
                {
                    Success = true,
                    Message = "Logout succeeded"

                };
            }
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
