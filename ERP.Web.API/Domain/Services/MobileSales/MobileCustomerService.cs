using System;
using System.Collections.Generic;
using System.Linq;
using ERP.Common;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.General;
using ERP.Entity.MobileSales;
using ERP.Web.API.Domain.Interfaces.MobileSales;

namespace ERP.Web.API.Domain.Services.MobileSales
{
    public class MobileCustomerService : GeneralService<MobileCustomer>, IMobileCustomerService
    {
        public MobileCustomerService(TenantContext db)
            :base(db)
        {
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts, string search)
        {
            var data = Db.VwMobileCustomers.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = data.Where(x =>
                    x.Code.Contains(search) || x.Initial.Contains(search) || x.Name.Contains(search)
                    || x.InitialAddress.Contains(search) || x.Address1.Contains(search) || x.Address2.Contains(search)
                    || x.ContactPerson.Contains(search) || x.Phone.Contains(search) || x.Fax.Contains(search)
                    || x.Mark.Contains(search) || x.Status.Contains(search));
            }

            return data.ToDataSourceResult(skip, take, filters, sorts);
        }

        public override SaveResult Insert(MobileCustomer data)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Checking initial already exists or not
                if (IsInitialExists(data.Initial, data.Code))
                {
                    result.Message = "Inisial sudah terdaftar. Tolong gunakan inisial lain.";
                    return result;
                }

                Db.MobileCustomers.Add(data);
                Db.SaveChanges();

                transaction.Commit();
            }
            catch (Exception ex)
            {
                result.Message = ex.InnerException?.Message ?? ex.Message;
                return result;
            }

            result.Success = true;
            result.Data = data;
            result.Message = "Data pelanggan mobile berhasil disimpan.";
            return result;
        }

        public override SaveResult Update(MobileCustomer data)
        {
            var result = new SaveResult(false);

            // Update data
            Db.MobileCustomers.Update(data);
            Db.Entry(data).Property(e => e.Code).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

            Db.SaveChanges();

            result.Success = true;
            result.Data = data.Code;
            result.Message = "Data pelanggan mobile berhasil diperbarui.";
            return result;
        }

        public SaveResult Approve(List<MobileCustomer> data, int userId)
        {
            var result = new SaveResult(false);

            if (!data.Any())
                return new SaveResult(false, "Tidak ada data yang di proses");

            foreach (var item in data)
            {
                if (IsCustomerInitialExists(item.Initial))
                {
                    result.Message = $"Inisial sudah {item.Initial} terdaftar. Tolong gunakan inisial lain.";
                    return result;
                }

                var custCode = GetNewCode("CUST_NUM_FMT", item.CreatedDate);

                var custData = new Customer
                {
                    Code = custCode,
                    Initial = item.Initial,
                    Name = item.Name,
                    TypeId = item.TypeId,
                    PaymentTermId = 1,
                    RefNo = item.Code,
                    AreaId1 = item.AreaId1,
                    AreaId2 = item.AreaId2,
                    AreaId3 = item.AreaId3,
                    AreaId4 = item.AreaId4,
                    AreaId5 = item.AreaId5,
                    IsActive = true,
                    CreatedBy = item.CreatedBy,
                    CreatedDate = item.CreatedDate,
                    UpdatedBy = item.UpdatedBy,
                    UpdatedDate = item.UpdatedDate
                };

                Db.Customers.Add(custData);

                var custAdd = new CustomerAddress
                {
                    Code = custCode,
                    Initial = item.Initial,
                    Address1 = item.Address1,
                    Address2 = item.Address2,
                    ContactPerson = item.ContactPerson,
                    Phone = item.Phone,
                    Fax = item.Fax,
                    Lat = item.Lat,
                    Lng = item.Lng,
                    IsDefault = true
                };

                Db.CustomerAddress.Add(custAdd);

                Db.SaveChanges();

                custData.BillingAddressId = custAdd.Id;
                custData.ShippingAddressId = custAdd.Id;
                Db.Customers.Update(custData);

                var mcData = Db.MobileCustomers.FirstOrDefault(x => x.Code == item.Code);

                mcData.CustCode = custCode;
                mcData.Mark = "APR";
                mcData.UpdatedBy = userId;
                mcData.UpdatedDate = DateTime.Now;
                mcData.ApprovedBy = userId;
                mcData.ApprovedDate = mcData.UpdatedDate;
                Db.MobileCustomers.Update(mcData);
            }

            Db.SaveChanges();

            result.Success = true;
            result.Message = "Data pelanggan mobile berhasil disetujui.";
            return result;
        }

        public SaveResult Reject(List<MobileCustomer> data, int userId)
        {
            var result = new SaveResult(false);

            if (!data.Any())
                return new SaveResult(false, "Tidak ada data yang di proses");

            foreach (var item in data)
            {
                if (item.Mark == "REJ")
                {
                    result.Message = $"Data pelanggan {item.Name} tidak bisa ditolak karena dalam status ditolak.";
                    return result;
                }

                var mcData = Db.MobileCustomers.FirstOrDefault(x => x.Code == item.Code);
                mcData.RejectedBy = userId;
                mcData.RejectedDate = DateTime.Now;
                mcData.Mark = "REJ";
                Db.MobileCustomers.Update(mcData);
            }

            Db.SaveChanges();

            result.Success = true;
            result.Message = "Data pelanggan mobile berhasil ditolak.";
            return result;
        }

        private bool IsInitialExists(string initial, string code)
        {
            return Db.MobileCustomers.Any(x => x.Initial == initial && x.Code != code);
        }

        private bool IsCustomerInitialExists(string initial)
        {
            return Db.Customers.Any(x => x.Initial == initial);
        }
    }
}
