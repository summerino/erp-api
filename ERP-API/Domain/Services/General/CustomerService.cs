using System;
using System.Collections.Generic;
using System.Linq;
using ERP_API.Domain.Entities;
using ERP_API.Domain.Entities.General;
using ERP_API.Domain.Extensions;
using ERP_API.Domain.Interfaces.General;
using ERP_API.Domain.Models;
using ERP_API.Model.General;

namespace ERP_API.Domain.Services.General
{
    public class CustomerService : GeneralService<Customer>, ICustomerService
    {
        public CustomerService(TenantContext db)
            : base(db)
        {
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
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

        public DataSourceResult GetLists(IEnumerable<Filter> filters, IEnumerable<Sort> sorts)
        {
            var data = Db.VwCustomers.Where(x => x.IsActive);

            return data.ToDataSourceResult(0, -1, filters, sorts);
        }

        public IEnumerable<CustomerAddress> GetAddress(string code)
        {
            var data = Db.CustomerAddress.Where(x => x.Code == code);

            return data.OrderBy(x => x.Id);
        }

        public Customer FindByCode(string code)
        {
            return Db.Customers.Find(code);
        }

        public SaveResult Insert(CustomerRequest data)
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
                var newCode = GetNewCode("CUST_NUM_FMT", data.CreatedDate);

                // Insert data
                data.Code = newCode;
                Db.Customers.Add(data);

                // Get temporary billing & shipping
                var tempBillingId = data.BillingAddressId;
                var tempShippingId = data.ShippingAddressId;
                var tempBillingInitial = "";
                var tempShippingInitial = "";

                // Insert detail data
                foreach (var item in data.ItemDetails)
                {
                    // Checking initial already exists or not
                    if (IsInitialAddressExists(item.Initial, ""))
                    {
                        result.Message = "Initial address is already exists. Please use another initial on address list.";
                        return result;
                    }

                    // Check user's choice
                    if (tempBillingId == item.Id)
                    {
                        tempBillingInitial = item.Initial;
                    }

                    if (tempShippingId == item.Id)
                    {
                        tempShippingInitial = item.Initial;
                    }

                    Db.CustomerAddress.Add(new CustomerAddress
                    {
                        Code = newCode,
                        Initial = item.Initial,
                        Address1 = item.Address1,
                        Address2 = item.Address2,
                        ContactPerson = item.ContactPerson,
                        Phone = item.Phone,
                        Fax = item.Fax,
                        IsDefault = item.IsDefault
                    });
                }

                Db.SaveChanges();

                // Update billing & shipping address id
                data.BillingAddressId = GetIdAddress(tempBillingInitial, newCode);
                data.ShippingAddressId = GetIdAddress(tempShippingInitial, newCode);

                Db.Customers.Update(data);
                Db.Entry(data).Property(e => e.Code).IsModified = false;
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
            result.Data = data.Code;
            result.Message = "Success insert customer.";
            return result;
        }

        public SaveResult Update(CustomerRequest data)
        {
            var result = new SaveResult(false);

            // Checking initial already exists or not
            if (IsInitialExists(data.Initial, data.Code))
            {
                result.Message = "Initial is already exists. Please use another initial.";
                return result;
            }

            // Update data
            Db.Customers.Update(data);
            Db.Entry(data).Property(e => e.Code).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

            // Delete existing item detail
            var delDetails = Db.CustomerAddress
                    .Where(d => d.Code == data.Code && !data.ItemDetails.Select(x => x.Id).Contains(d.Id))
                    .ToList();

            Db.CustomerAddress.RemoveRange(delDetails);

            // Get temporary billing & shipping to prevent if user choose a new address
            var tempBillingId = data.BillingAddressId;
            var tempShippingId = data.ShippingAddressId;
            var tempBillingInitial = "";
            var tempShippingInitial = "";

            // Update detail data
            foreach (var item in data.ItemDetails)
            {
                // Checking initial already exists or not
                if (IsInitialAddressExists(item.Initial, item.Code))
                {
                    result.Message = "Initial address is already exists. Please use another initial on address list.";
                    return result;
                }

                if (item.Id < 0)
                {
                    // Check user's choice
                    if (tempBillingId == item.Id)
                    {
                        tempBillingInitial = item.Initial;
                    }

                    if (tempShippingId == item.Id)
                    {
                        tempShippingInitial = item.Initial;
                    }

                    Db.CustomerAddress.Add(new CustomerAddress
                    {
                        Code = data.Code,
                        Initial = item.Initial,
                        Address1 = item.Address1,
                        Address2 = item.Address2,
                        ContactPerson = item.ContactPerson,
                        Phone = item.Phone,
                        Fax = item.Fax,
                        IsDefault = item.IsDefault
                    });
                }
                else
                {
                    Db.CustomerAddress.Update(item);
                    Db.Entry(item).Property(e => e.Id).IsModified = false;
                    Db.Entry(item).Property(e => e.Code).IsModified = false;
                }
            }

            Db.SaveChanges();

            // Update billing & shipping address id
            if (data.BillingAddressId < 0)
            {
                data.BillingAddressId = GetIdAddress(tempBillingInitial, data.Code);
            }

            if (data.ShippingAddressId < 0)
            {
                data.ShippingAddressId = GetIdAddress(tempShippingInitial, data.Code);
            }

            // Update data
            Db.Customers.Update(data);
            Db.Entry(data).Property(e => e.Code).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;
            Db.SaveChanges();

            result.Success = true;
            result.Data = data.Code;
            result.Message = "Success update customer.";
            return result;
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
                    result.Message = "Can't inactive customer because data already inactive.";
                    return result;
                }

                // Update data
                data.IsActive = false;
                data.UpdatedBy = userId;
                data.UpdatedDate = DateTime.Now;

                Db.SaveChanges();
            }

            result.Success = true;
            result.Message = "Success inactive customer.";
            return result;
        }

        private bool IsInitialExists(string initial, string code)
        {
            return Db.Customers.Any(x => x.Initial == initial && x.Code != code);
        }

        private bool IsInitialAddressExists(string initial, string code)
        {
            return Db.CustomerAddress.Any(x => x.Initial == initial && x.Code != code);
        }

        private int GetIdAddress(string initial, string code)
        {
            var addrDetails = Db.CustomerAddress
                    .Where(d => d.Initial == initial && d.Code == code)
                    .ToList();

            return (addrDetails[0].Id > 0) ? addrDetails[0].Id : 0;
        }
    }
}
