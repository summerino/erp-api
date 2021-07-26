using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using ERP.Common;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.SystemManagement;
using ERP.Web.API.Model.SystemManagement;
using UserCatalog = ERP.Entity.Catalog.User;
using UserTenant = ERP.Entity.SystemManagement.User;

namespace ERP.Web.API.Domain.Services.SystemManagement
{
    public class UserService : IUserService
    {
        private readonly CatalogContext _catalogCtx;
        private readonly IClaimService _claim;
        private readonly TenantContext _tenantCtx;

        public UserService(CatalogContext catalogContext, TenantContext tenantContext, IClaimService claim)
        {
            _catalogCtx = catalogContext;
            _claim = claim;
            _tenantCtx = tenantContext;
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts, string search)
        {
            var tenant = _catalogCtx.Tenants.FirstOrDefault(x => x.Id == _claim.TenantId);
            if (tenant == null)
            {
                return new DataSourceResult
                {
                    Total = 0
                };
            }

            var data = _tenantCtx.VwUsers.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = data.Where(x =>
                        x.Username.Contains(search) || x.Name.Contains(search) || x.RoleName.Contains(search) ||
                        x.EmployeeInitial.Contains(search) || x.Initial.Contains(search));
            }

            return data.ToDataSourceResult(skip, take, filters, sorts);
        }

        public SaveResult Insert(UserRequest data)
        {
            var result = new SaveResult(false);

            using var transaction = _catalogCtx.Database.BeginTransaction();
            try
            {
                var tenant = _catalogCtx.Tenants.FirstOrDefault(x => x.Id == _claim.TenantId);
                if (tenant == null)
                {
                    result.Message = "Tenant tidak terdaftar.";
                    return result;
                }

                var userCtg = _catalogCtx.Users.FirstOrDefault(x => x.Username == data.Username);
                if (userCtg != null)
                {
                    result.Message = "Username sudah terdaftar.";
                    return result;
                }

                var pwh = new PasswordHasher<UserCatalog>();
                var newCatalogUser = new UserCatalog()
                {
                    Id = new Guid(),
                    TenantId = _claim.TenantId,
                    Username = data.Username
                };
                var hashPwd = pwh.HashPassword(newCatalogUser, data.Password);
                newCatalogUser.Password = hashPwd;

                _catalogCtx.Add(newCatalogUser);
                _catalogCtx.SaveChanges();
                 
                var newTenantUser = new UserTenant()
                {
                    CatalogUserId = newCatalogUser.Id,
                    Username = newCatalogUser.Username,
                    Initial = data.Initial,
                    Name = data.Username,
                    RoleId = data.RoleId,
                    EmployeeId = data.EmployeeId,
                    IsActive = data.IsActive,
                    CreatedBy = data.CreatedBy,
                    CreatedDate = data.CreatedDate,
                    UpdatedBy = data.UpdatedBy,
                    UpdatedDate = data.UpdatedDate

                };

                _tenantCtx.Add(newTenantUser);
                _tenantCtx.SaveChanges();

                transaction.Commit();
            }
            catch (Exception ex)
            {
                result.Message = ex.InnerException?.Message ?? ex.Message;
                return result;
            }

            result.Success = true;
            result.Data = data.CatalogUserId;
            result.Message = "Data pengguna berhasil disimpan.";
            return result;
        }

