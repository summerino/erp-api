using ERP.Common;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.Inventory;
using ERP.Web.API.Domain.Interfaces.Inventory;
using ERP.Web.API.Model.Inventory;

namespace ERP.Web.API.Domain.Services.Inventory;

public class UnitOfMeasurementService : GeneralService<UoM>, IUnitOfMeasurementService
{
    public UnitOfMeasurementService(TenantContext db)
        : base(db)
    {
    }

    public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, string search)
    {
        var data = Db.VwUoMs.Where(x => x.IsActive).AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            data = data.Where(x =>
                x.Initial.Contains(search) || x.Description.Contains(search) ||
                x.BaseUnit.Contains(search));
        }

        return data.ToDataSourceResult(skip, take, filter, sort);
    }

    public DataSourceResult GetLists(IEnumerable<Filter> filters, IEnumerable<Sort> sorts, string mobileLastSync)
    {
        var data = Db.UoMs.Where(x => x.IsActive);

        if (!string.IsNullOrEmpty(mobileLastSync))
        {
            switch (mobileLastSync.Length)
            {
                case 21:
                    mobileLastSync += "000";
                    break;
                case 22:
                    mobileLastSync += "00";
                    break;
                case 23:
                    mobileLastSync += "0";
                    break;
            }
            data = data.Where(x => x.UpdatedDate > DateTime.ParseExact(mobileLastSync, "yyyy-MM-ddTHH:mm:ss.ffff", null));
        }

        return data.ToDataSourceResult(0, -1, filters, sorts);
    }

    public IEnumerable<UoMConversion> GetDataConversion(int? uomId)
    {
        var data = Db.UoMConversions.AsQueryable();

        if (uomId.HasValue)
            data = data.Where(x => x.UomId == uomId);

        return data.OrderBy(x => x.UomId).ThenBy(x => x.Seq);
    }

    public SaveResult Insert(UnitOfMeasurementRequest data, int userId)
    {
        var result = new SaveResult(false);

        using var transaction = Db.Database.BeginTransaction();
        try
        {
            DateTime insertDate = DateTime.Now;
            // Checking initial already exists or not
            if (IsInitialExists(data.Initial, 0))
            {
                result.Message = "Inisial sudah terdaftar. Tolong gunakan inisial lain.";
                return result;
            }
            data.IsActive = true;
            data.CreatedBy = userId;
            data.CreatedDate = insertDate;
            data.UpdatedBy = userId;
            data.UpdatedDate = insertDate;

            // Insert data
            Db.UoMs.Add(data);
            Db.SaveChanges();

            foreach (var item in data.Details)
            {
                Db.UoMConversions.Add(new UoMConversion
                {
                    UomId = data.Id,
                    Conversion = item.Conversion,
                    IsBaseUnit = item.IsBaseUnit,
                    Seq = item.Seq,
                    UnitEquivalent = item.UnitEquivalent,
                    UnitToConvert = item.UnitToConvert
                });
            }

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
        result.Message = "Data satuan pengukuran berhasil disimpan.";
        return result;
    }

    public SaveResult Update(UnitOfMeasurementRequest data, int userId)
    {
        var result = new SaveResult(false);

        using var transaction = Db.Database.BeginTransaction();
        try
        {

            // Checking initial already exists or not
            if (IsInitialExists(data.Initial, data.Id))
            {
                result.Message = "Inisial sudah terdaftar. Tolong gunakan inisial lain.";
                return result;
            }

            DateTime updateDate = DateTime.Now;
            data.UpdatedBy = userId;
            data.UpdatedDate = updateDate;

            // Update header data
            Db.UoMs.Update(data);
            Db.Entry(data).Property(e => e.Id).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

            // Get detail data that exists in order before
            var delDetails = Db.UoMConversions
                .Where(d => d.UomId == data.Id && !data.Details.Select(x => x.Id).Contains(d.Id))
                .ToList();

            // Delete detail data that exists in order before
            Db.UoMConversions.RemoveRange(delDetails);

            // Update detail data
            foreach (var item in data.Details)
            {
                if (item.Id < 0)
                {
                    Db.UoMConversions.Add(new UoMConversion
                    {
                        UomId = data.Id,
                        Conversion = item.Conversion,
                        IsBaseUnit = item.IsBaseUnit,
                        Seq = item.Seq,
                        UnitEquivalent = item.UnitEquivalent,
                        UnitToConvert = item.UnitToConvert
                    });
                }
                else
                {
                    var uomConversion = Db.UoMConversions.FirstOrDefault(x => x.Id == item.Id);

                    uomConversion.UomId = data.Id;
                    uomConversion.Conversion = item.Conversion;
                    uomConversion.IsBaseUnit = item.IsBaseUnit;
                    uomConversion.Seq = item.Seq;
                    uomConversion.UnitEquivalent = item.UnitEquivalent;
                    uomConversion.UnitToConvert = item.UnitToConvert;

                    Db.UoMConversions.Update(uomConversion);
                    Db.Entry(uomConversion).Property(e => e.Id).IsModified = false;
                    Db.Entry(uomConversion).Property(e => e.UomId).IsModified = false;
                }
            }

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
        result.Message = "Data satuan pengukuran berhasil diperbarui.";
        return result;
    }

    public SaveResult Delete(int id, int userId)
    {
        var result = new SaveResult(false);

        var data = Db.UoMs.Find(id);
        if (data != null)
        {
            // Checking active
            if (!data.IsActive)
            {
                result.Message = "Tidak bisa menonaktifkan data satuan pengukuran karena data sudah nonaktif.";
                return result;
            }

            // Update data
            data.IsActive = false;
            data.UpdatedBy = userId;
            data.UpdatedDate = DateTime.Now;

            Db.SaveChanges();
        }

        result.Success = true;
        result.Message = "Data satuan pengukuran berhasil dinonaktifkan.";
        return result;
    }

    private bool IsInitialExists(string initial, int id)
    {
        var result = Db.UoMs.Any(x => x.Initial.ToLower().Equals(initial.ToLower()) && x.Id != id);
        return result;
    }
}