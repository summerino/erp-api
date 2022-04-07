using ERP.Common;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.General;
using ERP.Web.API.Domain.Interfaces.General;

namespace ERP.Web.API.Domain.Services.General;

public class VehicleTypeService : GeneralService<VehicleType>, IVehicleTypeService
{
    public VehicleTypeService(TenantContext db)
        :base(db)
    {
    }

    public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts, string search)
    {
        var data = Db.VwVehicleTypes.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            data = data.Where(x =>
                x.Initial.Contains(search) || x.Name.Contains(search));
        }

        return data.ToDataSourceResult(skip, take, filters, sorts);
    }

    public DataSourceResult GetLists(IEnumerable<Filter> filters, IEnumerable<Sort> sorts)
    {
        var data = Db.VehicleTypes.Where(x => x.IsActive);

        return data.ToDataSourceResult(0, -1, filters, sorts);
    }

    public override SaveResult Insert(VehicleType data)
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
        result.Message = "Data tipe kendaraan berhasil disimpan.";
        return result;
    }

    public override SaveResult Update(VehicleType data)
    {
        var result = new SaveResult(false);

        // Checking initial already exists or not
        if (IsInitialExists(data.Initial, data.Id))
        {
            result.Message = "Inisial sudah terdaftar. Tolong gunakan inisial lain.";
            return result;
        }

        // Update data
        Db.VehicleTypes.Update(data);
        Db.Entry(data).Property(e => e.Id).IsModified = false;
        Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
        Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

        Db.SaveChanges();

        result.Success = true;
        result.Data = data.Id;
        result.Message = "Data tipe kendaraan berhasil diperbarui.";
        return result;
    }
    public SaveResult Delete(int id, int userId)
    {
        var result = new SaveResult(false);

        var data = Db.VehicleTypes.Find(id);
        if (data != null)
        {
            // Checking active
            if (data.IsActive == false)
            {
                result.Message = "Tidak bisa menghapus data tipe kendaraan karena data sudah dihapus.";
                return result;
            }

            if (Db.Vehicles.Any(x => x.TypeId == data.Id))
            {
                result.Message = "Tidak bisa menghapus data tipe kendaraan karena telah digunakan pada data kendaraan.";
                return result;
            }

            Db.VehicleTypes.Remove(data);

            Db.SaveChanges();
        }

        result.Success = true;
        result.Message = "Data tipe kendaraan berhasil dihapus.";
        return result;
    }

    private bool IsInitialExists(string initial, int id)
    {
        return Db.VehicleTypes.Any(x => x.Initial == initial && x.Id != id);
    }
}