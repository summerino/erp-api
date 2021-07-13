using ERP.Web.API.Domain.Interfaces.Accounting;
using ERP.Web.API.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERP.Common;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.Accounting;

namespace ERP.Web.API.Domain.Services.Accounting
{
    public class CoaTypeService : GeneralService<CoaType>, ICoaTypeService
    {
        public CoaTypeService(TenantContext db)
            : base(db)
        {
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts, string search)
        {
            var data = Db.CoaTypes.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = data.Where(x =>
                        x.Initial.Contains(search) || x.Name.Contains(search));
            }

            return data.ToDataSourceResult(skip, take, filters, sorts);
        }

        public override SaveResult Insert(CoaType data)
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
            result.Message = "Data tipe akun berhasil disimpan.";
            return result;
        }

        public override SaveResult Update(CoaType data)
        {
            var result = new SaveResult(false);

            // Checking initial already exists or not
            if (IsInitialExists(data.Initial, data.Id))
            {
                result.Message = "Inisial sudah terdaftar. Tolong gunakan inisial lain.";
                return result;
            }

            // Update data
            Db.CoaTypes.Update(data);
            Db.Entry(data).Property(e => e.Id).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

            Db.SaveChanges();

            result.Success = true;
            result.Data = data.Id;
            result.Message = "Data tipe akun berhasil diperbarui.";
            return result;
        }

        public SaveResult Delete(int id, int userId)
        {
            var result = new SaveResult(false);

            var data = Db.CoaTypes.Find(id);
            if (data != null)
            {
                // Checking active
                if (data.IsActive == false)
                {
                    result.Message = "Tidak bisa menghapus data tipe akun karena data sudah dihapus.";
                    return result;
                }

                //Check if any coa already using this type
                if (Db.Coas.Any(x => x.TypeId == data.Id))
                {
                    result.Message = "Tidak bisa menghapus data tipe akun karena telah digunakan pada data akun.";
                    return result;
                }

                Db.CoaTypes.Remove(data);

                Db.SaveChanges();
            }

            result.Success = true;
            result.Message = "Data tipe akun berhasil dihapus.";
            return result;
        }

        private bool IsInitialExists(string initial, int id)
        {
            return Db.CoaTypes.Any(x => x.Initial == initial && x.Id != id && x.IsActive == true);
        }
    }
}
