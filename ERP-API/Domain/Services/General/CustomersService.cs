using System;
using System.Collections.Generic;
using System.Linq;
using ERP_API.Domain.Entities;
using ERP_API.Domain.Entities.General;
using ERP_API.Domain.Extensions;
using ERP_API.Domain.Interfaces.General;
using ERP_API.Domain.Models;
using ERP_API.Model;
using Swift.Framework.Model;

namespace ERP_API.Domain.Services.General
{
    public class CustomersService : GeneralService<Customer>, ICustomersService
    {
        public CustomersService(TenantContext db)
            : base(db)
        {
        }

        public DataSourceResult GetViewData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            string search)
        {
            var data = Db.VwCustomers.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = data.Where(x =>
                        x.Code.Contains(search) || x.Name.Contains(search) || x.TypeName.Contains(search) ||
                        x.Address1.Contains(search) || x.Phone.Contains(search) || x.CreditTerm.ToString() == search);
            }

            return data.ToDataSourceResult(skip, take, filter, sort);
        }
 
        public Customer FindByCode(string code)
        {
            return Db.Customers.Find(code);
        }

        public override SaveResult Insert(Customer data)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                if (IsInitialExists(data.Initial))
                {
                    result.Message = "Initial code is already in the database.";
                    return result;
                }

                // Get new code
                var newCode = GetNewCode("CUST_NUM_FMT", data.CreatedDate);

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
            result.Message = "Success insert customer data.";
            return result;
        }

        public override SaveResult Update(Customer data)
        {
            var result = new SaveResult(false);

            if (IsInitialExists(data.Initial))
            {
                result.Message = "Initial code is already in the database.";
                return result;
            }

            // Update data
            Db.Customers.Update(data);
            Db.Entry(data).Property(e => e.Code).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

            return base.Update(data);
        }

        public SaveResult Delete(string code, int userId)
        {
            var result = new SaveResult(false);

            var data = Db.Customers.Find(code);
            if (data != null)
            {
                // Checking active
                if (data.IsActive == false)
                {
                    result.Message = "Can't remove customer because data already inactive.";
                    return result;
                }

                data.IsActive = false;
                data.UpdatedBy = userId;
                data.UpdatedDate = DateTime.Now;

                Db.SaveChanges();
            }

            result.Success = true;
            result.Message = "Success remove customer.";
            return result;
        }

        public bool IsInitialExists(string initial)
        {
            return Db.Customers.Any(x => x.Initial == initial);
        }
    }
}
