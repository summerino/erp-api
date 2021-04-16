using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using ERP_API.Domain.Entities;
using ERP_API.Domain.Entities.Inventory;
using ERP_API.Domain.Extensions;
using ERP_API.Domain.Interfaces.Inventory;
using ERP_API.Domain.Models;
using ERP_API.Model;
using Swift.Framework.Model;

namespace ERP_API.Domain.Services.Inventory
{
    public class ItemService : GeneralService<Item>, IItemService
    {
        public ItemService(TenantContext db)
            : base(db)
        {
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, string search)
        {
            var data = Db.VwItems.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = data.Where(x =>
                            x.Initial.Contains(search) || x.Name.Contains(search) || 
                            x.UomInitial.Contains(search) || x.UomSellName.Contains(search) ||
                            x.UomBuyName.Contains(search) || x.CategoryName.Contains(search));
            }

            return data.ToDataSourceResult(skip, take, filter, sort);
        }

        public override SaveResult Insert(Item data)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                if (IsInitialExists(data.Initial))
                {
                    result.Message = "Initial is already in the database.";
                    return result;
                }

                // Insert data
                Db.Items.Add(data);

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
            result.Message = "Success insert item.";
            return result;
        }

        public override SaveResult Update(Item data)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Update data
                Db.Items.Update(data);

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
            result.Message = "Success update item.";
            return result;
        }

        public SaveResult Delete(string Initial, int userId)
        {
            var result = new SaveResult(false);

            var data = Db.Items.Find(Initial);
            if (data != null)
            {
                // Checking active header data
                if (!data.IsActive)
                {
                    result.Message = "Can't delete the item because data already deleted.";
                    return result;
                }

                // Update header data
                data.IsActive = false;
                data.UpdatedBy = userId;
                data.UpdatedDate = DateTime.Now;

                Db.SaveChanges();
            }

            result.Success = true;
            result.Message = "Success delete item.";
            return result;
        }

        public bool IsInitialExists(string initial)
        {
            return Db.Items.Any(x => x.Initial == initial);
        }
    }
}
