using System;
using System.Collections.Generic;
using System.Linq;
using ERP_API.Domain.Entities;
using ERP_API.Domain.Entities.Inventory;
using ERP_API.Domain.Extensions;
using ERP_API.Domain.Interfaces.Inventory;
using ERP_API.Domain.Models;
using ERP_API.Model.Inventory;

namespace ERP_API.Domain.Services.Inventory
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
            var data = Db.ItemGroups.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = data.Where(x =>
                            x.Initial.Contains(search) || x.Name.Contains(search));
            }

            return data.ToDataSourceResult(skip, take, filter, sort);
        }

        public IEnumerable<ItemGroupSubGroup> GetDetailData(int id)
        {
            var data = Db.ItemGroupSubGroups.Where(x => x.ItemGroupId == id);

            return data.OrderBy(x => x.Id);
        }

        public IEnumerable<ItemGroup> GetItemGroup()
        {
            var data = Db.ItemGroups.Where(x => x.IsActive);

            return data.OrderBy(x => x.Id);
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
                    result.Message = "Initial is already exists. Please use another initial.";
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
            result.Message = "Success insert item group.";
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
                    result.Message = "Initial is already exists. Please use another initial.";
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
            result.Message = "Success update item group.";
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
                    result.Message = "Can't inactive the item group because data already inactive.";
                    return result;
                }

                // Update data
                data.IsActive = false;
                data.UpdatedBy = userId;
                data.UpdatedDate = DateTime.Now;

                Db.SaveChanges();
            }

            result.Success = true;
            result.Message = "Success inactive item group.";
            return result;
        }

        public bool IsInitialExists(string initial, int id)
        {
            return Db.Items.Any(x => x.Initial == initial && x.Id != id);
        }
    }
}
