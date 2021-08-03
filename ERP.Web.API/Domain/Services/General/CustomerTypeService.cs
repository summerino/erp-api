using System;
using System.Collections.Generic;
using System.Linq;
using ERP.Common;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.General;
using ERP.Web.API.Domain.Interfaces.General;

namespace ERP.Web.API.Domain.Services.General
{
    public class CustomerTypeService : GeneralService<CustomerType>, ICustomerTypeService
    {
        public CustomerTypeService(TenantContext db)
            : base(db)
        {
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts, string search)
        {
            var data = Db.VwCustomerTypes.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = data.Where(x =>
                        x.Initial.Contains(search) || x.Name.Contains(search));
            }

            return data.ToDataSourceResult(skip, take, filters, sorts);
        }

        public DataSourceResult GetLists(IEnumerable<Filter> filters, IEnumerable<Sort> sorts)
        {
            var data = Db.CustomerTypes.Where(x => x.IsActive);

            return data.ToDataSourceResult(0, -1, filters, sorts);
        }

        public override SaveResult Insert(CustomerType data)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Checking initial already exists or not
                if (IsInitialExists(data.Initial, 0))
                {
                    result.Message = "Inisial sudah terdaftar. Tolong gunakan inisial lain.";
                    return result;
                }

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
            result.Data = data.Id;
            result.Message = "Data tipe pelanggan berhasil disimpan.";
            return result;
        }

        public override SaveResult Update(CustomerType data)
        {
            var result = new SaveResult(false);

            // Checking initial already exists or not
            if (IsInitialExists(data.Initial, data.Id))
            {
                result.Message = "Inisial sudah terdaftar. Tolong gunakan inisial lain.";
                return result;
            }

            // Update data
            Db.CustomerTypes.Update(data);
            Db.Entry(data).Property(e => e.Id).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

            Db.SaveChanges();

            result.Success = true;
            result.Data = data.Id;
            result.Message = "Data tipe pelanggan berhasil diperbarui.";
            return result;
        }

        public SaveResult Delete(int id, int userId)
        {
            var result = new SaveResult(false);

            var data = Db.CustomerTypes.Find(id);
            if (data != null)
            {
                // Checking active
                if (data.IsActive == false)
                {
                    result.Message = "Tidak bisa menghapus data tipe pelanggan karena data sudah dihapus.";
                    return result;
                }

                //Check if any customer already using this type
                if (Db.Customers.Any(x=> x.TypeId == data.Id))
                {
                    result.Message = "Tidak bisa menghapus data tipe pelanggan karena telah digunakan pada data pelanggan.";
                    return result;
                }

                Db.CustomerTypes.Remove(data);                

                Db.SaveChanges();
            }

            result.Success = true;
            result.Message = "Data tipe pelanggan berhasil dihapus.";
            return result;
        }

        private bool IsInitialExists(string initial, int id)
        {
            return Db.CustomerTypes.Any(x => x.Initial == initial && x.Id != id && x.IsActive == true);
        }
    }
}
