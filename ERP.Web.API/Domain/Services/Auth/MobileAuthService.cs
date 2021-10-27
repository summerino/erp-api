using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Auth;
using ERP.Web.API.Model;
using ERP.Web.API.Model.Auth;
using UserTenant = ERP.Entity.SystemManagement.User;
using UserCatalog = ERP.Entity.Catalog.User;
using CustomerCatalog = ERP.Entity.Catalog.CustomerUser;
using UserCustomer = ERP.Entity.General.Customer;

namespace ERP.Web.API.Domain.Services.Auth
{
    public class MobileAuthService : IMobileAuthService
    {
        protected TenantContext Db;
        public MobileAuthService(TenantContext db)
        {
            Db = db;
        }

        private readonly CatalogContext _catalogCtx;
        private readonly IClaimService _claim;
        private readonly JwtConfig _jwtConfig;
        private readonly TenantContext _tenantCtx;

        public MobileAuthService(CatalogContext catalogCtx,
            IClaimService claim,
            IOptionsMonitor<JwtConfig> optionsMonitor,
            TenantContext tenantCtx)
        {
            _tenantCtx = tenantCtx;
            _catalogCtx = catalogCtx;
            _claim = claim;
            _jwtConfig = optionsMonitor.CurrentValue;
        }

        public MobileAuthResult Login(MobileLoginRequest data)
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            var allowedType = config["MobileApiSettings:UserTypeAllowed"];
            var allowedTypes = new List<int>();
            if (!string.IsNullOrEmpty(allowedType) && !string.IsNullOrWhiteSpace(allowedType))
            {
                var tempAllowedTypes = allowedType.Split(',');
                foreach (var item in tempAllowedTypes)
                {
                    int.TryParse(item, out var value);
                    allowedTypes.Add(value);
                }

                if (!allowedTypes.Contains(data.Type))
                {
                    return new MobileAuthResult
                    {
                        Message = "Data type tidak sesuai.",
                        Success = false
                    };
                }
            }

            var catalogUser = _catalogCtx.Users.FirstOrDefault(x => x.Username == data.Username);
            if (catalogUser == null)
            {
                return new MobileAuthResult
                {
                    Message = "Username atau Kata Sandi tidak sesuai.",
                    Success = false
                };
            }

            var pwh = new PasswordHasher<UserCatalog>();
            var isCorrect = pwh.VerifyHashedPassword(catalogUser, catalogUser.Password, data.Password);

            if (isCorrect != PasswordVerificationResult.Success)
            {
                return new MobileAuthResult
                {
                    Message = "Username atau Kata Sandi tidak sesuai.",
                    Success = false
                };
            }

            var tenant = _catalogCtx.Tenants.FirstOrDefault(x => x.Id == catalogUser.TenantId);
            if (tenant == null)
            {
                return new MobileAuthResult
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
            var tenantUser = tenantCtx.Users.FirstOrDefault(x => x.CatalogUserId == catalogUser.Id && x.MobileSignIn && x.IsActive);
            if (tenantUser == null)
            {
                return new MobileAuthResult
                {
                    Message = "Username atau Kata Sandi tidak sesuai.",
                    Success = false
                };
            }

            if (tenantUser.IsMobileLoggedIn)
            {
                if ((DateTime.Now - tenantUser.MobileLastLogin.GetValueOrDefault()).TotalMinutes < _jwtConfig.TimeInMinute)
                {
                    return new MobileAuthResult
                    {
                        Message = "Pengguna sedang digunakan.",
                        Success = false
                    };
                }
            }

            // Get employee information details
            var tenantEmployee = tenantCtx.Employees.FirstOrDefault(x => x.Id == tenantUser.EmployeeId && allowedTypes.Contains(x.Type) && x.Type == data.Type
          );
            if (tenantEmployee == null)
            {
                return new MobileAuthResult
                {
                    Message = "Data karyawan tidak ditemukan.",
                    Success = false
                };
            }

            var defaultWarehouseCode = tenantEmployee.WarehouseCode ?? tenantCtx.Warehouses.FirstOrDefault(x => x.IsDefault)?.Code ?? "";

            tenantUser.IsMobileLoggedIn = true;
            tenantUser.MobileLastLogin = DateTime.Now;
            tenantUser.MobileSessionId = Guid.NewGuid().ToString();
            tenantUser.MobileTokenId = GenerateJwtToken(tenantUser, catalogUser.TenantId, defaultWarehouseCode);
            tenantUser.MobileIpAddress = _claim.IpAddress;

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
            tenantCtx.Entry(tenantUser).Property(e => e.IsLoggedIn).IsModified = false;
            tenantCtx.Entry(tenantUser).Property(e => e.LastLogin).IsModified = false;
            tenantCtx.Entry(tenantUser).Property(e => e.SessionId).IsModified = false;
            tenantCtx.Entry(tenantUser).Property(e => e.TokenId).IsModified = false;
            tenantCtx.Entry(tenantUser).Property(e => e.IpAddress).IsModified = false;
            tenantCtx.SaveChanges();

            return new MobileAuthResult
            {
                AccessToken = tenantUser.MobileTokenId,
                ExpToken = EpochTime.GetIntDate(tenantUser.MobileLastLogin.Value.AddMinutes(_jwtConfig.TimeInMinute)),
                UserData = catalogUser.Id.ToString(),
                Success = true
            };
        }