        public SaveResult Update(UserRequest data)
        {
            var result = new SaveResult(false);

            using var transaction = _catalogCtx.Database.BeginTransaction();
            try
            {
                var tenant = _catalogCtx.Tenants.FirstOrDefault(x => x.Id == _claim.TenantId);
                if (tenant == null)
                {
                    result.Message = "Tenant tidak terdaftar.";
                    return result;
                }

                var userCtg = _catalogCtx.Users.FirstOrDefault(x => x.Username == data.Username && x.Id != data.CatalogUserId);
                if (userCtg != null)
                {
                    result.Message = "Username sudah terdaftar.";
                    return result;
                }


                var dataCatalog = _catalogCtx.Users.FirstOrDefault(x => x.Id == data.CatalogUserId);
                dataCatalog.Username = data.Username;
                if (data.Password != null)
                {
                    var pwh = new PasswordHasher<UserCatalog>();
                    dataCatalog.Password = pwh.HashPassword(dataCatalog, data.Password);
                }
                _catalogCtx.Update(dataCatalog);
                _catalogCtx.SaveChanges();

                var userTenant = _tenantCtx.Users.FirstOrDefault(x => x.Id == data.Id);

                userTenant.Username = data.Username;
                userTenant.Initial = data.Initial;
                userTenant.Name = data.Name;
                userTenant.RoleId = data.RoleId;
                userTenant.EmployeeId = data.EmployeeId;
                userTenant.MobileSignIn = data.MobileSignIn;
                userTenant.IsActive = data.IsActive;
                userTenant.UpdatedBy = data.UpdatedBy;
                userTenant.UpdatedDate = data.UpdatedDate;

                _tenantCtx.Update(userTenant);
                _tenantCtx.Entry(userTenant).Property(e => e.Id).IsModified = false;
                _tenantCtx.Entry(userTenant).Property(e => e.CatalogUserId).IsModified = false;
                _tenantCtx.Entry(userTenant).Property(e => e.CreatedBy).IsModified = false;
                _tenantCtx.Entry(userTenant).Property(e => e.CreatedDate).IsModified = false;
                _tenantCtx.SaveChanges();

                transaction.Commit();
            }
            catch (Exception ex)
            {
                result.Message = ex.InnerException?.Message ?? ex.Message;
                return result;
            }

            result.Success = true;
            result.Data = data.CatalogUserId;
            result.Message = "Data pengguna berhasil diperbarui.";
            return result;
        }

        public SaveResult Delete(int Id)
        {
            var result = new SaveResult(false);

            var tenant = _catalogCtx.Tenants.FirstOrDefault(x => x.Id == _claim.TenantId);
            if (tenant == null)
            {
                result.Message = "Tenant tidak terdaftar.";
                return result;
            }

            var data = _tenantCtx.Users.Find(Id);

            if (data != null)
            {
                // Checking active
                if (data.IsActive == false)
                {
                    result.Message = "Pengguna tidak bisa dinonaktifkan karena sudah tidak aktif.";
                    return result;
                }

                // Update data
                data.IsActive = false;
                data.UpdatedBy = _claim.UserId;
                data.UpdatedDate = DateTime.Now;

                _tenantCtx.SaveChanges();
            }

            result.Success = true;
            result.Message = "Data pengguna berhasil dinonaktifkan.";
            return result;
        }

        public SaveResult ChangePassword(UserRequest data)
        {
            var result = new SaveResult(false);

            using var transaction = _catalogCtx.Database.BeginTransaction();
            try
            {
                var tenant = _catalogCtx.Tenants.FirstOrDefault(x => x.Id == _claim.TenantId);
                if (tenant == null)
                {
                    result.Message = "Tenant tidak terdaftar.";
                    return result;
                }

                if (data.NewPassword != data.ConfirmPassword)
                {
                    result.Message = "Konfirmasi kata sandi baru tidak sama dengan kata sandi baru.";
                    return result;
                }

                var user = _tenantCtx.Users.FirstOrDefault(x => x.Id.Equals(_claim.UserId));
                if (user != null)
                {
                    var catalogUser = _catalogCtx.Users.FirstOrDefault(x => x.Username == user.Username);
                    var pwh = new PasswordHasher<UserCatalog>();
                    var isCorrect = pwh.VerifyHashedPassword(catalogUser, catalogUser.Password, data.Password);

                    if (isCorrect != PasswordVerificationResult.Success)
                    {
                        result.Message = "Kata sandi lama tidak sesuai.";
                        return result;
                    }

                    var hashPwd = pwh.HashPassword(catalogUser, data.NewPassword);
                    catalogUser.Password = hashPwd;
                    _catalogCtx.SaveChanges();
                }

                transaction.Commit();
            }
            catch (Exception ex)
            {
                result.Message = ex.InnerException?.Message ?? ex.Message;
                return result;
            }

            result.Success = true;
            result.Data = data.CatalogUserId;
            result.Message = "Kata sandi berhasil diperbarui.";
            return result;
        }
    }
}
