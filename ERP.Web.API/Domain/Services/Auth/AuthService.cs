using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Auth;
using ERP.Web.API.Model;
using ERP.Web.API.Model.Auth;
using UserTenant = ERP.Entity.SystemManagement.User;
using UserCatalog = ERP.Entity.Catalog.User;

namespace ERP.Web.API.Domain.Services.Auth;

public class AuthService : IAuthService
{
    private readonly CatalogContext _catalogCtx;
    private readonly IClaimService _claim;
    private readonly JwtConfig _jwtConfig;
    private readonly TenantContext _tenantCtx;

    public AuthService(CatalogContext catalogCtx,
        IClaimService claim,
        IOptionsMonitor<JwtConfig> optionsMonitor,
        TenantContext tenantCtx)
    {
        _tenantCtx = tenantCtx;
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
            .UseSqlServer($"Server={tenant.ServerName};Database={tenant.DatabaseName};User Id={tenant.ServerUserId};Password={tenant.ServerPassword};MultipleActiveResultSets=True;TrustServerCertificate=True;Command Timeout=1000;Application Name=ERP")
            .Options;
        var tenantCtx = new TenantContext(contextOptions, _catalogCtx, _claim);

        // Get user information details
        var tenantUser = tenantCtx.Users.FirstOrDefault(x => x.CatalogUserId == catalogUser.Id);
        if (tenantUser.IsLoggedIn)
        {
            if ((DateTime.Now - tenantUser.LastLogin.GetValueOrDefault()).TotalMinutes < _jwtConfig.ExpiresInMinute)
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

        string defaultWarehouseCode = "";
        var temp = tenantCtx.Employees.SingleOrDefault(x => x.Id.Equals(tenantUser.Id));

        if (temp == null)
        {
            return new AuthResult
            {
                Message = "Data karyawan tidak ditemukan.",
                Success = false
            };
        }
        else {
            defaultWarehouseCode = temp.WarehouseCode;
        }

        tenantUser.IsLoggedIn = true;
        tenantUser.LastLogin = DateTime.Now;
        tenantUser.SessionId = Guid.NewGuid().ToString();
        tenantUser.TokenId = GenerateJwtToken(tenantUser, catalogUser.TenantId, tenant.Initial, defaultWarehouseCode);
        tenantUser.IpAddress = _claim.IpAddress;

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
            AccessToken = tenantUser.TokenId,
            ExpToken = EpochTime.GetIntDate(tenantUser.LastLogin.Value.AddMinutes(_jwtConfig.ExpiresInMinute)),
            UserData = catalogUser.Id.ToString(),
            Success = true
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

    private string GenerateJwtToken(UserTenant data, int tenantId, string tenantInitial,
        string defaultWarehouseCode)
    {
        var jwtTokenHandler = new JwtSecurityTokenHandler();

        var key = Encoding.ASCII.GetBytes(_jwtConfig.Secret);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, data.Username),
                new Claim(JwtRegisteredClaimNames.Jti, data.SessionId),
                new Claim(JwtRegisteredClaimNames.GivenName, data.Initial),
                new Claim("UserId", data.Id.ToString()),
                new Claim("RoleId", data.RoleId.ToString()),
                new Claim("CatalogUserId", data.CatalogUserId.ToString()),
                new Claim("TenantId", tenantId.ToString()),
                new Claim("TenantInitial", tenantInitial),
                new Claim("WarehouseCode", defaultWarehouseCode == null ? string.Empty : defaultWarehouseCode)
            }),
            Expires = data.LastLogin.GetValueOrDefault(DateTime.Now).AddMinutes(_jwtConfig.ExpiresInMinute),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = _jwtConfig.Issuer,
            NotBefore = data.LastLogin.GetValueOrDefault(DateTime.Now)
        };

        var token = jwtTokenHandler.CreateJwtSecurityToken(tokenDescriptor);
        var jwtToken = jwtTokenHandler.WriteToken(token);

        return jwtToken;
    }

    public IEnumerable<int> GetActions(int menuId, int roleId, Actions[] actions)
    {
        var arr = actions.Select(x => (int)x).ToList();
        return GetActions(menuId, roleId, arr);
    }

    public IEnumerable<int> GetActions(int menuId, int roleId, List<int> actions)
    {
        var data = _tenantCtx.RoleMenuActions.Where(x => x.MenuId == menuId && x.RoleId == roleId);
            
        if (actions.Any())
            data = data.Where(x => actions.Contains(x.ActionId));
            
        return data.Select(x => x.ActionId);
    }
}