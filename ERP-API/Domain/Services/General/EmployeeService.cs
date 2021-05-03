using System;
using System.Collections.Generic;
using System.Linq;
using ERP_API.Domain.Entities;
using ERP_API.Domain.Entities.General;
using ERP_API.Domain.Extensions;
using ERP_API.Domain.Interfaces.General;
using ERP_API.Domain.Models;

namespace ERP_API.Domain.Services.General
{
    public class EmployeeService : GeneralService<Employee>, IEmployeeService
    {
        public EmployeeService(TenantContext db)
            : base(db)
        {
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            string search)
        {
            var data = Db.VwEmployees.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = search.ToLower() switch
                {
                    "male" => data.Where(x => x.Sex),
                    "female" => data.Where(x => !x.Sex),
                    _ => data.Where(x =>
                        x.Initial.Contains(search) || x.FirstName.Contains(search) || x.LastName.Contains(search) ||
                        x.Address1.Contains(search) || x.Phone.Contains(search))
                };
            }

            return data.ToDataSourceResult(skip, take, filter, sort);
        }

        public DataSourceResult GetLists(IEnumerable<Filter> filters, IEnumerable<Sort> sorts)
        {
            var data = Db.Employees.Where(x => x.IsActive);

            return data.ToDataSourceResult(0, -1, filters, sorts);
        }

        public override SaveResult Insert(Employee data)
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
            result.Data = data.Initial;
            result.Message = "Success insert employee.";
            return result;
        }

        public override SaveResult Update(Employee data)
        {
            var result = new SaveResult(false);

            // Checking initial already exists or not
            if (IsInitialExists(data.Initial, data.Id))
            {
                result.Message = "Initial is already exists. Please use another initial.";
                return result;
            }

            // Update data
            Db.Employees.Update(data);
            Db.Entry(data).Property(e => e.Id).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

            Db.SaveChanges();

            result.Success = true;
            result.Data = data.Initial;
            result.Message = "Success update employee.";
            return result;
        }

        public SaveResult Delete(long id, int userId)
        {
            var result = new SaveResult(false);

            var data = Db.Employees.Find(id);
            if (data != null)
            {
                // Checking active
                if (data.IsActive == false)
                {
                    result.Message = "Can't inactive employee because data already inactive.";
                    return result;
                }

                // Update data
                data.IsActive = false;
                data.UpdatedBy = userId;
                data.UpdatedDate = DateTime.Now;

                Db.SaveChanges();
            }

            result.Success = true;
            result.Message = "Success inactive employee.";
            return result;
        }

        private bool IsInitialExists(string initial, long id)
        {
            return Db.Employees.Any(x => x.Initial == initial && x.Id != id);
        }
    }
}