        public MobileAuthResult LoginWarehouse(MobileLoginRequest data)
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            var allowedType = config["AppSettings:userTypeAllowed"];
            var allowedTypes = new List<int>();
            var salesmanId = _tenantCtx.Users.Where(x => x.Username.Equals(data.Username)).Select(x => x.EmployeeId).SingleOrDefault();
            var employeeType = _tenantCtx.Employees.Where(x => x.Id.Equals(salesmanId)).Select(x => x.Type).SingleOrDefault();

            if (!string.IsNullOrEmpty(allowedType) && !string.IsNullOrWhiteSpace(allowedType))
            {
                var tempAllowedTypes = allowedType.Split(',');
                foreach (var item in tempAllowedTypes)
                {
                    int.TryParse(item, out var value);
                    allowedTypes.Add(value);
                }

                if (!allowedTypes.Contains(data.Type))
                {
                    return new MobileAuthResult
                    {
                        Message = "Data type tidak sesuai.",
                        Success = false
                    };
                }

                if (employeeType != 4)
                {
                    return new MobileAuthResult
                    {
                        Message = "Pengguna tidak memiliki akses.",
                        Success = false
                    };
                }
            }

            var catalogUser = _catalogCtx.Users.FirstOrDefault(x => x.Username == data.Username);
            if (catalogUser == null)
            {
                return new MobileAuthResult
                {
                    Message = "Username atau Kata Sandi tidak sesuai.",
                    Success = false
                };
            }

            var pwh = new PasswordHasher<UserCatalog>();
            var isCorrect = pwh.VerifyHashedPassword(catalogUser, catalogUser.Password, data.Password);

            if (isCorrect != PasswordVerificationResult.Success)
            {
                return new MobileAuthResult
                {
                    Message = "Username atau Kata Sandi tidak sesuai.",
                    Success = false
                };
            }

            var tenant = _catalogCtx.Tenants.FirstOrDefault(x => x.Id == catalogUser.TenantId);
            if (tenant == null)
            {
                return new MobileAuthResult
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
            var tenantUser = tenantCtx.Users.FirstOrDefault(x => x.CatalogUserId == catalogUser.Id && x.MobileSignIn && x.IsActive);
            if (tenantUser == null)
            {
                return new MobileAuthResult
                {
                    Message = "Username atau Kata Sandi tidak sesuai.",
                    Success = false
                };
            }

            if (tenantUser.IsMobileLoggedIn)
            {
                if ((DateTime.Now - tenantUser.MobileLastLogin.GetValueOrDefault()).TotalMinutes < _jwtConfig.TimeInMinute)
                {
                    return new MobileAuthResult
                    {
                        Message = "Pengguna sedang digunakan.",
                        Success = false
                    };
                }
            }

            // Get employee information details
            var tenantEmployee = tenantCtx.Employees.FirstOrDefault(x => x.Id == tenantUser.EmployeeId && allowedTypes.Contains(x.Type));
            if (tenantEmployee == null)
            {
                return new MobileAuthResult
                {
                    Message = "Data karyawan tidak ditemukan.",
                    Success = false
                };
            }

            var defaultWarehouseCode = tenantEmployee.WarehouseCode ?? tenantCtx.Warehouses.FirstOrDefault(x => x.IsDefault)?.Code ?? "";

            tenantUser.IsMobileLoggedIn = true;
            tenantUser.MobileLastLogin = DateTime.Now;
            tenantUser.MobileSessionId = Guid.NewGuid().ToString();
            tenantUser.MobileTokenId = GenerateJwtToken(tenantUser, catalogUser.TenantId, defaultWarehouseCode);
            tenantUser.MobileIpAddress = _claim.IpAddress;

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
            tenantCtx.Entry(tenantUser).Property(e => e.IsLoggedIn).IsModified = false;
            tenantCtx.Entry(tenantUser).Property(e => e.LastLogin).IsModified = false;
            tenantCtx.Entry(tenantUser).Property(e => e.SessionId).IsModified = false;
            tenantCtx.Entry(tenantUser).Property(e => e.TokenId).IsModified = false;
            tenantCtx.Entry(tenantUser).Property(e => e.IpAddress).IsModified = false;
            tenantCtx.SaveChanges();

            return new MobileAuthResult
            {
                AccessToken = tenantUser.MobileTokenId,
                ExpToken = EpochTime.GetIntDate(tenantUser.MobileLastLogin.Value.AddMinutes(_jwtConfig.TimeInMinute)),
                UserData = catalogUser.Id.ToString(),
                Success = true
            };
        }

