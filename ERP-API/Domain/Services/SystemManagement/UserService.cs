using ERP_API.Domain.Entities;
using ERP_API.Domain.Entities.Catalog;
using ERP_API.Domain.Entities.SystemManagement;
using ERP_API.Domain.Interfaces.SystemManagement;
using ERP_API.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using VMUserTenant = ERP_API.Domain.Entities.SystemManagement.VMUser;
using UserTenant = ERP_API.Domain.Entities.SystemManagement.User;
using UserCatalog = ERP_API.Domain.Entities.Catalog.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using ERP_API.Domain.Extensions;

namespace ERP_API.Domain.Services.SystemManagement
{
    public class UserService : IUserService
    {
        private readonly CatalogContext _catalogCtx;
        private readonly IClaimService _claim;

        public UserService(CatalogContext catalogContext, IClaimService claim)
        {
            _catalogCtx = catalogContext;
            _claim = claim;
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

            var contextOptions = new DbContextOptionsBuilder<TenantContext>()
            .UseSqlServer($"Server={tenant.ServerName};Database={tenant.DatabaseName};User Id={tenant.ServerUserId};Password={tenant.ServerPassword}")
            .Options;
            var tenantCtx = new TenantContext(contextOptions, _catalogCtx, _claim);

            var data = tenantCtx.Users.Find(Id);

            if (data != null)
            {
                // Checking active
                if (data.IsActive == false)
                {
                    result.Message = "Tidak bisa menonaktifkan data pengguna karena data sudah nonaktif.";
                    return result;
                }

                // Update data
                data.IsActive = false;
                data.UpdatedBy = _claim.UserId;
                data.UpdatedDate = DateTime.Now;

                tenantCtx.SaveChanges();
            }

            result.Success = true;
            result.Message = "Data pengguna berhasil dinonaktifkan.";
            return result;
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts, string search)
        {
            var tenant = _catalogCtx.Tenants.FirstOrDefault(x => x.Id == _claim.TenantId);
            if (tenant == null)
            {
                return new DataSourceResult{
                    Total = 0
                };
            }

            var contextOptions = new DbContextOptionsBuilder<TenantContext>()
            .UseSqlServer($"Server={tenant.ServerName};Database={tenant.DatabaseName};User Id={tenant.ServerUserId};Password={tenant.ServerPassword}")
            .Options;
            var tenantCtx = new TenantContext(contextOptions, _catalogCtx, _claim);

            var data = tenantCtx.VwUsers.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = data.Where(x =>
                        x.Username.Contains(search) || x.Name.Contains(search) || x.RoleName.Contains(search) ||
                        x.EmployeeUsername.Contains(search) || x.Initial.Contains(search));
            }

            return data.ToDataSourceResult(skip, take, filters, sorts);
        }

        public SaveResult Insert(VMUserTenant dataTenant, string password)
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

                var contextOptions = new DbContextOptionsBuilder<TenantContext>()
                .UseSqlServer($"Server={tenant.ServerName};Database={tenant.DatabaseName};User Id={tenant.ServerUserId};Password={tenant.ServerPassword}")
                .Options;
                var tenantCtx = new TenantContext(contextOptions, _catalogCtx, _claim);

                var existingUser = tenantCtx.Users.Where(x => x.Username == dataTenant.Username).FirstOrDefault();
                if (existingUser != null)
                {
                    result.Message = "Nama pengguna sudah terdaftar. Tolong gunakan nama pengguna lain.";
                    return result;
                }


                var pwh = new PasswordHasher<UserCatalog>();
                var newCatalogUser = new UserCatalog()
                {
                    Id = new Guid(),
                    TenantId = _claim.TenantId,
                    Username = dataTenant.Username
                };
                var hashPwd = pwh.HashPassword(newCatalogUser, password);
                newCatalogUser.Password = hashPwd;

                _catalogCtx.Add(newCatalogUser);
                _catalogCtx.SaveChanges();

                var newTenantUser = new UserTenant()
                {
                    CatalogUserId = newCatalogUser.Id,
                    Username = newCatalogUser.Username,
                    Initial = dataTenant.Initial,
                    Name = dataTenant.Username,
                    RoleId = dataTenant.RoleId,
                    EmployeeId = dataTenant.EmployeeId,
                    IsActive = dataTenant.IsActive,
                    CreatedBy = dataTenant.CreatedBy,
                    CreatedDate = dataTenant.CreatedDate,
                    UpdatedBy = dataTenant.UpdatedBy,
                    UpdatedDate = dataTenant.UpdatedDate

                };

                tenantCtx.Add(newTenantUser);
                tenantCtx.SaveChanges();

                transaction.Commit();
            }
            catch (Exception ex)
            {
                result.Message = ex.InnerException?.Message ?? ex.Message;
                return result;
            }

            result.Success = true;
            result.Data = dataTenant.CatalogUserId;
            result.Message = "Data pengguna berhasil disimpan.";
            return result;
        }

        public SaveResult Update(VMUserTenant dataTenant, string password)
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

                var contextOptions = new DbContextOptionsBuilder<TenantContext>()
                .UseSqlServer($"Server={tenant.ServerName};Database={tenant.DatabaseName};User Id={tenant.ServerUserId};Password={tenant.ServerPassword}")
                .Options;
                var tenantCtx = new TenantContext(contextOptions, _catalogCtx, _claim);

                if (password != null)
                {
                    var dataCatalog = _catalogCtx.Users.FirstOrDefault(x => x.Id == dataTenant.CatalogUserId);
                    var pwh = new PasswordHasher<UserCatalog>();
                    dataCatalog.Password = pwh.HashPassword(dataCatalog, password);
                    _catalogCtx.Update(dataCatalog);
                    _catalogCtx.SaveChanges();
                }

                var userTenant = tenantCtx.Users.FirstOrDefault(x => x.Id == dataTenant.Id);
               
                userTenant.Username = dataTenant.Username;
                userTenant.Initial = dataTenant.Initial;
                userTenant.Name = dataTenant.Name;
                userTenant.RoleId = dataTenant.RoleId;
                userTenant.EmployeeId = dataTenant.EmployeeId;
                userTenant.IsActive = dataTenant.IsActive;
                userTenant.UpdatedBy = dataTenant.UpdatedBy;
                userTenant.UpdatedDate = dataTenant.UpdatedDate;

                tenantCtx.Update(userTenant);
                tenantCtx.Entry(userTenant).Property(e => e.Id).IsModified = false;
                tenantCtx.Entry(userTenant).Property(e => e.CatalogUserId).IsModified = false;
                tenantCtx.Entry(userTenant).Property(e => e.CreatedBy).IsModified = false;
                tenantCtx.Entry(userTenant).Property(e => e.CreatedDate).IsModified = false;
                tenantCtx.SaveChanges();
            }
            catch (Exception ex)
            {
                result.Message = ex.InnerException?.Message ?? ex.Message;
                return result;
            }

            result.Success = true;
            result.Data = dataTenant.CatalogUserId;
            result.Message = "Data pengguna berhasil diperbarui.";
            return result;
        }
    }
}
