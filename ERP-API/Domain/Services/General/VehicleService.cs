using ERP_API.Domain.Extensions;
using ERP_API.Domain.Interfaces.General;
using ERP_API.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ERP.Entity;
using ERP.Entity.General;

namespace ERP_API.Domain.Services.General
{
    public class VehicleService : GeneralService<Vehicle>, IVehicleService
    {
        public VehicleService(TenantContext db)
            : base(db)
        {

        }
        public SaveResult Delete(int id, int userId)
        {
            var result = new SaveResult(false);

            var data = Db.Vehicles.Find(id);
            if (data != null)
            {
                // Checking active
                if (data.IsActive == false)
                {
                    result.Message = "Tidak bisa menghapus data kendaraan karena data sudah dihapus.";
                    return result;
                }

                // Update data
                data.IsActive = false;
                data.UpdatedBy = userId;
                data.UpdatedDate = DateTime.Now;

                Db.SaveChanges();
            }

            result.Success = true;
            result.Message = "Data kendaraan berhasil dihapus.";
            return result;
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts, string search)
        {
            var data = Db.VwVehicles.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = data.Where(x =>
                        x.VehicleNo.Contains(search) || x.TypeName.Contains(search) || x.MaxLoadVolume.ToString().Contains(search) ||
                        x.MaxLoadWeight.ToString().Contains(search) || x.DriverId.ToString() == search || x.HelperId1.ToString() == search ||
                        x.HelperId2.ToString() == search);
            }

            return data.ToDataSourceResult(skip, take, filters, sorts);
        }

        public override SaveResult Insert(Vehicle data)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Checking vehicle no already exists or not
                if (IsVehicleNoExists(data.VehicleNo, 0))
                {
                    result.Message = "No. kendaraan sudah terdaftar. Tolong gunakan No. kendaraan lain.";
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
            result.Message = "Data kendaraan berhasil disimpan.";
            return result;
        }

        public override SaveResult Update(Vehicle data)
        {
            var result = new SaveResult(false);

            // Checking VehicleNo already exists or not
            if (IsVehicleNoExists(data.VehicleNo, data.Id))
            {
                result.Message = "No. kendaraan sudah terdaftar. Tolong gunakan No. kendaraan lain.";
                return result;
            }

            // Update data
            Db.Vehicles.Update(data);
            Db.Entry(data).Property(e => e.Id).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

            Db.SaveChanges();

            result.Success = true;
            result.Data = data.Id;
            result.Message = "Data kendaraan berhasil diperbarui.";
            return result;
        }

        private bool IsVehicleNoExists(string initial, int id)
        {
            return Db.Vehicles.Any(x => x.VehicleNo == initial && x.Id != id);
        }
    }
}
