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
    public class CustomersService : GeneralService<CustomersService>, ICustomersService
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

        public SaveResult Insert(Customer data)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                Db.Add(new Customer
                {
                    Code = data.Code,
                    Initial = data.Initial,
                    Name = data.Name,
                    TypeId = data.TypeId,
                    Address1 = data.Address1,
                    Address2 = data.Address2,
                    Phone = data.Phone,
                    Fax = data.Fax,
                    Email = data.Email,
                    Website = data.Website,
                    CreditTerm = data.CreditTerm,
                    CreditLimit = data.CreditLimit,
                    RefNo = data.RefNo,
                    Note = data.Note,
                    IsActive = data.IsActive,
                    CreatedBy = data.CreatedBy,
                    CreatedDate = data.CreatedDate,
                    UpdatedBy = data.UpdatedBy,
                    UpdatedDate = data.UpdatedDate
                }
                );

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

        public SaveResult ReverseUpdate(Customer data, params Expression<Func<Customer, object>>[] properties)
        {
            throw new NotImplementedException();
        }

        public SaveResult Update(Customer data)
        {
            throw new NotImplementedException();
        }

        public SaveResult Update(Customer data, params Expression<Func<Customer, object>>[] properties)
        {
            throw new NotImplementedException();
        }

        public SaveResult Update(Customer data, int userId)
        {
            var result = new SaveResult(false);

            var tdata = Db.Customers.Where(x => x.Code == data.Code).FirstOrDefault();

            if (tdata != null)
            {
                tdata.Name = data.Name;
                tdata.TypeId = data.TypeId;
                tdata.Address1 = data.Address1;
                tdata.Address2 = data.Address2;
                tdata.Phone = data.Phone;
                tdata.Fax = data.Fax;
                tdata.Email = data.Email;
                tdata.Website = data.Website;
                tdata.CreditTerm = data.CreditTerm;
                tdata.CreditLimit = data.CreditLimit;
                tdata.RefNo = data.RefNo;
                tdata.Note = data.Note;
                tdata.IsActive = data.IsActive;
                tdata.UpdatedBy = userId;
                tdata.UpdatedDate = DateTime.Now;

                Db.Customers.Update(tdata);

                Db.SaveChanges();

            }

            result.Success = true;
            result.Data = data.Code;
            result.Message = "Success update customer data.";
            return result;
        }
    }
}
