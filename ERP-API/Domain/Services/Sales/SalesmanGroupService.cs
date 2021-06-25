using ERP_API.Domain.Entities;
using ERP_API.Domain.Entities.Sales;
using ERP_API.Domain.Extensions;
using ERP_API.Domain.Interfaces.Sales;
using ERP_API.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_API.Domain.Services.Sales
{
    public class SalesmanGroupService : GeneralService<SalesmanGroup>, ISalesmanGroupService
    {
        public SalesmanGroupService(TenantContext db)
            :base(db)
        {

        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            List<int> category, string search)
        {
            var data = Db.VwSalesmanGroups.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = data.Where(x =>
                        x.Initial.Contains(search) || x.Name.Contains(search) || x.SupervisorInitial.Contains(search));
            }

            return data.ToDataSourceResult(skip, take, filter, sort);
        }

        public override SaveResult Insert(SalesmanGroup data)
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
            result.Message = "Data grup penjual berhasil disimpan.";
            return result;
        }

        public override SaveResult Update(SalesmanGroup data)
        {
            var result = new SaveResult(false);

            // Checking initial already exists or not
            if (IsInitialExists(data.Initial, data.Id))
            {
                result.Message = "Inisial sudah terdaftar. Tolong gunakan inisial lain.";
                return result;
            }

            // Update data
            Db.SalesmanGroups.Update(data);
            Db.Entry(data).Property(e => e.Id).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

            Db.SaveChanges();

            result.Success = true;
            result.Data = data.Id;
            result.Message = "Data grup penjual berhasil diperbarui.";
            return result;
        }

        public SaveResult Delete(int id, int userId)
        {
            var result = new SaveResult(false);

            var data = Db.SalesmanGroups.Find(id);
            if (data != null)
            {
                // Checking active
                if (data.IsActive == false)
                {
                    result.Message = "Tidak bisa menghapus data grup penjual karena data sudah dihapus.";
                    return result;
                }

                Db.SalesmanGroups.Remove(data);

                Db.SaveChanges();
            }

            result.Success = true;
            result.Message = "Data grup penjual berhasil dihapus.";
            return result;
        }

        private bool IsInitialExists(string initial, int id)
        {
            return Db.SalesmanGroups.Any(x => x.Initial == initial && x.Id != id && x.IsActive == true);
        }
    }
}
