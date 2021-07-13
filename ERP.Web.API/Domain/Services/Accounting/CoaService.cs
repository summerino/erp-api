using System;
using System.Collections.Generic;
using System.Linq;
using ERP.Common;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.Accounting;
using ERP.Web.API.Domain.Interfaces.Accounting;
using ERP.Web.API.Domain.Models;

namespace ERP.Web.API.Domain.Services.Accounting
{
    public class CoaService : GeneralService<Coa>, ICoaService
    {
        public CoaService(TenantContext db)
            : base(db)
        {
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            string search)
        {
            var data = Db.VwCoas.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = data.Where(x => x.Code.Contains(search) || x.Name.Contains(search));
            }

            return data.ToDataSourceResult(skip, take, filter, sort);
        }

        public DataSourceResult GetLists(IEnumerable<Filter> filters, IEnumerable<Sort> sorts)
        {
            var data = Db.Coas.Where(x => x.IsActive);

            return data.ToDataSourceResult(0, -1, filters, sorts);
        }
        
        public override SaveResult Insert(Coa data)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Checking code already exists or not
                if (IsCoaExists(data.Code, 0))
                {
                    result.Message = "Kode sudah terdaftar. Tolong gunakan kode lain.";
                    return result;
                }

                // Insert data
                if(data.ParentId != null)
                {
                    var dataParent = Db.Coas.FirstOrDefault(x => x.Id == data.ParentId);
                    data.Deep = dataParent.Deep == null ? 1 : dataParent.Deep + 1;
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
            result.Data = data.Code;
            result.Message = "Data akun berhasil disimpan.";
            return result;
        }

        public override SaveResult Update(Coa data)
        {
            var result = new SaveResult(false);

            // Checking code already exists or not
            if (IsCoaExists(data.Code, data.Id))
            {
                result.Message = "Kode sudah terdaftar. Tolong gunakan kode lain.";
                return result;
            }

            // Update data
            if (data.ParentId != null)
            {
                var dataParent = Db.Coas.FirstOrDefault(x => x.Id == data.ParentId);
                data.Deep = dataParent.Deep == null ? 1 : dataParent.Deep + 1;
            }
            Db.Coas.Update(data);            
            Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

            Db.SaveChanges();

            result.Success = true;
            result.Data = data.Code;
            result.Message = "Data akun berhasil diperbarui.";
            return result;
        }

        public SaveResult Delete(int id, int userId)
        {
            var result = new SaveResult(false);

            var data = Db.Coas.Find(id);
            if (data != null)
            {
                // Checking active
                if (data.IsActive == false)
                {
                    result.Message = "Tidak bisa menonaktifkan data akun karena data sudah nonaktif.";
                    return result;
                }

                //Check if any promo already using this coa
                if (Db.PromoHeaders.Any(x => x.CoaCost == data.Code))
                {
                    result.Message = "Tidak bisa menghapus data akun karena telah digunakan pada data promo.";
                    return result;
                }
                // Delete data
                Db.Coas.Remove(data);

                Db.SaveChanges();
            }

            result.Success = true;
            result.Message = "Data akun berhasil dinonaktifkan.";
            return result;
        }

        private bool IsCoaExists(string code, int id)
        {
            return Db.Coas.Any(x => x.Code == code && x.Id != id);
        }
    }
}