        public MobileSimpleResponse ChangePassword(MobileChangePasswordRequest data)
        {
            if (data.NewPassword != data.ConfirmPassword)
            {
                return new MobileSimpleResponse
                {
                    Message = "Kata Sandi tidak sama",
                    Success = false
                };
            }
            var user = _tenantCtx.Users.FirstOrDefault(x => x.Id.Equals(_claim.UserId));
            if (user != null)
            {
                var catalogUser = _catalogCtx.Users.FirstOrDefault(x => x.Username == user.Username);
                var pwh = new PasswordHasher<UserCatalog>();
                var isCorrect = pwh.VerifyHashedPassword(catalogUser, catalogUser.Password, data.OldPassword);

                if (isCorrect != PasswordVerificationResult.Success)
                {
                    return new MobileSimpleResponse
                    {
                        Message = "Kata Sandi tidak sesuai.",
                        Success = false
                    };
                }
                var hashPwd = pwh.HashPassword(catalogUser, data.NewPassword);
                catalogUser.Password = hashPwd;
                _catalogCtx.SaveChanges();
            }

            return new MobileSimpleResponse { Success = true, Message = "Kata Sandi berhasil dirubah." };
        }
        public MobileAuthResult Logout()
        {
            var catalogUser = _catalogCtx.Users.FirstOrDefault(x => x.Id.ToString() == _claim.CatalogUserId);
            if (catalogUser == null)
            {
                return new MobileAuthResult
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
                return new MobileAuthResult
                {
                    Message = "Pengguna tidak terdaftar.",
                    Success = false
                };
            }

            tenantUser.IsMobileLoggedIn = false;
            tenantUser.MobileIpAddress = null;
            tenantUser.MobileTokenId = null;
            tenantUser.MobileSessionId = null;

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
            tenantCtx.Entry(tenantUser).Property(e => e.IsLoggedIn).IsModified = false;
            tenantCtx.Entry(tenantUser).Property(e => e.LastLogin).IsModified = false;
            tenantCtx.Entry(tenantUser).Property(e => e.SessionId).IsModified = false;
            tenantCtx.Entry(tenantUser).Property(e => e.TokenId).IsModified = false;
            tenantCtx.Entry(tenantUser).Property(e => e.IpAddress).IsModified = false;
            tenantCtx.SaveChanges();

            return new MobileAuthResult
            {
                Success = true,
                Message = "Berhasil Keluar."
            };
        }
        private string GenerateJwtToken(UserTenant data, int tenantId, string defaultWarehouseCode)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.ASCII.GetBytes(_jwtConfig.Secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, data.Username),
                    new Claim(JwtRegisteredClaimNames.Jti, data.MobileSessionId),
                    new Claim(JwtRegisteredClaimNames.GivenName, data.Initial),
                    new Claim("UserId", data.Id.ToString()),
                    new Claim("RoleId", data.RoleId.ToString()),
                    new Claim("CatalogUserId", data.CatalogUserId.ToString()),
                    new Claim("TenantId", tenantId.ToString()),
                    new Claim("WarehouseCode", defaultWarehouseCode)
                }),
                Expires = data.MobileLastLogin.GetValueOrDefault(DateTime.Now).AddMinutes(_jwtConfig.TimeInMinute),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _jwtConfig.Issuer,
                NotBefore = data.MobileLastLogin.GetValueOrDefault(DateTime.Now)
            };

            var token = jwtTokenHandler.CreateJwtSecurityToken(tokenDescriptor);
            var jwtToken = jwtTokenHandler.WriteToken(token);

            return jwtToken;
        }

        private string GenerateJwtTokenCustomer(UserCustomer data, int tenantId)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.ASCII.GetBytes(_jwtConfig.Secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, data.MobileUsername),
                    new Claim(JwtRegisteredClaimNames.Jti, data.MobileSessionId),
                    new Claim(JwtRegisteredClaimNames.GivenName, data.Initial),
                    new Claim("Code", data.Code),
                    new Claim("CatalogUserId", data.CatalogUserId.ToString()),
                    new Claim("TenantId", tenantId.ToString()),
                }),
                Expires = data.MobileLastLogin.GetValueOrDefault(DateTime.Now).AddMinutes(_jwtConfig.TimeInMinute),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _jwtConfig.Issuer,
                NotBefore = data.MobileLastLogin.GetValueOrDefault(DateTime.Now)
            };

            var token = jwtTokenHandler.CreateJwtSecurityToken(tokenDescriptor);
            var jwtToken = jwtTokenHandler.WriteToken(token);

            return jwtToken;
        }

        public MobileAuthResult LoginCustomer(MobileLoginCustomerRequest data)
        {
            var catalogUser = _catalogCtx.CustomerUsers.FirstOrDefault(x => x.Username == data.Username);
            if (catalogUser == null)
            {
                return new MobileAuthResult
                {
                    Message = "Username atau Kata Sandi tidak sesuai.",
                    Success = false
                };
            }

            var pwh = new PasswordHasher<CustomerCatalog>();
            var isCorrect = pwh.VerifyHashedPassword(catalogUser, catalogUser.Password, data.Password);

            if (isCorrect != PasswordVerificationResult.Success)
            {
                return new MobileAuthResult
                {
                    Message = "Username atau Kata Sandi tidak sesuai.",
                    Success = false
                };
            }

            var tenant = _catalogCtx.Tenants.FirstOrDefault(x => x.Id == catalogUser.TenantId);
            if (tenant == null)
            {
                return new MobileAuthResult
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
            var tenantUser = tenantCtx.Customers.FirstOrDefault(x => x.CatalogUserId == catalogUser.Id && x.MobileSignIn && x.IsActive);
            if (tenantUser == null)
            {
                return new MobileAuthResult
                {
                    Message = "Username atau Kata Sandi tidak sesuai.",
                    Success = false
                };
            }

            if (tenantUser.IsMobileLoggedIn)
            {
                if ((DateTime.Now - tenantUser.MobileLastLogin.GetValueOrDefault()).TotalMinutes < _jwtConfig.TimeInMinute)
                {
                    return new MobileAuthResult
                    {
                        Message = "Pengguna sedang digunakan.",
                        Success = false
                    };
                }
            }

            // Get employee information details
            var tenantEmployee = tenantCtx.Customers.FirstOrDefault(x => x.Code == tenantUser.Code 
          );
            if (tenantEmployee == null)
            {
                return new MobileAuthResult
                {
                    Message = "Data karyawan tidak ditemukan.",
                    Success = false
                };
            }


            tenantUser.IsMobileLoggedIn = true;
            tenantUser.MobileLastLogin = DateTime.Now;
            tenantUser.MobileSessionId = Guid.NewGuid().ToString();
            tenantUser.MobileTokenId = GenerateJwtTokenCustomer(tenantUser, catalogUser.TenantId);
            tenantUser.MobileIpAddress = _claim.IpAddress;

            tenantCtx.Customers.Update(tenantUser);
            tenantCtx.Entry(tenantUser).Property(e => e.CatalogUserId).IsModified = false;
            tenantCtx.Entry(tenantUser).Property(e => e.MobileUsername).IsModified = false;
            tenantCtx.Entry(tenantUser).Property(e => e.Initial).IsModified = false;
            tenantCtx.Entry(tenantUser).Property(e => e.Name).IsModified = false;
            tenantCtx.Entry(tenantUser).Property(e => e.IsActive).IsModified = false;
            tenantCtx.Entry(tenantUser).Property(e => e.CreatedBy).IsModified = false;
            tenantCtx.Entry(tenantUser).Property(e => e.CreatedDate).IsModified = false;
            tenantCtx.Entry(tenantUser).Property(e => e.UpdatedBy).IsModified = false;
            tenantCtx.Entry(tenantUser).Property(e => e.UpdatedDate).IsModified = false;
            tenantCtx.Entry(tenantUser).Property(e => e.IsMobileLoggedIn).IsModified = false;
            tenantCtx.Entry(tenantUser).Property(e => e.MobileLastLogin).IsModified = false;
            tenantCtx.Entry(tenantUser).Property(e => e.MobileSessionId).IsModified = false;
            tenantCtx.Entry(tenantUser).Property(e => e.MobileTokenId).IsModified = false;
            tenantCtx.Entry(tenantUser).Property(e => e.MobileIpAddress).IsModified = false;
            tenantCtx.SaveChanges();

            return new MobileAuthResult
            {
                AccessToken = tenantUser.MobileTokenId,
                ExpToken = EpochTime.GetIntDate(tenantUser.MobileLastLogin.Value.AddMinutes(_jwtConfig.TimeInMinute)),
                UserData = catalogUser.Id.ToString(),
                Success = true
            };
        }

        //public IEnumerable<int> GetActions(int menuId, int roleId, Actions[] actions)
        //{
        //    var arr = actions.Select(x => (int)x).ToList();
        //    return GetActions(menuId, roleId, arr);
        //}

        //public IEnumerable<int> GetActions(int menuId, int roleId, List<int> actions)
        //{
        //    var query = _tenantCtx.RoleMenuActions.Where(x => x.MenuId.Equals(menuId) && x.RoleId.Equals(roleId) && actions.Contains(x.ActionId));
        //    return query.Select(x=>x.ActionId);
        //}

    }
}
