using System;
using System.Collections.Generic;
using System.Linq;
using ERP_API.Domain.Entities;
using ERP_API.Domain.Entities.Inventory;
using ERP_API.Domain.Extensions;
using ERP_API.Domain.Interfaces.Inventory;
using ERP_API.Domain.Models;
using Swift.Framework.Model;

namespace ERP_API.Domain.Services.Inventory
{
    public class WarehouseService : GeneralService<Warehouse>, IWarehouseService
    {
        public WarehouseService(TenantContext db)
            : base(db)
        {
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            List<int> category, string search)
        {
            var data = Db.Warehouses.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = data.Where(x =>
                            x.Code.Contains(search) || x.Initial.Contains(search) || x.Name.Contains(search) || 
                            x.Address.Contains(search) || x.Phone.Contains(search));
            }

            return data.ToDataSourceResult(skip, take, filter, sort);
        }

        public DataSourceResult GetLists(IEnumerable<Filter> filters, IEnumerable<Sort> sorts)
        {
            var data = Db.Warehouses.Where(x => x.IsActive);

            return data.ToDataSourceResult(0, -1, filters, sorts);
        }

        public override SaveResult Insert(Warehouse data)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Checking initial already exists or not
                if (IsInitialExists(data.Initial, string.Empty))
                {
                    result.Message = "Initial is already exists. Please use another initial.";
                    return result;
                }

                // Get new code
                var newCode = GetNewCode("WHS_NUM_FMT", data.CreatedDate);

                // Insert data
                data.Code = newCode;

                // Set as Default Data
                if (data.IsDefault) 
                {
                    SetDefault(data.Code);    
                }

                // Insert data
                Db.Warehouses.Add(data);

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
            result.Message = "Success insert warehouse.";
            return result;
        }

        public override SaveResult Update(Warehouse data)
        {
            var result = new SaveResult(false);

            // Checking initial already exists or not
            if (IsInitialExists(data.Initial, data.Code))
            {
                result.Message = "Initial is already exists. Please use another initial.";
                return result;
            }
            

            // Update data
            Db.Warehouses.Update(data);
            Db.Entry(data).Property(e => e.Code).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

            Db.SaveChanges();

            // Set as Default Data
            if (data.IsDefault)
            {
                SetDefault(data.Code);
            }
            Db.SaveChanges();

            result.Success = true;
            result.Data = data.Initial;
            result.Message = "Success update warehouse.";
            return result;
        }

        public SaveResult Delete(string code, int userId)
        {
            var result = new SaveResult(false);

            var data = Db.Warehouses.Find(code);
            if (data != null)
            {
                // Checking active
                if (!data.IsActive)
                {
                    result.Message = "Can't inactive warehouse because data already inactive.";
                    return result;
                }

                // Update data
                data.IsActive = false;
                data.UpdatedBy = userId;
                data.UpdatedDate = DateTime.Now;

                Db.SaveChanges();
            }

            result.Success = true;
            result.Message = "Success inactive warehouse.";
            return result;
        }

        public bool IsInitialExists(string initial, string code)
        {
            return Db.Warehouses.Any(x => x.Initial == initial && x.Code != code);
        }

        public void SetDefault(string code) 
        {

            var allWarehouses = Db.Warehouses.ToList();
            foreach (var warehouse in allWarehouses)
            {
                warehouse.IsDefault = false;
            }

            var temp = allWarehouses.SingleOrDefault(x => x.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            if (temp != null) {
                temp.IsDefault = true;
            }
        }
    }
}
