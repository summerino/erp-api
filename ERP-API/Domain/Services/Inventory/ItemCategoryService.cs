using System;
using System.Collections.Generic;
using System.Linq;
using ERP_API.Domain.Entities;
using ERP_API.Domain.Entities.Inventory;
using ERP_API.Domain.Extensions;
using ERP_API.Domain.Interfaces.Inventory;
using ERP_API.Domain.Models;

namespace ERP_API.Domain.Services.Inventory
{
    public class ItemCategoryService: GeneralService<ItemCategory>, IItemCategoryService
    {
        public ItemCategoryService(TenantContext db)
            : base(db)
        {
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            string search)
        {
            var data = Db.ItemCategories.AsQueryable();

            if (!string.IsNullOrEmpty(search))
                data = data.Where(x => x.Initial.Contains(search) || x.Name.Contains(search));

            return data.ToDataSourceResult(skip, take, filter, sort);
        }

        public IEnumerable<ItemCategory> GetLists()
        {
            var data = Db.ItemCategories.Where(x => x.IsActive);

            return data.OrderBy(x => x.Name);
        }

        public object GetHierarchy()
        {
            return new
            {
                Id = 0,
                Initial = "",
                Name = "Semua Kategori",
                ParentId = 0,
                GroupId = 0,
                Deep = 0,
                Seq = 0,
                Lineage = "",
                IsParent = true,
                Children = DefineChildNodes(Db.ItemCategories.Where(x => x.IsActive).ToList())
            };
        }

        private static object DefineChildNodes(List<ItemCategory> data, int? parentId = null)
        {
            var nodes = data
                .Where(x => x.ParentId == parentId)
                .OrderBy(x => x.Seq)
                .Select(x => new
                {
                    x.Id,
                    x.Initial,
                    x.Name,
                    x.ParentId,
                    x.GroupId,
                    x.Deep,
                    x.Seq,
                    x.Lineage,
                    IsParent = IsParent(data, x.Id),
                    Children = DefineChildNodes(data, x.Id)
                });

            return nodes;
        }

        public override SaveResult Insert(ItemCategory data)
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

                // Checking max seq & update Seq
                int? seq = getSequence(data.ParentId);
                if (seq.HasValue)
                {
                    seq += 1;
                    data.Seq = seq;
                } else
                {
                    data.Seq = 1;
                }

                Db.ItemCategories.Add(data);
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
            result.Message = "Success insert item category.";
            return result;
        }

        public override SaveResult Update(ItemCategory data)
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
                Db.ItemCategories.Update(data);
                Db.Entry(data).Property(e => e.Id).IsModified = false;
                Db.Entry(data).Property(e => e.IsActive).IsModified = false;
                Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
                Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

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
            result.Message = "Success update item category.";
            return result;
        }

        public SaveResult Delete(int id, int userId)
        {
            var result = new SaveResult(false);

            var data = Db.ItemCategories.Find(id);
            if (data != null)
            {
                // Checking active
                if (!data.IsActive)
                {
                    result.Message = "Can't inactive the item category because data already inactive.";
                    return result;
                }

                // Update data
                data.IsActive = false;
                data.UpdatedBy = userId;
                data.UpdatedDate = DateTime.Now;

                Db.SaveChanges();
            }

            result.Success = true;
            result.Message = "Success inactive item category.";
            return result;
        }

        private bool IsInitialExists(string initial, int id)
        {
            return Db.ItemCategories.Any(x => x.Initial == initial && x.Id != id);
        }

        private static bool IsParent(List<ItemCategory> data, int? id = null)
        {
            return data.Any(x => x.ParentId == id);
        }

        private int? getSequence(int? id)
        {
            if (id.HasValue)
            {
                if (Db.ItemCategories.Any(x => x.ParentId == id))
                {
                    int? value = Db.ItemCategories.Where(x => x.ParentId == id).OrderByDescending(x => x.Seq).First().Seq;
                    return (value.HasValue) ? value : null;
                } else
                {
                    return null;
                }
            } else
            {
                return null;
            }
        }
    }
}
