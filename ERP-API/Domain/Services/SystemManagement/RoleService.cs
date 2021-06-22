using System;
using System.Collections.Generic;
using System.Linq;
using ERP_API.Domain.Entities;
using ERP_API.Domain.Entities.SystemManagement;
using ERP_API.Domain.Extensions;
using ERP_API.Domain.Interfaces.SystemManagement;
using ERP_API.Domain.Models;
using ERP_API.Model.SystemManagement;

namespace ERP_API.Domain.Services.SystemManagement
{
    public class RoleService : GeneralService<Role>, IRoleService
    {
        public RoleService(TenantContext db)
            : base(db)
        {
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            List<int> category, string search)
        {
            var data = Db.VwRoles.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = data.Where(x =>
                        x.Initial.Contains(search) || x.Name.Contains(search));
            }

            return data.ToDataSourceResult(skip, take, filter, sort);
        }

        public object GetRoleMenu(int id)
        {
            var data = Db.RoleMenus.Where(x => x.RoleId == id && x.IsActive).Select(x => x.MenuId);

            return data;
        }

        public IEnumerable<RoleMenuAction> GetRoleMenuAction(int id)
        {
            var data = Db.RoleMenuActions.Where(x => x.RoleId == id);

            return data.OrderBy(x => x.Id);
        }

        public SaveResult Insert(RoleRequest data)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Checking initial already exists or not
                if (IsInitialExists(data.Initial, 0))
                {
                    result.Message = "Inisial sudah terdaftar. Tolong gunakan inisial lain.";
                    return result;
                }

                // Insert data
                Db.Roles.Add(data);
                Db.SaveChanges();

                // Disabled, add for first time
                //// Insert detail data 1
                //foreach (var item in data.RoleMenus)
                //{
                //    Db.RoleMenus.Add(new RoleMenu
                //    {
                //        RoleId = data.Id,
                //        MenuId = item.MenuId
                //    });
                //}

                //// Insert detail data 2
                //foreach (var item in data.RoleMenuActions)
                //{
                //    Db.RoleMenuActions.Add(new RoleMenuAction
                //    {
                //        RoleId = data.Id,
                //        MenuId = item.MenuId,
                //        ActionId = item.ActionId
                //    });
                //}

                Db.SaveChanges();
                transaction.Commit();
            }
            catch (Exception ex)
            {
                result.Message = ex.InnerException?.Message ?? ex.Message;
                return result;
            }

            result.Success = true;
            result.Data = data.Initial;
            result.Message = "Data peran berhasil disimpan.";
            return result;
        }

        public SaveResult Update(RoleRequest data, int id)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Checking initial already exists or not
                if (IsInitialExists(data.Initial, id))
                {
                    result.Message = "Inisial sudah terdaftar. Tolong gunakan inisial lain.";
                    return result;
                }

                // Update data
                Db.Roles.Update(data);
                Db.Entry(data).Property(e => e.Id).IsModified = false;
                Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
                Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

                // Delete existing item detail 1
                var delRoleMenus = Db.RoleMenus
                        .Where(d => d.RoleId == id && !data.RoleMenus.Select(x => x.Id).Contains(d.Id))
                        .ToList();

                Db.RoleMenus.RemoveRange(delRoleMenus);

                // Delete existing item detail 2
                var delRoleMenuActions = Db.RoleMenuActions
                        .Where(d => d.RoleId == id && !data.RoleMenuActions.Select(x => x.Id).Contains(d.Id))
                        .ToList();

                Db.RoleMenuActions.RemoveRange(delRoleMenuActions);

                // Update detail data 1
                foreach (var item in data.RoleMenus)
                {
                    if (item.Id < 0)
                    {
                        Db.RoleMenus.Add(new RoleMenu
                        {
                            RoleId = data.Id,
                            MenuId = item.MenuId,
                            IsActive = item.IsActive,
                            UpdatedBy = data.UpdatedBy,
                            UpdatedDate = item.UpdatedDate
                        });
                    }
                    else
                    {
                        Db.RoleMenus.Update(item);
                        Db.Entry(item).Property(e => e.Id).IsModified = false;
                        Db.Entry(item).Property(e => e.RoleId).IsModified = false;
                    }
                }

                // Update detail data 2
                foreach (var item in data.RoleMenuActions)
                {
                    if (item.Id < 0)
                    {
                        Db.RoleMenuActions.Add(new RoleMenuAction
                        {
                            RoleId = data.Id,
                            MenuId = item.MenuId,
                            ActionId = item.ActionId,
                            UpdatedBy = data.UpdatedBy,
                            UpdatedDate = item.UpdatedDate
                        });
                    }
                    else
                    {
                        Db.RoleMenuActions.Update(item);
                        Db.Entry(item).Property(e => e.Id).IsModified = false;
                        Db.Entry(item).Property(e => e.RoleId).IsModified = false;
                    }
                }

                Db.SaveChanges();
                transaction.Commit();
            }
            catch (Exception ex)
            {
                result.Message = ex.InnerException?.Message ?? ex.Message;
                return result;
            }

            result.Success = true;
            result.Data = data.Initial;
            result.Message = "Data peran berhasil diperbarui.";
            return result;
        }

        public SaveResult Delete(int id, int userId)
        {
            var result = new SaveResult(false);

            var data = Db.Roles.Find(id);
            if (data != null)
            {
                //Check if any user already using this role
                if (Db.Users.Any(x => x.RoleId == data.Id))
                {
                    result.Message = "Tidak bisa menghapus data peran karena telah digunakan pada data pengguna.";
                    return result;
                }

                Db.Roles.Remove(data);

                Db.SaveChanges();
            }

            result.Success = true;
            result.Message = "Data peran berhasil dihapus.";
            return result;
        }

        public bool IsInitialExists(string initial, int id)
        {
            return Db.Items.Any(x => x.Initial == initial && x.Id != id);
        }
    }
}
