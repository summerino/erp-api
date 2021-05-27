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
using Microsoft.EntityFrameworkCore;
using UserTenant = ERP_API.Domain.Entities.SystemManagement.User;
using UserCatalog = ERP_API.Domain.Entities.Catalog.User;

namespace ERP_API.Domain.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly CatalogContext _catalogCtx;
        private readonly IClaimService _claim;
        private readonly JwtConfig _jwtConfig;

        public AuthService(CatalogContext catalogCtx,
            IClaimService claim,
            IOptionsMonitor<JwtConfig> optionsMonitor)
        {
            _catalogCtx = catalogCtx;
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
                    Message = "Username atau Kata Sandi tidak sesuai.",
                    Success = false
                };
            }

            var pwh = new PasswordHasher<UserCatalog>();
            var isCorrect = pwh.VerifyHashedPassword(catalogUser, catalogUser.Password, data.Password);

            if (isCorrect != PasswordVerificationResult.Success)
            {
                return new AuthResult
                {
                    Message = "Username atau Kata Sandi tidak sesuai.",
                    Success = false
                };
            }

            var tenant = _catalogCtx.Tenants.FirstOrDefault(x => x.Id == catalogUser.TenantId);
            if (tenant == null)
            {
                return new AuthResult
                {
                    Message = "Username atau Kata Sandi tidak sesuai.",
                    Success = false
                };
            }

            // Configure tenant context db
            var contextOptions = new DbContextOptionsBuilder<TenantContext>()
                .UseSqlServer($"Server={tenant.ServerName};Database={tenant.DatabaseName};User Id={tenant.ServerUserId};Password={tenant.ServerPassword}")
                .Options;
            var tenantCtx = new TenantContext(contextOptions, _catalogCtx, _claim);

            // Get user information details
            var tenantUser = tenantCtx.Users.FirstOrDefault(x => x.CatalogUserId == catalogUser.Id);
            if (tenantUser.IsLoggedIn)
            {
                if ((DateTime.Now - tenantUser.LastLogin.GetValueOrDefault()).TotalMinutes < _jwtConfig.TimeInMinute)
                {
                    return new AuthResult
                    {
                        Message = "Pengguna sedang digunakan.",
                        Success = false
                    };
                }
            }

            if (!tenantUser.IsActive)
            {
                return new AuthResult
                {
                    Message = "Pengguna tidak aktif.",
                    Success = false
                };
            }

            var accessIpAdd = _claim.KeyToken;
            var jwtToken = GenerateJwtToken(tenantUser, catalogUser.TenantId);

            tenantUser.IsLoggedIn = true;
            tenantUser.LastLogin = DateTime.Now;
            tenantUser.IpAddress = _claim.IpAddress;
            tenantUser.TokenId = jwtToken;
            tenantUser.SessionId = _claim.UserId.ToString();

            tenantCtx.Users.Update(tenantUser);
            tenantCtx.Entry(tenantUser).Property(e => e.CatalogUserId).IsModified = false;
            tenantCtx.Entry(tenantUser).Property(e => e.Username).IsModified = false;
            tenantCtx.Entry(tenantUser).Property(e => e.Initial).IsModified = false;
            tenantCtx.Entry(tenantUser).Property(e => e.Name).IsModified = false;
            tenantCtx.Entry(tenantUser).Property(e => e.RoleId).IsModified = false;
            tenantCtx.Entry(tenantUser).Property(e => e.EmployeeId).IsModified = false;
            tenantCtx.Entry(tenantUser).Property(e => e.IsActive).IsModified = false;
            tenantCtx.Entry(tenantUser).Property(e => e.CreatedBy).IsModified = false;
            tenantCtx.Entry(tenantUser).Property(e => e.CreatedDate).IsModified = false;
            tenantCtx.Entry(tenantUser).Property(e => e.UpdatedBy).IsModified = false;
            tenantCtx.Entry(tenantUser).Property(e => e.UpdatedDate).IsModified = false;
            tenantCtx.SaveChanges();

            return new AuthResult
            {
                accessToken = jwtToken,
                Success = true,
                Message = "Berhasil Masuk.",
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
                    Message = "Pengguna tidak terdaftar.",
                    Success = false
                };
            }

            // Configure tenant context db
            var contextOptions = new DbContextOptionsBuilder<TenantContext>().Options;
            var tenantCtx = new TenantContext(contextOptions, _catalogCtx, _claim);

            // Get user information details
            var tenantUser = tenantCtx.Users.FirstOrDefault(x => x.CatalogUserId == catalogUser.Id);
            if (tenantUser == null)
            {
                return new AuthResult
                {
                    Message = "Pengguna tidak terdaftar.",
                    Success = false
                };
            }

            tenantUser.IsLoggedIn = false;
            tenantUser.IpAddress = null;
            tenantUser.TokenId = null;
            tenantUser.SessionId = null;

            tenantCtx.Users.Update(tenantUser);
            tenantCtx.Entry(tenantUser).Property(e => e.CatalogUserId).IsModified = false;
            tenantCtx.Entry(tenantUser).Property(e => e.Username).IsModified = false;
            tenantCtx.Entry(tenantUser).Property(e => e.Initial).IsModified = false;
            tenantCtx.Entry(tenantUser).Property(e => e.Name).IsModified = false;
            tenantCtx.Entry(tenantUser).Property(e => e.RoleId).IsModified = false;
            tenantCtx.Entry(tenantUser).Property(e => e.EmployeeId).IsModified = false;
            tenantCtx.Entry(tenantUser).Property(e => e.IsActive).IsModified = false;
            tenantCtx.Entry(tenantUser).Property(e => e.CreatedBy).IsModified = false;
            tenantCtx.Entry(tenantUser).Property(e => e.CreatedDate).IsModified = false;
            tenantCtx.Entry(tenantUser).Property(e => e.UpdatedBy).IsModified = false;
            tenantCtx.Entry(tenantUser).Property(e => e.UpdatedDate).IsModified = false;
            tenantCtx.SaveChanges();

            return new AuthResult
            {
                Success = true,
                Message = "Berhasil Keluar."

            };
        }

        private string GenerateJwtToken(UserTenant data, int tenantId)
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
                    new Claim("TenantId", tenantId.ToString()),
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
