using System;
using System.Collections.Generic;
using System.Linq;
using ERP.Entity;
using ERP.Entity.AssetManagement;
using ERP_API.Domain.Extensions;
using ERP_API.Domain.Interfaces.AssetManagement;
using ERP_API.Domain.Models;

namespace ERP_API.Domain.Services.AssetManagement
{
    public class AssetTypeService : GeneralService<AssetType>, IAssetTypeService
    {
        public AssetTypeService(TenantContext db)
            : base(db)
        {
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, string search)
        {
            var data = Db.AssetTypes.Where(x=>x.IsActive).AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = data.Where(x =>
                            x.Initial.Contains(search) || x.Name.Contains(search) || 
                            x.CoaDeprecExpense.Contains(search) || 
                            x.CoaAccumDeprec.Contains(search) || 
                            x.CoaAsset.Contains(search) || x.CoaExpense.Contains(search));
            }

            return data.ToDataSourceResult(skip, take, filter, sort);
        }

        public DataSourceResult GetLists(IEnumerable<Filter> filters, IEnumerable<Sort> sorts)
        {
            var data = Db.AssetTypes.Where(x => x.IsActive);

            return data.ToDataSourceResult(0, -1, filters, sorts);
        }

        public override SaveResult Insert(AssetType data)
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

                // Insert data
                Db.AssetTypes.Add(data);

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
            result.Message = "Data tipe aktiva berhasil disimpan.";
            return result;
        }

        public override SaveResult Update(AssetType data)
        {
            var result = new SaveResult(false);

            // Checking initial already exists or not
            if (IsInitialExists(data.Initial, data.Id))
            {
                result.Message = "Inisial sudah terdaftar. Tolong gunakan inisial lain.";
                return result;
            }

            // Update data
            Db.AssetTypes.Update(data);
            Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

            Db.SaveChanges();

            result.Success = true;
            result.Data = data.Initial;
            result.Message = "Data tipe aktiva berhasil diperbarui.";
            return result;
        }

        public SaveResult Delete(int id)
        {
            var result = new SaveResult(false);

            var data = Db.AssetTypes.Find(id);
            if (data != null)
            {

                if (Db.FixedAssets.Where(x=>x.TypeId.Equals(id)).Any()) 
                {
                    result.Message = "Tidak bisa menghapus data karena sudah digunakan di transaksi.";
                    return result;
                }

                Db.AssetTypes.Remove(data);
                Db.SaveChanges();
            }

            result.Success = true;
            result.Message = "Data tipe aktiva berhasil dihapus.";
            return result;
        }

        public bool IsInitialExists(string initial, int id)
        {
            return Db.AssetTypes.Any(x => x.Initial == initial && x.Id != id);
        }

    }
}
