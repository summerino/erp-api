using System;
using System.Collections.Generic;
using System.Linq;
using ERP_API.Domain.Entities;
using ERP_API.Domain.Entities.General;
using ERP_API.Domain.Extensions;
using ERP_API.Domain.Interfaces.General;
using ERP_API.Domain.Models;
using Swift.Framework.Model;

namespace ERP_API.Domain.Services.General
{
    public class SupplierService : GeneralService<Supplier>, ISupplierService
    {
        public SupplierService(TenantContext db)
            : base(db)
        {
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            string search)
        {
            var data = Db.VwSuppliers.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = data.Where(x =>
                        x.Code.Contains(search) || x.Name.Contains(search) || x.TypeName.Contains(search) ||
                        x.Address1.Contains(search) || x.Phone.Contains(search) || x.Email.Contains(search));
            }

            return data.ToDataSourceResult(skip, take, filter, sort);
        }

        public DataSourceResult GetLists(IEnumerable<Filter> filters, IEnumerable<Sort> sorts)
        {
            var data = Db.Suppliers.Where(x => x.IsActive);

            return data.ToDataSourceResult(-1, -1, filters, sorts);
        }

        public override SaveResult Insert(Supplier data)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Checking initial already exists or not
                if (IsInitialExists(data.Initial, ""))
                {
                    result.Message = "Initial is already exists. Please use another initial.";
                    return result;
                }

                // Get new code
                var newCode = GetNewCode("SUP_NUM_FMT", data.CreatedDate);

                // Insert data
                data.Code = newCode;
                Db.Add(data);

                Db.SaveChanges();
                transaction.Commit();
            }
            catch (Exception ex)
            {
                result.Message = ex.InnerException?.Message ?? ex.Message;
                return result;
            }

            result.Success = true;
            result.Data = data.Code;
            result.Message = "Success insert supplier.";
            return result;
        }

        public override SaveResult Update(Supplier data)
        {
            var result = new SaveResult(false);

            // Checking initial already exists or not
            if (IsInitialExists(data.Initial, data.Code))
            {
                result.Message = "Initial is already exists. Please use another initial.";
                return result;
            }

            // Update data
            Db.Suppliers.Update(data);
            Db.Entry(data).Property(e => e.Code).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

            Db.SaveChanges();

            result.Success = true;
            result.Data = data.Code;
            result.Message = "Success update supplier.";
            return result;
        }

        public SaveResult Delete(string code, int userId)
        {
            var result = new SaveResult(false);

            var data = Db.Suppliers.Find(code);
            if (data != null)
            {
                // Checking active
                if (data.IsActive == false)
                {
                    result.Message = "Can't inactive supplier because data already inactive.";
                    return result;
                }

                // Update data
                data.IsActive = false;
                data.UpdatedBy = userId;
                data.UpdatedDate = DateTime.Now;

                Db.SaveChanges();
            }

            result.Success = true;
            result.Message = "Success inactive supplier.";
            return result;
        }

        private bool IsInitialExists(string initial, string code)
        {
            return Db.Customers.Any(x => x.Initial == initial && x.Code != code);
        }
    }
}
