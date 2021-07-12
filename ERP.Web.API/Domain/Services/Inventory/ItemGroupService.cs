using System;
using System.Collections.Generic;
using System.Linq;
using ERP.Entity;
using ERP.Entity.Inventory;
using ERP.Web.API.Domain.Extensions;
using ERP.Web.API.Domain.Interfaces.Inventory;
using ERP.Web.API.Domain.Models;
using ERP.Web.API.Model.Inventory;

namespace ERP.Web.API.Domain.Services.Inventory
{
    public class ItemGroupService : GeneralService<ItemGroup>, IItemGroupService
    {
        public ItemGroupService(TenantContext db)
            : base(db)
        {
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            List<int> category, string search)
        {
            var data = Db.VwItemGroups.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = data.Where(x =>
                            x.Initial.Contains(search) || x.Name.Contains(search) && x.IsActive);
            } else
            {
                data = data.Where(x => x.IsActive);
            }

            return data.ToDataSourceResult(skip, take, filter, sort);
        }

        public IEnumerable<ItemGroupSubGroup> GetDetailData(int id)
        {
            var data = Db.ItemGroupSubGroups.Where(x => x.ItemGroupId == id);

            return data.OrderBy(x => x.Id);
        }

        public IEnumerable<ItemGroup> GetLists()
        {
            var data = Db.ItemGroups.Where(x => x.IsActive);

            return data.OrderBy(x => x.Id);
        }

        public IEnumerable<ItemGroupSubGroup> GetDetailById(int id)
        {
            var data = Db.ItemGroups.Where(x => x.Id == id).ToList();

            if (data != null && data.Count > 0)
            {
                var detailData = Db.ItemGroupSubGroups.Where(x => x.ItemGroupId == data[0].Id);

                return detailData.OrderBy(x => x.Id);
            } else
            {
                return Db.ItemGroupSubGroups.Where(x => x.Id == 0);
            }
        }

        public SaveResult Insert(ItemGroupRequest data)
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
                Db.ItemGroups.Add(data);
                Db.SaveChanges();

                // Insert detail data
                foreach (var item in data.ItemDetails)
                {
                    Db.ItemGroupSubGroups.Add(new ItemGroupSubGroup
                    {
                        ItemGroupId = data.Id,
                        Name = item.Name,
                        Value = item.Value
                    });
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
            result.Message = "Data kelompok barang berhasil disimpan.";
            return result;
        }

        public SaveResult Update(ItemGroupRequest data)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Checking initial already exists or not
                if (IsInitialExists(data.Initial, data.Id))
                {
                    result.Message = "Inisial sudah terdaftar. Tolong gunakan inisial lain.";
                    return result;
                }

                // Update data
                Db.ItemGroups.Update(data);
                Db.Entry(data).Property(e => e.Id).IsModified = false;
                Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
                Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

                // Delete existing item detail
                var delDetails = Db.ItemGroupSubGroups
                        .Where(d => d.ItemGroupId == data.Id && !data.ItemDetails.Select(x => x.Id).Contains(d.Id))
                        .ToList();

                Db.ItemGroupSubGroups.RemoveRange(delDetails);

                // Update detail data
                foreach (var item in data.ItemDetails)
                {
                    if (item.Id < 0)
                    {
                        Db.ItemGroupSubGroups.Add(new ItemGroupSubGroup
                        {
                            ItemGroupId = data.Id,
                            Name = item.Name,
                            Value = item.Value
                        });
                    }
                    else
                    {
                        Db.ItemGroupSubGroups.Update(item);
                        Db.Entry(item).Property(e => e.Id).IsModified = false;
                        Db.Entry(item).Property(e => e.ItemGroupId).IsModified = false;
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
            result.Message = "Data kelompok barang berhasil diperbarui.";
            return result;
        }

        public SaveResult Delete(int id, int userId)
        {
            var result = new SaveResult(false);

            var data = Db.ItemGroups.Find(id);
            if (data != null)
            {
                // Checking active
                if (!data.IsActive)
                {
                    result.Message = "Tidak bisa menonaktifkan data kelompok barang karena data sudah nonaktif.";
                    return result;
                }

                // Update data
                data.IsActive = false;
                data.UpdatedBy = userId;
                data.UpdatedDate = DateTime.Now;

                Db.SaveChanges();
            }

            result.Success = true;
            result.Message = "Data kelompok barang berhasil dinonaktifkan.";
            return result;
        }

        public bool IsInitialExists(string initial, int id)
        {
            return Db.Items.Any(x => x.Initial == initial && x.Id != id);
        }
    }
}
