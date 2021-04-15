using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERP_API.Domain.Entities.General;
using ERP_API.Domain.Entities;
using ERP_API.Domain.Interfaces.General;
using ERP_API.Domain.Models;
using ERP_API.Model;
using Swift.Framework.Model;
using System.Linq.Expressions;
using ERP_API.Domain.Extensions;

namespace ERP_API.Domain.Services.General
{
    public class CustomersService : GeneralService<Customer>, ICustomersService
    {
        public CustomersService(TenantContext db) : base(db)
        {

        }

        public DataSourceResult GetDataView(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
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

        public SaveResult Delete(string code,int userId)
        {
            var result = new SaveResult(false);
            var data = Db.Customers.Where(x => x.Code == code).FirstOrDefault();

            if(data != null)
            {
                if(data.IsActive == false)
                {
                    result.Message = "Can't remove customer because customer already inactive.";
                    return result;
                }
                data.IsActive = false;
                data.UpdatedBy = userId;
                data.UpdatedDate = DateTime.Now;
                Db.SaveChanges();
            }

            result.Success = true;
            result.Message = "Remove customer success.";
            return result;
        }
 
        public Customer GetCustomers(string code)
        {
            return Db.Customers.Where(x => x.Code == code).FirstOrDefault();
        }

        public override SaveResult Insert(Customer data)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                if (Db.Customers.Any(x => x.Initial.Contains(data.Initial)) == false)
                {
                    var newCode = GetNewCode("CUST_NUM_FMT", data.CreatedDate);
                    data.Code = newCode;
                    Db.Add(data);
                    Db.SaveChanges();
                    transaction.Commit();
                }
                else
                {
                    result.Message = "Initial code is already in the database";
                    return result;
                }
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
    }
}
