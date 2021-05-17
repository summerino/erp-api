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
    public class TaxService : GeneralService<Tax>, ITaxService
    {
        public TaxService(TenantContext db)
            :base(db)
        {
                
        }
        public SaveResult Delete(int id, int userId)
        {
            var result = new SaveResult(false);

            var data = Db.Taxes.Find(id);
            if (data != null)
            {
                // Checking active
                if (data.IsActive == false)
                {
                    result.Message = "Tidak bisa menghapus data pajak karena data sudah dihapus.";
                    return result;
                }

                // Update data
                data.IsActive = false;
                data.UpdatedBy = userId;
                data.UpdatedDate = DateTime.Now;

                Db.SaveChanges();
            }

            result.Success = true;
            result.Message = "Data pajak berhasil dihapus.";
            return result;
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts, string search)
        {
            var data = Db.VwTaxes.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = data.Where(x =>
                        x.Initial.Contains(search) || x.Name.Contains(search) || x.CoaCode.Contains(search) ||
                        x.CoaName.Contains(search) || x.Rate.ToString() == search || x.Seq.ToString() == search);
            }

            return data.ToDataSourceResult(skip, take, filters, sorts);
        }

        public DataSourceResult GetLists(IEnumerable<Filter> filters, IEnumerable<Sort> sorts)
        {
            var data = Db.Taxes.Where(x => x.IsActive).OrderBy(x => x.Seq);

            return data.ToDataSourceResult(0, -1, filters, sorts);
        }

        public override SaveResult Insert(Tax data)
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
            result.Message = "Data pajak berhasil disimpan.";
            return result;
        }

        public override SaveResult Update(Tax data)
        {
            var result = new SaveResult(false);

            // Checking initial already exists or not
            if (IsInitialExists(data.Initial, data.Id))
            {
                result.Message = "Inisial sudah terdaftar. Tolong gunakan inisial lain.";
                return result;
            }

            // Update data
            Db.Taxes.Update(data);
            Db.Entry(data).Property(e => e.Id).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

            Db.SaveChanges();

            result.Success = true;
            result.Data = data.Id;
            result.Message = "Data pajak berhasil diperbarui.";
            return result;
        }

        private bool IsInitialExists(string initial, int id)
        {
            return Db.Taxes.Any(x => x.Initial == initial && x.Id != id);
        }
    }
}
