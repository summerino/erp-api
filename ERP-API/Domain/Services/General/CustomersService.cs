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

namespace ERP_API.Domain.Services.General
{
    public class CustomersService : GeneralService<Customer>, ICustomersService
    {
        public CustomersService(TenantContext db) : base(db)
        {

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

        public IEnumerable<Customer> GetAllCustomer()
        {
            List<Customer> result = new List<Customer>();

            result = Db.Customers.ToList();

            return result;
        }

        public Customer GetCustomers(string code)
        {
            return Db.Customers.Where(x => x.Code == code).FirstOrDefault();
        }
    }
}
